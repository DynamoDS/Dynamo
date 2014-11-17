using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using DynamoUtilities;
using Dynamo.Core;

namespace Dynamo.TestInfrastructure
{
    [MutationTest("CustomNodeCompatibilityMutator")]
    class CustomNodeCompatibilityMutator : AbstractMutator
    {
        public CustomNodeCompatibilityMutator(DynamoViewModel viewModel)
            : base(viewModel)
        {
        }

        public override Type GetNodeType()
        {
            return typeof(Function);
        }

        public override int NumberOfLaunches
        {
            get { return 1; }
        }

        public override bool RunTest(NodeModel node, StreamWriter writer)
        {
            bool pass = false;

            var types = LoadAllTypesFromDynamoAssemblies();

            foreach (Type type in types)
            {
                string nodeName = GetName(type);

                var firstNodeConnectors = node.AllConnectors.ToList();

                double coordinatesX = node.X;
                double coordinatesY = node.Y;

                if (!string.IsNullOrEmpty(nodeName))
                {
                    DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        Guid guidNumber = Guid.NewGuid();

                        DynamoModel.CreateNodeCommand createCommand =
                            new DynamoModel.CreateNodeCommand(guidNumber, nodeName,
                                coordinatesX, coordinatesY, false, false);

                        DynamoViewModel.ExecuteCommand(createCommand);
                    }));

                    var valueMap = new Dictionary<Guid, String>();
                    foreach (ConnectorModel connector in firstNodeConnectors)
                    {
                        Guid guid = connector.Start.Owner.GUID;
                        Object data = connector.Start.Owner.GetValue(0).Data;
                        String val = data != null ? data.ToString() : "null";
                        valueMap.Add(guid, val);
                        writer.WriteLine(guid + " :: " + val);
                        writer.Flush();
                    }

                    int numberOfUndosNeeded = Mutate(node);
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoModel.UndoRedoCommand undoCommand =
                                new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);

