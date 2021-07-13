﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Library;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using ProtoFFI.Reflection;
using ProtoCore.Utils;
using ProtoFFI;
using static Dynamo.Engine.LibraryServices;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class AssemblyHandler
    {
        /// <summary>
        /// Scans a list of assemblies using a Reflection only load context to determine which nodes are in the assemblies.
        /// </summary>
        /// <param name="assemblyPaths">List of dll paths that should be scanned</param>
        /// <param name="additionalPathsToLoad">List of dll paths that should be added to the PathAssemblyResolver. This can be used when there are types in the assembly that depends on coming from an external assembly that are not part of the assembly paths</param>
        /// <returns></returns>
        internal static List<MdFileInfo> ScanAssemblies(IEnumerable<string> assemblyPaths, IEnumerable<string> additionalPathsToLoad = null)
        {
            var paths = GetDefaultPaths();
            paths.AddRange(assemblyPaths);
            if (additionalPathsToLoad != null)
            {
                paths.AddRange(additionalPathsToLoad);
            }

            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);

            return Scan(assemblyPaths, mlc, resolver);
        }

        private static List<MdFileInfo> Scan(IEnumerable<string> assemblyPaths, MetadataLoadContext mlc, PathAssemblyResolver resolver)
        {
            var fileInfos = new List<MdFileInfo>();
            var nodeSearchModel = new NodeSearchModel();
            var pathManager = new PathManager(new PathManagerParams());

            Console.WriteLine($"Starting scan of following assemblies: {string.Join(", ", assemblyPaths)}");

            using (mlc)
            {
                var dynamoCoreAss = resolver.Resolve(mlc, new AssemblyName("DynamoCore"));
                var nodeModelType = dynamoCoreAss.GetType("Dynamo.Graph.Nodes.NodeModel");

                var functionGroups = new Dictionary<string, Dictionary<string, FunctionGroup>>(new LibraryPathComparer());
                
                foreach (var path in assemblyPaths)
                {
                    try
                    {
                        var functionDescriptors = new List<FunctionDescriptor>();

                        if (new FileInfo(path).Extension == ".ds")
                        {
                            var dsDescriptors = GetFunctionDescriptorsFromDsFile(pathManager, path);
                            if (dsDescriptors != null)
                            {
                                functionDescriptors.AddRange(dsDescriptors);
                            }
                        }

                        else
                        {
                            Assembly asm = mlc.LoadFromAssemblyPath(path);
                            AssemblyName name = asm.GetName();

                            if (NodeModelAssemblyLoader.ContainsNodeModelSubTypeReflectionLoaded(asm, nodeModelType))
                            {
                                AddNodeModelsToSearchModel(asm, nodeModelType, nodeSearchModel);
                                continue;
                            }

                            var dllDescriptors = GetFunctionDescriptorsFromDll(pathManager, asm);
                            if (dllDescriptors != null)
                            {
                                functionDescriptors.AddRange(dllDescriptors);
                            }
                        }                  

                        foreach (var descriptor in functionDescriptors)
                        {
                            Dictionary<string, FunctionGroup> functionGroupDict;
                            if (!functionGroups.TryGetValue(descriptor.Assembly, out functionGroupDict))
                            {
                                functionGroupDict = new Dictionary<string, FunctionGroup>();
                                functionGroups[descriptor.Assembly] = functionGroupDict;
                            }

                            var qualifiedName = descriptor.QualifiedName;
                            FunctionGroup functionGroup;
                            if (!functionGroupDict.TryGetValue(qualifiedName, out functionGroup))
                            {
                                functionGroup = new FunctionGroup(qualifiedName);
                                functionGroupDict[qualifiedName] = functionGroup;
                            }
                            functionGroup.AddFunctionDescriptor(descriptor);
                        }
                    }
                    catch (Exception e)
                    {
                        CommandHandler.LogExceptionToConsole(e);
                        continue;
                    }
                }

                if (functionGroups.Count > 0)
                {
                    AddZeroTouchNodesToSearch(functionGroups.Values.SelectMany(x => x.Values.Select(g => g)), nodeSearchModel);
                }

                fileInfos.AddRange(FileInfosFromSearchModel(nodeSearchModel));
                Console.WriteLine($"{fileInfos.Count()} nodes found during scan");
            }

            return fileInfos;
        }

        private static List<FunctionDescriptor> GetFunctionDescriptorsFromDll(PathManager pathManager, Assembly asm)
        {
            var objectAsmName = typeof(object).Assembly.GetName().Name;
            var descriptors = new List<FunctionDescriptor>();

            string extension = System.IO.Path.GetExtension(asm.Location).ToLower();
            if (extension != ".dll" && extension != ".exe")
            {
                return new List<FunctionDescriptor>();
            }

            // Getting ZT imports from CLRModuleTypes
            var dllModule = new CLRDLLModule(asm.GetName().Name, asm);
            dllModule.ScanModule(true);
            List<CLRModuleType> moduleTypes = CLRModuleType.GetTypes((CLRModuleType mtype) => { return mtype.Module == dllModule; });

            var customizationFile = LibraryCustomizationServices.GetForAssembly(asm.Location, pathManager);

            foreach (var t in moduleTypes)
            {
                try
                {
                    var className = t.ClassNode.ClassName;
                    var externalLib = t.ClassNode.ExternLibName;

                    // For some reason mscorelib sometimes gets pass to here, so filtering it away.
                    if (t.CLRType.Assembly.GetName().Name == objectAsmName ||
                        t.ClassNode.ClassAttributes.HiddenInLibrary)
                    {
                        continue;
                    }
                        
                        

                    var ctorNodes = t.ClassNode.Procedures
                        .OfType<ConstructorDefinitionNode>()
                        .Where(c => c.Access == ProtoCore.CompilerDefinitions.AccessModifier.Public &&
                                    c.MethodAttributes != null ?
                                        !c.MethodAttributes.IsObsolete && !c.MethodAttributes.HiddenInLibrary :
                                        true)
                        .Cast<AssociativeNode>();

                    var functionNodes = t.ClassNode.Procedures
                        .OfType<FunctionDefinitionNode>()
                        .Where(f => !f.MethodAttributes.HiddenInLibrary &&
                                    !f.MethodAttributes.IsObsolete &&
                                    f.Name != "_Dispose")
                        .Cast<AssociativeNode>();

                    var associativeNodes = ctorNodes.Union(functionNodes);

                    foreach (var node in associativeNodes)
                    {
                        if (TryGetFunctionDescriptor(node, asm, asm.Location, className, customizationFile, out ReflectionFunctionDescriptor des))
                        {
                            descriptors.Add(des);
                        }
                    }
                }
                catch (Exception e)
                {
                    CommandHandler.LogExceptionToConsole(e);
                    continue;
                }
            }

            return descriptors;
        }

        private static IEnumerable<FunctionDescriptor> GetFunctionDescriptorsFromDsFile(PathManager pathManager, string dsFilePath)
        {
            var descriptors = new List<FunctionDescriptor>();
            //we need to put CLRModuleType into reflection mode because importing DS may import some CLR modules as a side effect.
            CLRModuleType.IsReflectionContext = true;
            var importModuleHandler = new ImportModuleHandler(new ProtoCore.Core(new ProtoCore.Options()));
            var dsCodeNode = importModuleHandler.Import(dsFilePath, "", "");
            var classNodes = dsCodeNode.CodeNode.Body.OfType<ClassDeclNode>();
            var customizationFile = LibraryCustomizationServices.GetForAssembly(dsFilePath, pathManager);

            //build up tuples of all functions and their owning classes
            var allFunctionTuples = new List<(string ClassName, AssociativeNode Procedure)>();
            foreach (var class_ in classNodes)
            {
                foreach(var procedure in class_.Procedures)
                {   //todo not sure if we should use className or class_.Name or ExternLibName... see 
                    //that generated output of Builtin.ds is now of the form List.FunctionName
                    allFunctionTuples.Add((ClassName: class_.ClassName, Procedure: procedure));
                }
            }
            //comine class methods and free functions
            allFunctionTuples.AddRange(dsCodeNode.CodeNode.Body.OfType<FunctionDefinitionNode>().Select(func=>(ClassName:"",Procedure:func as AssociativeNode)));
            foreach (var tuple in allFunctionTuples)
            {
                if (TryGetFunctionDescriptor(tuple.Procedure, null, dsFilePath, tuple.ClassName, customizationFile, out ReflectionFunctionDescriptor des))
                {
                    descriptors.Add(des);
                }
            }
            return descriptors;
        }

        private static void AddZeroTouchNodesToSearch(IEnumerable<FunctionGroup> functionGroups, NodeSearchModel nodeSearchModel)
        {
            foreach (var funcGroup in functionGroups)
            {
                AddZeroTouchNodeToSearch(funcGroup, nodeSearchModel);
            }
        }

        /// <summary>
        /// As we rely on search when generating markdown files and matching them with content from the DynamoDictionary
        /// we need to add all ZT nodes to the SearchModel.
        /// </summary>
        /// <param name="funcGroup"></param>
        /// <param name="nodeSearchModel"></param>
        private static void AddZeroTouchNodeToSearch(FunctionGroup funcGroup, NodeSearchModel nodeSearchModel)
        {
            foreach (var functionDescriptor in funcGroup.Functions)
            {
                if (functionDescriptor.IsVisibleInLibrary)
                {
                    nodeSearchModel.Add(new ZeroTouchSearchElement(functionDescriptor));
                }
            }
        }

        /// <summary>
        /// As we rely on search when generating markdown files and matching them with content from the DynamoDictionary
        /// we need to add all ZT nodes to the SearchModel.
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="nodeModelType"></param>
        /// <param name="searchModel"></param>
        private static void AddNodeModelsToSearchModel(Assembly asm, Type nodeModelType, NodeSearchModel searchModel)
        {
            System.Type[] typesInAsm = null;
            try
            {
                typesInAsm = asm.GetTypes();
            }
            // see https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly.gettypes?view=netframework-4.8#remarks
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }

            var nodeTypes = typesInAsm
                .Where(x => NodeModelAssemblyLoader.IsNodeSubTypeReflectionLoaded(x, nodeModelType))
                .Select(t => new TypeLoadData(t, t.GetAttributesFromReflectionContext().ToArray()))
                .Where(type => type != null && !type.IsDeprecated && !type.IsHidden)
                .ToList();

            foreach (var type in nodeTypes)
            {
                searchModel.Add(new NodeModelSearchElement(type, false));
            }
        }

        private static List<string> GetDefaultPaths()
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);
            var dynamoAssemblies = Program.DynamoDirectoryAssemblyPaths.Select(x => x.FullName);

            var paths = new List<string>(runtimeAssemblies);
            paths.AddRange(dynamoAssemblies);

            return paths;
        }

        private static bool TryGetFunctionDescriptor(
            AssociativeNode associativeNode, Assembly asm, string asmPath,
            string className, LibraryCustomization customization,
            out ReflectionFunctionDescriptor descriptor)
        {
            descriptor = null;
            string name = associativeNode.Name;
            if (CoreUtils.IsSetter(name) ||
                CoreUtils.IsDisposeMethod(name) ||
                CoreUtils.StartsWithDoubleUnderscores(name))
            {
                return false;
            }

            FunctionDescriptorParams functionParams = null;
            switch (associativeNode.Kind)
            {
                case AstKind.Constructor:
                    functionParams = FunctionParamsFromConstructor(associativeNode as ConstructorDefinitionNode, asmPath, className);
                    break;
                case AstKind.FunctionDefintion:
                    functionParams = FunctionParamsFromFunction(associativeNode as FunctionDefinitionNode, asmPath, className);
                    break;
                default:
                    break;
            }

            descriptor = new ReflectionFunctionDescriptor(functionParams, customization, asm);
            return true;
        }

        private static FunctionDescriptorParams FunctionParamsFromFunction(FunctionDefinitionNode funcDefinitionNode, string location, string className)
        {
            FunctionType type;
            var name = funcDefinitionNode.Name;

            if (CoreUtils.IsGetter(name))
            {
                type = funcDefinitionNode.IsStatic
                    ? FunctionType.StaticProperty
                    : FunctionType.InstanceProperty;

                string property;
                if (CoreUtils.TryGetPropertyName(name, out property))
                {
                    name = property;
                }
            }
            else
            {
                if (funcDefinitionNode.IsStatic)
                {
                    type = FunctionType.StaticMethod;
                }
                else
                {
                    type = FunctionType.InstanceMethod;
                }
            }


            var functionParams = new FunctionDescriptorParams
            {
                Assembly = location,
                ClassName = className,
                FunctionName = name,
                ReturnType = funcDefinitionNode.ReturnType,
                Parameters = BuildParameters(funcDefinitionNode.Signature),
                FunctionType = type,
            };

            return functionParams;
        }

        private static FunctionDescriptorParams FunctionParamsFromConstructor(ConstructorDefinitionNode constructorDefinitionNode, string asmLoc, string className)
        {
            var functionParams = new FunctionDescriptorParams
            {
                Assembly = asmLoc,
                ClassName = className,
                FunctionName = constructorDefinitionNode.Name,
                ReturnType = constructorDefinitionNode.ReturnType,
                Parameters = BuildParameters(constructorDefinitionNode.Signature),
                FunctionType = FunctionType.Constructor,
            };

            return functionParams;
        }

        private static List<TypedParameter> BuildParameters(ArgumentSignatureNode signatureNode)
        {
            var typeParams = new List<TypedParameter>();
            foreach (var arg in signatureNode.Arguments)
            {
                var name = arg.Name;

                // If we canont get the argument name from the Name property
                // we need to check its ASTKind and get it differently based on that.
                // So far only BinaryExpression and Identifier has come up, more can be added if necessary.
                if (string.IsNullOrEmpty(name))
                {
                    switch (arg.NameNode.Kind)
                    {
                        case AstKind.BinaryExpression:
                            name = ((BinaryExpressionNode)arg.NameNode).LeftNode.Name;
                            break;
                        case AstKind.Identifier:
                            name = arg.NameNode.Name;
                            break;
                    }
                }

                typeParams.Add(new TypedParameter(name));
            }

            return typeParams;
        }

        private static List<MdFileInfo> FileInfosFromSearchModel(NodeSearchModel nodeSearchModel)
        {
            var fileInfos = new List<MdFileInfo>();

            foreach (var entry in nodeSearchModel.SearchEntries)
            {
                if (MdFileInfo.TryGetMdFileInfoFromSearchEntry(entry,  out MdFileInfo info))
                {
                    fileInfos.Add(info);
                }              
            }

            return fileInfos;
        }
    }
}