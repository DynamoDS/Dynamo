using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Reflection;
using ProtoFFI;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class AssemblyHandler
    {
        /// <summary>
        /// Scans a list of assemblies using a Reflection only load context to determine which nodes are in the assemblies
        /// </summary>
        /// <param name="assemblyPaths">List of dll paths that should be scanned</param>
        /// <returns></returns>
        internal static List<MdFileInfo> ScanAssemblies(List<string> assemblyPaths)
        {
            var paths = GetDefaultPaths();
            paths.AddRange(assemblyPaths);

            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);

            return Scan(assemblyPaths, mlc, resolver);
        }

        /// <summary>
        /// Scans a list of assemblies using a Reflection only load context to determine which nodes are in the assemblies
        /// </summary>
        /// <param name="assemblyPaths">List of dll paths that should be scanned</param>
        /// <param name="addtionalPathsToLoad">List of dll paths that should be added to the PathAssemblyResolver</param>
        /// <returns></returns>
        internal static List<MdFileInfo> ScanAssemblies(List<string> assemblyPaths, List<string> addtionalPathsToLoad)
        {
            var paths = GetDefaultPaths();
            paths.AddRange(assemblyPaths);
            paths.AddRange(addtionalPathsToLoad);

            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);

            return Scan(assemblyPaths, mlc, resolver);
        }

        private static List<MdFileInfo> Scan(List<string> assemblyPaths, MetadataLoadContext mlc, PathAssemblyResolver resolver)
        {
            var fileInfos = new List<MdFileInfo>();

            using (mlc)
            {
                var dynamoCoreAss = resolver.Resolve(mlc, new AssemblyName("DynamoCore"));
                var nodeModelType = dynamoCoreAss.GetType("Dynamo.Graph.Nodes.NodeModel");

                foreach (var path in assemblyPaths)
                {
                    try
                    {
                        Assembly asm = mlc.LoadFromAssemblyPath(path);
                        AssemblyName name = asm.GetName();

                        if (NodeModelAssemblyLoader.ContainsNodeModelSubTypeReflectionLoaded(asm, nodeModelType))
                        {
                            fileInfos.AddRange(FileInfosFromNodeModels(asm, nodeModelType));
                            continue;
                        }

                        // Getting ZT imports from CLRModuleTypes
                        DLLModule dllModule = null;
                        string extension = System.IO.Path.GetExtension(asm.Location).ToLower();
                        if (extension == ".dll" || extension == ".exe")
                        {
                            try
                            {
                                dllModule = DLLFFIHandler.GetModuleForInspection(asm);
                            }
                            catch { }
                        }

                        dllModule.ScanModule();
                        List<CLRModuleType> moduleTypes = CLRModuleType.GetTypes((CLRModuleType mtype) => { return mtype.Module == dllModule; });

                        foreach (var t in moduleTypes)
                        {
                            var tn = t.FullName;
                            try
                            {
                                var className = t.ClassNode.ClassName;
                                
                                // For some reason mscorelib sometimes gets pass to here, so filtering it away.
                                if (t.CLRType.Assembly.GetName().Name == typeof(object).Assembly.GetName().Name ||
                                    t.ClassNode.ClassAttributes.HiddenInLibrary) continue;

                                var ctorNodes = t.ClassNode.Procedures
                                    .Where(c => c.Kind == AstKind.Constructor)
                                    .Cast<ConstructorDefinitionNode>()
                                    .Where(c => c.Access == ProtoCore.CompilerDefinitions.AccessModifier.Public)
                                    .GroupBy(x => x.Name);

                                var functionNodes = t.ClassNode.Procedures
                                    .Where(c => c is FunctionDefinitionNode)
                                    .Cast<FunctionDefinitionNode>()
                                    .Where(f => !f.MethodAttributes.HiddenInLibrary &&
                                                !f.MethodAttributes.IsObsolete && f.Name != "_Dispose" &&
                                                !f.Name.StartsWith("get_") &&
                                                !f.Name.StartsWith("set_"))
                                    .GroupBy(x => x.Name);

                                var props = t.ClassNode.Variables.GroupBy(x=>x.Name);

                                fileInfos.AddRange(FileInfosFromAsscociativeNodeGroups(ctorNodes, className));
                                fileInfos.AddRange(FileInfosFromAsscociativeNodeGroups(functionNodes, className));
                                fileInfos.AddRange(FileInfosFromAsscociativeNodeGroups(props, className));

                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return fileInfos;
        }

        private static List<MdFileInfo> FileInfosFromAsscociativeNodeGroups(IEnumerable<IGrouping<string,AssociativeNode>> associativeNodes, string className)
        {
            var fileInfos = new List<MdFileInfo>();
            foreach (var group in associativeNodes)
            {
                if (group.Count() == 1)
                {
                    fileInfos.Add(MarkdownHandler.GetMdFileInfoFromAssociativeNode(group.FirstOrDefault(), className, false));
                    continue;
                }

                fileInfos.AddRange(group.Select(x => MarkdownHandler.GetMdFileInfoFromAssociativeNode(x, className, true)));
            }
            return fileInfos;
        }
        private static List<MdFileInfo> FileInfosFromNodeModels(Assembly asm, Type nodeModelType)
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

            var t1 = typesInAsm[0].AttributesFromReflectionContext();

            var nodeTypes = typesInAsm.
                Where(x => NodeModelAssemblyLoader.IsNodeSubTypeReflectionLoaded(x, nodeModelType)).
                Select(t => new TypeLoadData(t, t.AttributesFromReflectionContext())).
                ToList();

            foreach (var nodeType in nodeTypes)
            {
                if (nodeType is null ||
                    nodeType.IsDeprecated ||
                    nodeType.IsHidden)
                    continue;
                fileInfos.Add(MarkdownHandler.GetMdFileNameFromTypeLoadData(nodeType));
            }

            return fileInfos;
        }

        private static List<string> GetDefaultPaths()
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);
            var dynamoDlls = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnumerateFiles("*.dll").Select(x => x.FullName);

            var paths = new List<string>(runtimeAssemblies);
            paths.AddRange(dynamoDlls);

            return paths;
        }
    }
}
