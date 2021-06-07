using System;
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
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Reflection;
using ProtoCore.Utils;
using ProtoFFI;
using static Dynamo.Engine.LibraryServices;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class AssemblyHandler
    {
        /// <summary>
        /// Scans a list of assemblies using a Reflection only load context to determine which nodes are in the assemblies
        /// </summary>
        /// <param name="assemblyPaths">List of dll paths that should be scanned</param>
        /// <param name="additionalPathsToLoad">List of dll paths that should be added to the PathAssemblyResolver</param>
        /// <returns></returns>
        internal static List<MdFileInfo> ScanAssemblies(List<string> assemblyPaths, List<string> additionalPathsToLoad = null)
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

        private static List<MdFileInfo> Scan(List<string> assemblyPaths, MetadataLoadContext mlc, PathAssemblyResolver resolver)
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
                            var dsDescriptors = GetFunctionDescriptorsFromDs(pathManager, path);
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
                                fileInfos.AddRange(FileInfosFromNodeModels(asm, nodeModelType, ref nodeSearchModel));
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
                            Dictionary<string, FunctionGroup> fptrs;
                            if (!functionGroups.TryGetValue(descriptor.Assembly, out fptrs))
                            {
                                fptrs = new Dictionary<string, FunctionGroup>();
                                functionGroups[descriptor.Assembly] = fptrs;
                            }

                            string qualifiedName = descriptor.QualifiedName;
                            FunctionGroup functionGroup;
                            if (!fptrs.TryGetValue(qualifiedName, out functionGroup))
                            {
                                functionGroup = new FunctionGroup(qualifiedName);
                                fptrs[qualifiedName] = functionGroup;
                            }
                            functionGroup.AddFunctionDescriptor(descriptor);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                AddZeroTouchNodesToSearch(functionGroups.Values.SelectMany(x => x.Values.Select(g => g)), ref nodeSearchModel);
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
            DLLModule dllModule = null;
            dllModule = DLLFFIHandler.GetModuleForInspection(asm);

            dllModule.ScanModule();
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
                        t.ClassNode.ClassAttributes.HiddenInLibrary) continue;

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
                catch (Exception)
                {
                    continue;
                }
            }

            return descriptors;
        }

        private static List<FunctionDescriptor> GetFunctionDescriptorsFromDs(PathManager pathManager, string dsFilePath)
        {
            var descriptors = new List<FunctionDescriptor>();

            var t = new ImportModuleHandler(new ProtoCore.Core(new ProtoCore.Options()));
            var dsCodeNode = t.Import(dsFilePath, "", "");
            var classNode = dsCodeNode.CodeNode.Body.OfType<ClassDeclNode>().FirstOrDefault();
            var associativeNodes = classNode.Procedures;
            var customizationFile = LibraryCustomizationServices.GetForAssembly(dsFilePath, pathManager);
            foreach (var node in associativeNodes)
            {
                if (TryGetFunctionDescriptor(node, null, dsFilePath, classNode.ClassName, customizationFile, out ReflectionFunctionDescriptor des))
                {
                    descriptors.Add(des);
                }
            }
            return descriptors;
        }

        private static void AddZeroTouchNodesToSearch(IEnumerable<FunctionGroup> functionGroups, ref NodeSearchModel nodeSearchModel)
        {
            foreach (var funcGroup in functionGroups)
            {
                AddZeroTouchNodeToSearch(funcGroup, ref nodeSearchModel);
            }
        }

        private static void AddZeroTouchNodeToSearch(FunctionGroup funcGroup, ref NodeSearchModel nodeSearchModel)
        {
            foreach (var functionDescriptor in funcGroup.Functions)
            {
                if (functionDescriptor.IsVisibleInLibrary)
                {
                    nodeSearchModel.Add(new ReflectionZeroTouhSearchElement(functionDescriptor as ReflectionFunctionDescriptor));
                }
            }
        }

        private static List<MdFileInfo> FileInfosFromNodeModels(Assembly asm, Type nodeModelType, ref NodeSearchModel searchModel)
        {

            var fileInfos = new List<MdFileInfo>();
            System.Type[] typesInAsm = null;
            try
            {
                typesInAsm = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }

            var nodeTypes = typesInAsm
                .Where(x => NodeModelAssemblyLoader.IsNodeSubTypeReflectionLoaded(x, nodeModelType))
                .Select(t => new TypeLoadData(t, t.GetAttributesFromReflectionContext()))
                .Where(type => type != null && !type.IsDeprecated && !type.IsHidden)
                .ToList();

            foreach (var type in nodeTypes)
            {
                searchModel.Add(new NodeModelSearchElement(type, false));
            }

            return FileInfosFromSearchModel(searchModel);
        }

        private static List<string> GetDefaultPaths()
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);
            var dynamoDlls = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnumerateFiles("*.dll").Select(x => x.FullName);

            var paths = new List<string>(runtimeAssemblies);
            paths.AddRange(dynamoDlls);

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
                    name = property;
            }
            else
            {
                if (funcDefinitionNode.IsStatic)
                    type = FunctionType.StaticMethod;
                else
                    type = FunctionType.InstanceMethod;
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
                var name = !string.IsNullOrEmpty(arg.Name) ?
                    arg.Name :
                    arg.NameNode.Kind == AstKind.Identifier ?
                        arg.NameNode.Name :
                        arg.NameNode.Kind == AstKind.BinaryExpression ?
                            ((BinaryExpressionNode)arg.NameNode).LeftNode.Name :
                            string.Empty;

                typeParams.Add(new TypedParameter(name));
            }

            return typeParams;
        }

        private static List<MdFileInfo> FileInfosFromSearchModel(NodeSearchModel nodeSearchModel)
        {
            var fileInfos = new List<MdFileInfo>();

            foreach (var entry in nodeSearchModel.SearchEntries)
            {
                if (MdFileInfo.TryGetFromSearchEntry(entry,  out MdFileInfo info))
                {
                    fileInfos.Add(info);
                }              
            }

            return fileInfos;
        }
    }
}