                            DynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                    }
                    Thread.Sleep(100);

                    writer.WriteLine("### - undo complete");
                    writer.Flush();

                    DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoModel.RunCancelCommand runCancel =
                            new DynamoModel.RunCancelCommand(false, false);

                        DynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (DynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Beginning test of CustomNode");
                    if (node.OutPorts.Count > 0)
                    {
                        try
                        {
                            NodeModel nodeAfterUndo = DynamoViewModel.Model.Nodes.ToList().FirstOrDefault((t) =>
                            {
                                return (t.GUID == node.GUID);
                            });

                            if (nodeAfterUndo != null)
                            {
                                var firstNodeConnectorsAfterUndo = nodeAfterUndo.AllConnectors.ToList();
                                foreach (ConnectorModel connector in firstNodeConnectors)
                                {
                                    Guid guid = connector.Start.Owner.GUID;
                                    Object data = connector.Start.Owner.GetValue(0).Data;
                                    String val = data != null ? data.ToString() : "null";

                                    if (valueMap[guid] != val)
                                    {
                                        writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                        writer.WriteLine(node.GUID);

                                        writer.WriteLine("Was: " + val);
                                        writer.WriteLine("Should have been: " + valueMap[guid]);
                                        writer.Flush();
                                        return pass;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                            writer.Flush();
                            return pass;
                        }
                    }
                    writer.WriteLine("### - test of CustomNode complete");
                    writer.Flush();
                }
            }
            return pass = true;
        }

        public override int Mutate(NodeModel node)
        {
            NodeModel lastNode = DynamoModel.Nodes.ToList().Last();

            if (lastNode.OutPorts.Count > 0)
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.MakeConnectionCommand connectCmd1 =
                        new DynamoModel.MakeConnectionCommand(lastNode.GUID, 0, PortType.OUTPUT,
                            DynamoModel.MakeConnectionCommand.Mode.Begin);
                    DynamoModel.MakeConnectionCommand connectCmd2 =
                        new DynamoModel.MakeConnectionCommand(node.GUID, 0, PortType.INPUT,
                            DynamoModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connectCmd1);
                    DynamoViewModel.ExecuteCommand(connectCmd2);
                }));
            }
            else
            {
                DynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                {
                    DynamoModel.MakeConnectionCommand connectCmd1 =
                        new DynamoModel.MakeConnectionCommand(node.GUID, 0, PortType.OUTPUT,
                            DynamoModel.MakeConnectionCommand.Mode.Begin);
                    DynamoModel.MakeConnectionCommand connectCmd2 =
                        new DynamoModel.MakeConnectionCommand(lastNode.GUID, 0, PortType.INPUT,
                            DynamoModel.MakeConnectionCommand.Mode.End);

                    DynamoViewModel.ExecuteCommand(connectCmd1);
                    DynamoViewModel.ExecuteCommand(connectCmd2);
                }));
            }

            return 5;
        }

        #region Auxiliary methods

        public string GetName(Type type)
        {
            string name = string.Empty;
            var excludedTypeNames = new List<string>() { "Code Block", "Custom Node", "Compose Functions", "List.ForEach", "Build Sublists", "Apply Function" };

            var attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
            var attrs = type.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
            if (attribs.Length > 0)
            {
                if (!excludedTypeNames.Contains((attribs[0] as NodeNameAttribute).Name))
                    name = (attribs[0] as NodeNameAttribute).Name;

                if ((attrs != null) && attrs.Any())
                {
                    var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                    if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                    {
                        name = string.Empty;
                    }
                }
            }

            return name;
        }

        public List<Type> LoadAllTypesFromDynamoAssemblies()
        {
            var nodeTypes = new List<Type>();
            var loadedAssemblyNames = new HashSet<string>();
            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>();
            var allLoadedAssemblies = new Dictionary<string, Assembly>();

            // cache the loaded assembly information
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    allLoadedAssembliesByPath[assembly.Location] = assembly;
                    allLoadedAssemblies[assembly.FullName] = assembly;
                }
                catch { }
            }

            // find all the dlls registered in all search paths
            // and concatenate with all dlls in the current directory
            var allDynamoAssemblyPaths =
                DynamoPathManager.Instance.Nodes.SelectMany((path) =>
                    {
                        return (Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly));
                    }).ToList();

            // add the core assembly to get things like code block nodes and watches.
            allDynamoAssemblyPaths.Add(Path.Combine(DynamoPathManager.Instance.MainExecPath, "DynamoCore.dll"));

            var resolver = new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
            {
                Assembly result;
                allLoadedAssemblies.TryGetValue(args.Name, out result);
                return result;
            });

            foreach (var assemblyPath in allDynamoAssemblyPaths)
            {
                var fn = Path.GetFileName(assemblyPath);

                if (fn == null)
                    continue;

                // if the assembly has already been loaded, then
                // skip it, otherwise cache it.
                if (loadedAssemblyNames.Contains(fn))
                    continue;

                loadedAssemblyNames.Add(fn);

                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                {
                    List<Type> types = LoadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                    nodeTypes.AddRange(types);
                }
                else
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        allLoadedAssemblies[assembly.GetName().Name] = assembly;
                        List<Type> types = LoadNodesFromAssembly(assembly);
                        nodeTypes.AddRange(types);
                    }
                    catch (Exception e)
                    {
                        DynamoViewModel.Model.Logger.Log(e);
                    }
                }
            }

            return nodeTypes;
        }

        private List<Type> LoadNodesFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var searchViewModel = DynamoViewModel.Model.SearchModel;

            var types = new List<Type>();

            try
            {
                var loadedTypes = assembly.GetTypes();

                foreach (var t in loadedTypes)
                {
                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
                    var isDeprecated = t.GetCustomAttributes(typeof(NodeDeprecatedAttribute), true).Any();
                    var isMetaNode = t.GetCustomAttributes(typeof(IsMetaNodeAttribute), false).Any();
                    var isDSCompatible = t.GetCustomAttributes(typeof(IsDesignScriptCompatibleAttribute), true).Any();

                    bool isHidden = false;
                    var attrs = t.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
                    if (null != attrs && attrs.Any())
                    {
                        var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                        if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                        {
                            isHidden = true;
                        }
                    }

                    if (!Nodes.Utilities.IsNodeSubType(t) && 
                        t.Namespace != "Dynamo.Nodes") /*&& attribs.Length > 0*/
                        continue;

                    //if we are running in revit (or any context other than NONE) 
                    //use the DoNotLoadOnPlatforms attribute, 
                    
                    //if available, to discern whether we should load this type
                    if (!DynamoViewModel.Model.Context.Equals(Context.NONE))
                    {

                        object[] platformExclusionAttribs = 
                            t.GetCustomAttributes(typeof(DoNotLoadOnPlatformsAttribute), false);
                        if (platformExclusionAttribs.Length > 0)
                        {
                            string[] exclusions = 
                                (platformExclusionAttribs[0] as DoNotLoadOnPlatformsAttribute).Values;

                            //if the attribute's values contain the context stored on the controller
                            //then skip loading this type.

                            if (exclusions.Reverse().Any(e => e.Contains(DynamoViewModel.Model.Context)))
                                continue;

                            //utility was late for Vasari release, 
                            //but could be available with after-post RevitAPI.dll
                            if (t.Name.Equals("dynSkinCurveLoops"))
                            {
                                MethodInfo[] specialTypeStaticMethods = 
                                    t.GetMethods(BindingFlags.Static | BindingFlags.Public);
                                const string nameOfMethodCreate = "noSkinSolidMethod";
                                bool exclude = true;
                                foreach (MethodInfo m in specialTypeStaticMethods)
                                {
                                    if (m.Name == nameOfMethodCreate)
                                    {
                                        var argsM = new object[0];
                                        exclude = (bool)m.Invoke(null, argsM);
                                        break;
                                    }
                                }
                                if (exclude)
                                    continue;
                            }
                        }
                    }

                    string typeName;

                    if (attribs.Length > 0 && !isDeprecated && 
                        !isMetaNode && isDSCompatible && !isHidden)
                    {
                        searchViewModel.Add(t);
                        typeName = (attribs[0] as NodeNameAttribute).Name;
                    }
                    else
                        typeName = t.Name;

                    types.Add(t);

                    var data = new TypeLoadData(assembly, t);


                }
            }
            catch (Exception e)
            {
                DynamoViewModel.Model.Logger.Log("Could not load types.");
                DynamoViewModel.Model.Logger.Log(e);
            }

            return types;
        }

        #endregion
    }
}