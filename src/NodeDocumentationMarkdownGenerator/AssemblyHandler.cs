using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Autodesk.DesignScript.Runtime;
using ProtoFFI;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Reflection;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class AssemblyHandler
    {
        internal static List<MdFileInfo> ScanAssemblies(List<string> assemblyPaths)
        {
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);
            var dynamoDlls = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnumerateFiles("*.dll").Select(x => x.FullName);

            var paths = new List<string>(runtimeAssemblies);
            paths.AddRange(dynamoDlls);
            paths.AddRange(assemblyPaths);

            var fileInfos = new List<MdFileInfo>();
            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);

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
                            try
                            {
                                if (t.ClassNode.ClassAttributes.HiddenInLibrary)
                                    continue;

                                var ctorNodes = t.ClassNode.Procedures
                                    .Where(c => c is ConstructorDefinitionNode)
                                    .Cast<ConstructorDefinitionNode>()
                                    .Where(c => !c.MethodAttributes.HiddenInLibrary && !c.MethodAttributes.IsObsolete)
                                    .ToList();

                                var functionNodes = t.ClassNode.Procedures
                                    .Where(c => c is FunctionDefinitionNode)
                                    .Cast<FunctionDefinitionNode>()
                                    .Where(f => !f.MethodAttributes.HiddenInLibrary && !f.MethodAttributes.IsObsolete && f.Name != "_Dispose" && !f.Name.StartsWith("get_") && !f.Name.StartsWith("set_"))
                                    .ToList();

                                var props = t.ClassNode.Variables;

                                ctorNodes.ForEach(x => fileInfos.Add(MarkdownHandler.GetMdFileInfoFromAssociativeNode(x, t.ClassNode.ClassName, false)));
                                functionNodes.ForEach(x => fileInfos.Add(MarkdownHandler.GetMdFileInfoFromAssociativeNode(x, t.ClassNode.ClassName, false)));
                                props.ForEach(x => fileInfos.Add(MarkdownHandler.GetMdFileInfoFromAssociativeNode(x, t.ClassNode.ClassName, false)));

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
    }
}
