using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Utilities;
using ProtoCore.AST;
using ProtoCore.Namespace;
using Symbol = Dynamo.Nodes.Symbol;

namespace Dynamo.Core
{
    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeManager : LogSourceBase, ICustomNodeSource
    {
        public CustomNodeManager(NodeFactory nodeFactory, MigrationManager migrationManager)
        {
            this.nodeFactory = nodeFactory;
            this.migrationManager = migrationManager;
        }

        #region Fields and properties

        private readonly OrderedSet<Guid> loadOrder = new OrderedSet<Guid>();

        private readonly Dictionary<Guid, CustomNodeDefinition> loadedCustomNodes =
            new Dictionary<Guid, CustomNodeDefinition>();

        private readonly Dictionary<Guid, CustomNodeWorkspaceModel> loadedWorkspaceModels =
            new Dictionary<Guid, CustomNodeWorkspaceModel>();

        private readonly NodeFactory nodeFactory;
        private readonly MigrationManager migrationManager;

        /// <summary>
        ///     CustomNodeDefinitions for all loaded custom nodes, in load order.
        /// </summary>
        public IEnumerable<CustomNodeDefinition> LoadedDefinitions
        {
            get { return loadOrder.Select(id => loadedCustomNodes[id]); }
        }

        /// <summary>
        ///     Registry of all NodeInfos corresponding to discovered custom nodes. These
        ///     custom nodes are not all necessarily initialized.
        /// </summary>
        public readonly Dictionary<Guid, CustomNodeInfo> NodeInfos = new Dictionary<Guid, CustomNodeInfo>();

        /// <summary>
        ///     All loaded custom node workspaces.
        /// </summary>
        public IEnumerable<CustomNodeWorkspaceModel> LoadedWorkspaces
        {
            get { return loadedWorkspaceModels.Values; }
        }

        /// <summary>
        /// Gets custom node workspace by a specified custom node ID
        /// </summary>
        /// <param name="customNodeId">Custom node ID of a requested workspace</param>
        /// <returns>Custom node workspace by a specified ID</returns>
        public CustomNodeWorkspaceModel GetWorkspaceById (Guid customNodeId)
        {
            return loadedWorkspaceModels.ContainsKey(customNodeId) ? loadedWorkspaceModels[customNodeId] : null;
        }

        #endregion

        /// <summary>
        ///     An event that is fired when a definition is updated
        /// </summary>
        public event Action<CustomNodeDefinition> DefinitionUpdated;
        protected virtual void OnDefinitionUpdated(CustomNodeDefinition obj)
        {
            var handler = DefinitionUpdated;
            if (handler != null) handler(obj);
        }

        /// <summary>
        ///     An event that is fired when new or updated info is available for
        ///     a custom node.
        /// </summary>
        public event Action<CustomNodeInfo> InfoUpdated;
        protected virtual void OnInfoUpdated(CustomNodeInfo obj)
        {
            var handler = InfoUpdated;
            if (handler != null) handler(obj);
        }

        /// <summary>
        ///     An event that is fired when a custom node is removed from Dynamo.
        /// </summary>
        public event Action<Guid> CustomNodeRemoved;
        protected virtual void OnCustomNodeRemoved(Guid functionId)
        {
            var handler = CustomNodeRemoved;
            if (handler != null) handler(functionId);
        }

        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="nickname">
        ///     Nickname for the custom node to be instantiated, used for error recovery if
        ///     the given id could not be found.
        /// </param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        public Function CreateCustomNodeInstance(
            Guid id, string nickname = null, bool isTestMode = false)
        {
            CustomNodeWorkspaceModel workspace;
            CustomNodeDefinition def;
            CustomNodeInfo info;
            // Try to get the definition, initializing the custom node if necessary
            if (TryGetFunctionDefinition(id, isTestMode, out def))
            {
                // Got the definition, proceed as planned.
                info = NodeInfos[id];
            }
            else
            {
                // Couldn't get the workspace with the given ID, try a nickname lookup instead.
                if (nickname != null && TryGetNodeInfo(nickname, out info))
                    return CreateCustomNodeInstance(info.FunctionId, nickname, isTestMode);
                
                // Couldn't find the workspace at all, prepare for a late initialization.
                Log(
                    Properties.Resources.UnableToCreateCustomNodeID + id + "\"",
                    WarningLevel.Moderate);
                info = new CustomNodeInfo(id, nickname ?? "", "", "", "");
            }

            if (def == null)
            {
                def = CustomNodeDefinition.MakeProxy(id, info.Name);
            }

            var node = new Function(def, info.Name, info.Description, info.Category);
            if (loadedWorkspaceModels.TryGetValue(id, out workspace))
                RegisterCustomNodeInstanceForUpdates(node, workspace);
            else
                RegisterCustomNodeInstanceForLateInitialization(node, id, nickname, isTestMode);

            return node;
        }

        private void RegisterCustomNodeInstanceForLateInitialization(Function node, Guid id, string nickname, bool isTestMode)
        {
            var disposed = false;
            Action<CustomNodeInfo> infoUpdatedHandler = null;
            infoUpdatedHandler = newInfo =>
            {
                if (newInfo.FunctionId == id || newInfo.Name == nickname)
                {
                    CustomNodeWorkspaceModel foundWorkspace;
                    if (TryGetFunctionWorkspace(newInfo.FunctionId, isTestMode, out foundWorkspace))
                    {
                        node.ResyncWithDefinition(foundWorkspace.CustomNodeDefinition);
                        RegisterCustomNodeInstanceForUpdates(node, foundWorkspace);
                        InfoUpdated -= infoUpdatedHandler;
                        disposed = true;
                    }
                }
            };
            InfoUpdated += infoUpdatedHandler;
            node.Disposed += (args) =>
            {
                if (!disposed)
                    InfoUpdated -= infoUpdatedHandler;
            };
        }

        private static void RegisterCustomNodeInstanceForUpdates(Function node, CustomNodeWorkspaceModel workspace)
        {
            Action defUpdatedHandler = () =>
            {
                node.ResyncWithDefinition(workspace.CustomNodeDefinition);
            };
            workspace.DefinitionUpdated += defUpdatedHandler;

            Action infoChangedHandler = () =>
            {
                var info = workspace.CustomNodeInfo;
                node.NickName = info.Name;
                node.Description = info.Description;
                node.Category = info.Category;
            };
            workspace.InfoChanged += infoChangedHandler;
            node.Disposed += (args) =>
            {
                workspace.DefinitionUpdated -= defUpdatedHandler;
                workspace.InfoChanged -= infoChangedHandler;
            };
        }

        /// <summary> 
        ///     Get a function id from a guid assuming that the file is already loaded.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Guid GuidFromPath(string path)
        {
            var pair = NodeInfos.FirstOrDefault(x => x.Value.Path == path);
            return pair.Key;
        }

        private void SetFunctionDefinition(CustomNodeDefinition def)
        {
            var id = def.FunctionId;
            loadedCustomNodes[id] = def;
            loadOrder.Add(id);
        }
        
        private void SetPreloadFunctionDefinition(Guid id)
        {
            loadedCustomNodes[id] = null;
        }


        /// <summary>
        ///     Import a dyf file for eventual initialization.  
        /// </summary>
        /// <param name="file">Path to a custom node file on disk.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="info">
        ///     If the info was successfully processed, this parameter will be set to
        ///     it. Otherwise, it will be set to null.
        /// </param>
        /// <returns>True on success, false if the file could not be read properly.</returns>
        public bool AddUninitializedCustomNode(string file, bool isTestMode, out CustomNodeInfo info)
        {
            if (TryGetInfoFromPath(file, isTestMode, out info))
            {
                SetNodeInfo(info);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Attempts to remove all traces of a particular custom node from Dynamo, assuming the node is not in a loaded workspace.
        /// </summary>
        /// <param name="guid">Custom node identifier.</param>
        public void Remove(Guid guid)
        {
            Uninitialize(guid);
            NodeInfos.Remove(guid);
            OnCustomNodeRemoved(guid);
        }

        /// <summary>
        ///     Uninitialized a custom node. The information for the node is still retained, but the next time
        ///     the node is queried for it's workspace / definition / an instace it will be re-initialized from
        ///     disk.
        /// </summary>
        /// <param name="guid">Custom node identifier.</param>
        public bool Uninitialize(Guid guid)
        {
            CustomNodeWorkspaceModel ws;
            if (loadedWorkspaceModels.TryGetValue(guid, out ws))
            {
                ws.Dispose();
                loadedWorkspaceModels.Remove(guid);
                loadedCustomNodes.Remove(guid);
                loadOrder.Remove(guid);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Scans the given path for custom node files, retaining their information in the manager for later
        ///     potential initialization.
        /// </summary>
        /// <param name="path">Path on disk to scan for custom nodes.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="isPackageMember">
        ///     Indicates whether custom node comes from package or not.
        /// </param>
        /// <returns></returns>
        public IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(string path, bool isTestMode, bool isPackageMember = false)
        {
            var result = new List<CustomNodeInfo>();
            foreach (var info in ScanNodeHeadersInDirectory(path, isTestMode))
            {
                info.IsPackageMember = isPackageMember;

                SetNodeInfo(info);
                result.Add(info);
            }
            return result;
        }

        /// <summary>
        ///     Enumerates all of the files in the search path and get's their guids.
        ///     Does not instantiate the nodes.
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        private IEnumerable<CustomNodeInfo> ScanNodeHeadersInDirectory(string dir, bool isTestMode)
        {
            if (!Directory.Exists(dir))
                yield break;

            foreach (var file in Directory.EnumerateFiles(dir, "*.dyf"))
            {
                CustomNodeInfo info;
                if (TryGetInfoFromPath(file, isTestMode, out info))
                    yield return info;
            }
        }

        /// <summary>
        /// Stores the path and function definition without initializing a node.  Overwrites
        /// the existing NodeInfo if necessary
        /// </summary>
        private void SetNodeInfo(CustomNodeInfo newInfo)
        {
            NodeInfos[newInfo.FunctionId] = newInfo;
            OnInfoUpdated(newInfo);
        }

        /// <summary>
        ///     Get the function workspace from a guid
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="ws"></param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public bool TryGetFunctionWorkspace(Guid id, bool isTestMode, out CustomNodeWorkspaceModel ws)
        {
            if (Contains(id))
            {
                if (!loadedWorkspaceModels.TryGetValue(id, out ws))
                {
                    if (InitializeCustomNode(id, isTestMode, out ws))
                        return true;
                }
                else
                    return true;
            }
            ws = null;
            return false;
        }

        /// <summary>
        ///     Get the function definition from a guid.
        /// </summary>
        /// <param name="id">Custom node identifier.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public bool TryGetFunctionDefinition(Guid id, bool isTestMode, out CustomNodeDefinition definition)
        {
            if (Contains(id))
            {
                CustomNodeWorkspaceModel ws;
                if (IsInitialized(id) || InitializeCustomNode(id, isTestMode, out ws))
                {
                    definition = loadedCustomNodes[id];
                    return true;
                }
            }
            definition = null;
            return false;
        }
        
        /// <summary>
        ///     Tells whether the custom node's unique identifier is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="guid">The FunctionId</param>
        public bool Contains(Guid guid)
        {
            return IsInitialized(guid) || NodeInfos.ContainsKey(guid);
        }

        /// <summary>
        ///     Tells whether the custom node's name is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="name">The name of the custom node.</param>
        public bool Contains(string name)
        {
            CustomNodeInfo info;
            return TryGetNodeInfo(name, out info);
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="name">The name of the node</param>
        /// <returns>The name of the </returns>
        public bool IsInitialized(string name)
        {
            CustomNodeInfo info;
            return TryGetNodeInfo(name, out info) && IsInitialized(info.FunctionId);
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        public bool IsInitialized(Guid guid)
        {
            return loadedCustomNodes.ContainsKey(guid);
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="info"></param>
        /// <returns>The custom node info object - null if we failed</returns>
        public bool TryGetInfoFromPath(string path, bool isTestMode, out CustomNodeInfo info)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                WorkspaceInfo header;
                if (!WorkspaceInfo.FromXmlDocument(xmlDoc, path, isTestMode, false, AsLogger(), out header))
                {
                    Log(String.Format(Properties.Resources.FailedToLoadHeader, path));
                    info = null;
                    return false;
                }
                info = new CustomNodeInfo(
                    Guid.Parse(header.ID),
                    header.Name,
                    header.Category,
                    header.Description, 
                    path);
                return true;
            }
            catch (Exception e)
            {
                Log(String.Format(Properties.Resources.FailedToLoadHeader, path));
                Log(e.ToString());
                info = null;
                return false;
            }
        }

        /// <summary>
        ///     Opens a Custom Node workspace from an XmlDocument, given a pre-constructed WorkspaceInfo.
        /// </summary>
        /// <param name="xmlDoc">XmlDocument representing the parsed custom node file.</param>
        /// <param name="workspaceInfo">Workspace header describing the custom node file.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public bool OpenCustomNodeWorkspace(
            XmlDocument xmlDoc, WorkspaceInfo workspaceInfo, bool isTestMode, out WorkspaceModel workspace)
        {
            CustomNodeWorkspaceModel customNodeWorkspace;
            if (InitializeCustomNode(
                workspaceInfo,
                xmlDoc,
                out customNodeWorkspace))
            {
                workspace = customNodeWorkspace;
                return true;
            }
            workspace = null;
            return false;
        }

        private bool InitializeCustomNode(
            WorkspaceInfo workspaceInfo,
            XmlDocument xmlDoc, out CustomNodeWorkspaceModel workspace)
        {
            // Add custom node definition firstly so that a recursive
            // custom node won't recursively load itself.
            SetPreloadFunctionDefinition(Guid.Parse(workspaceInfo.ID));
 
            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, nodeFactory);
           
            var newWorkspace = new CustomNodeWorkspaceModel(
                nodeFactory,
                nodeGraph.Nodes,
                nodeGraph.Notes,
                nodeGraph.Annotations,                               
                workspaceInfo);

            
            RegisterCustomNodeWorkspace(newWorkspace);

            workspace = newWorkspace;
            return true;
        }

        private void RegisterCustomNodeWorkspace(CustomNodeWorkspaceModel newWorkspace)
        {
            RegisterCustomNodeWorkspace(
                newWorkspace,
                newWorkspace.CustomNodeInfo,
                newWorkspace.CustomNodeDefinition);
        }

        private void RegisterCustomNodeWorkspace(
            CustomNodeWorkspaceModel newWorkspace, CustomNodeInfo info, CustomNodeDefinition definition)
        {
            loadedWorkspaceModels[newWorkspace.CustomNodeId] = newWorkspace;

            SetFunctionDefinition(definition);
            OnDefinitionUpdated(definition);
            newWorkspace.DefinitionUpdated += () =>
            {
                var newDef = newWorkspace.CustomNodeDefinition;
                SetFunctionDefinition(newDef);
                OnDefinitionUpdated(newDef);
            };

            SetNodeInfo(info);
            newWorkspace.InfoChanged += () =>
            {
                var newInfo = newWorkspace.CustomNodeInfo;
                SetNodeInfo(newInfo);
                OnInfoUpdated(newInfo);
            };

            newWorkspace.FunctionIdChanged += oldGuid =>
            {
                loadedWorkspaceModels.Remove(oldGuid);
                loadedCustomNodes.Remove(oldGuid);
                loadOrder.Remove(oldGuid);
                loadedWorkspaceModels[newWorkspace.CustomNodeId] = newWorkspace;
            };
        }

        /// <summary>
        ///     Deserialize a function definition from a given path.  A side effect of this function is that
        ///     the node is added to the dictionary of loadedNodes.  
        /// </summary>
        /// <param name="functionId">The function guid we're currently loading</param>
        /// <param name="isTestMode"></param>
        /// <param name="workspace">The resultant function definition</param>
        /// <returns></returns>
        private bool InitializeCustomNode(Guid functionId, bool isTestMode, out CustomNodeWorkspaceModel workspace)
        {
            try
            {
                var customNodeInfo = NodeInfos[functionId];

                var xmlPath = customNodeInfo.Path;

                Log(String.Format(Properties.Resources.LoadingNodeDefinition, customNodeInfo, xmlPath));

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                WorkspaceInfo info;
                if (WorkspaceInfo.FromXmlDocument(
                    xmlDoc,
                    xmlPath,
                    isTestMode,
                    false,
                    AsLogger(),
                    out info) && info.IsCustomNodeWorkspace)
                {
                    info.ID = functionId.ToString();
                    if (migrationManager.ProcessWorkspace(info, xmlDoc, isTestMode, nodeFactory))
                    {
                        return InitializeCustomNode(info, xmlDoc, out workspace);
                    }
                }
                Log(string.Format(Properties.Resources.CustomNodeCouldNotBeInitialized, customNodeInfo.Name));
                workspace = null;
                return false;
            }
            catch (Exception ex)
            {
                Log(Properties.Resources.OpenWorkspaceError);
                Log(ex);

                if (isTestMode)
                    throw; // Rethrow for NUnit.

                workspace = null;
                return false;
            }
        }

        /// <summary>
        ///     Creates a new Custom Node in the manager.
        /// </summary>
        /// <param name="name">Name of the custom node.</param>
        /// <param name="category">Category for the custom node.</param>
        /// <param name="description">Description of the custom node.</param>
        /// <param name="functionId">
        ///     Optional identifier to be used for the custom node. By default, will make a new unique one.
        /// </param>
        /// <returns>Newly created Custom Node Workspace.</returns>
        public WorkspaceModel CreateCustomNode(string name, string category, string description, Guid? functionId = null)
        {
            var newId = functionId ?? Guid.NewGuid();

            var info = new WorkspaceInfo()
            {
                Name = name,
                Category = category,
                Description = description,
                X = 0,
                Y = 0,
                ID = newId.ToString(), 
                FileName = string.Empty
            };
            var workspace = new CustomNodeWorkspaceModel(info, nodeFactory);

            RegisterCustomNodeWorkspace(workspace);
            return workspace;
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }

        /// <summary>
        ///     Attempts to retrieve information for the given custom node identifier.
        /// </summary>
        /// <param name="id">Custom node identifier.</param>
        /// <param name="info"></param>
        /// <returns>Success or failure.</returns>
        public bool TryGetNodeInfo(Guid id, out CustomNodeInfo info)
        {
            return NodeInfos.TryGetValue(id, out info);
        }

        /// <summary>
        ///     Attempts to retrieve information for the given custom node name. If there are multiple
        ///     custom nodes matching the given name, this method will return any one of them.
        /// </summary>
        /// <param name="name">Name of a custom node.</param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryGetNodeInfo(string name, out CustomNodeInfo info)
        {
            info = NodeInfos.Values.FirstOrDefault(x => x.Name == name);
            return info != null;
        }

        /// <summary>
        ///     Collapse a set of nodes in a given workspace.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        /// <param name="currentWorkspace"> The workspace where</param>
        /// <param name="isTestMode"></param>
        /// <param name="args"></param>
        public CustomNodeWorkspaceModel Collapse(
            IEnumerable<NodeModel> selectedNodes, WorkspaceModel currentWorkspace,
            bool isTestMode, FunctionNamePromptEventArgs args)
        {
            var selectedNodeSet = new HashSet<NodeModel>(selectedNodes);
            // Note that undoable actions are only recorded for the "currentWorkspace", 
            // the nodes which get moved into "newNodeWorkspace" are not recorded for undo,
            // even in the new workspace. Their creations will simply be treated as part of
            // the opening of that new workspace (i.e. when a user opens a file, she will 
            // not expect the nodes that show up to be undoable).
            // 
            // After local nodes are moved into "newNodeWorkspace" as the result of 
            // conversion, if user performs an undo, new set of nodes will be created in 
            // "currentWorkspace" (not moving those nodes in the "newNodeWorkspace" back 
            // into "currentWorkspace"). In another word, undo recording is on a per-
            // workspace basis, it does not work across different workspaces.
            // 
            UndoRedoRecorder undoRecorder = currentWorkspace.UndoRecorder;

            CustomNodeWorkspaceModel newWorkspace;

            using (undoRecorder.BeginActionGroup())
            {
                #region Determine Inputs and Outputs

                //Step 1: determine which nodes will be inputs to the new node
                var inputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.InPortData.Count)
                                .Where(node.HasConnectedInput)
                                .Select(data => Tuple.Create(node, data, node.InputNodes[data]))
                                .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

                var outputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPortData.Count)
                                .Where(node.HasOutput)
                                .SelectMany(
                                    data =>
                                        node.OutputNodes[data].Where(
                                            output => !selectedNodeSet.Contains(output.Item2))
                                        .Select(output => Tuple.Create(node, data, output)))));

                #endregion

                #region Detect 1-node holes (higher-order function extraction)

                Log(Properties.Resources.CouldNotRepairOneNodeHoles, WarningLevel.Mild);
                // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5603

                //var curriedNodeArgs =
                //    new HashSet<NodeModel>(
                //        inputs.Select(x => x.Item3.Item2)
                //            .Intersect(outputs.Select(x => x.Item3.Item2))).Select(
                //                outerNode =>
                //                {
                //                    //var node = new Apply1();
                //                    var node = newNodeWorkspace.AddNode<Apply1>();
                //                    node.SetNickNameFromAttribute();

                //                    node.DisableReporting();

                //                    node.X = outerNode.X;
                //                    node.Y = outerNode.Y;

                //                    //Fetch all input ports
                //                    // in order
                //                    // that have inputs
                //                    // and whose input comes from an inner node
                //                    List<int> inPortsConnected =
                //                        Enumerable.Range(0, outerNode.InPortData.Count)
                //                            .Where(
                //                                x =>
                //                                    outerNode.HasInput(x)
                //                                        && selectedNodeSet.Contains(
                //                                            outerNode.Inputs[x].Item2))
                //                            .ToList();

                //                    var nodeInputs =
                //                        outputs.Where(output => output.Item3.Item2 == outerNode)
                //                            .Select(
                //                                output =>
                //                                    new
                //                                    {
                //                                        InnerNodeInputSender = output.Item1,
                //                                        OuterNodeInPortData = output.Item3.Item1
                //                                    })
                //                            .ToList();

                //                    nodeInputs.ForEach(_ => node.AddInput());

                //                    node.RegisterAllPorts();

                //                    return
                //                        new
                //                        {
                //                            OuterNode = outerNode,
                //                            InnerNode = node,
                //                            Outputs =
                //                                inputs.Where(
                //                                    input => input.Item3.Item2 == outerNode)
                //                                    .Select(input => input.Item3.Item1),
                //                            Inputs = nodeInputs,
                //                            OuterNodePortDataList = inPortsConnected
                //                        };
                //                }).ToList();

                #endregion

                #region UI Positioning Calculations

                double avgX = selectedNodeSet.Average(node => node.X);
                double avgY = selectedNodeSet.Average(node => node.Y);

                double leftMost = selectedNodeSet.Min(node => node.X);
                double topMost = selectedNodeSet.Min(node => node.Y);
                double rightMost = selectedNodeSet.Max(node => node.X + node.Width);

                double leftShift = leftMost - 250;

                #endregion

                #region Handle full selected connectors

                // Step 2: Determine all the connectors whose start/end owners are 
                // both in the selection set, and then move them from the current 
                // workspace into the new workspace.

                var fullySelectedConns = new HashSet<ConnectorModel>(
                    currentWorkspace.Connectors.Where(
                        conn =>
                        {
                            bool startSelected = selectedNodeSet.Contains(conn.Start.Owner);
                            bool endSelected = selectedNodeSet.Contains(conn.End.Owner);
                            return startSelected && endSelected;
                        }));

                foreach (var connector in fullySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(connector);
                    connector.Delete();
                }

                #endregion

                #region Handle partially selected connectors

                // Step 3: Partially selected connectors (either one of its start 
                // and end owners is in the selection) are to be destroyed.

                var partiallySelectedConns =
                    currentWorkspace.Connectors.Where(
                        conn =>
                            selectedNodeSet.Contains(conn.Start.Owner)
                                || selectedNodeSet.Contains(conn.End.Owner)).ToList();

                foreach (var connector in partiallySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(connector);
                    connector.Delete();
                }

                #endregion

                #region Transfer nodes and connectors to new workspace

                var newNodes = new List<NodeModel>();
                var newAnnotations = new List<AnnotationModel>();
            
                // Step 4: move all nodes to new workspace remove from old
                // PB: This could be more efficiently handled by a copy paste, but we
                // are preservering the node 
                foreach (var node in selectedNodeSet)
                {                   
                    undoRecorder.RecordDeletionForUndo(node);
                    currentWorkspace.RemoveNode(node);

                    // Assign a new guid to this node, otherwise when node is
                    // compiled to AST, literally it is still in global scope
                    // instead of in function scope.
                    node.GUID = Guid.NewGuid();
                    node.RenderPackages.Clear();

                    // shift nodes
                    node.X = node.X - leftShift;
                    node.Y = node.Y - topMost;
                 
                    newNodes.Add(node);
                }

                //Copy the group from newNodes
                foreach (var group in DynamoSelection.Instance.Selection.OfType<AnnotationModel>())
                {
                    undoRecorder.RecordDeletionForUndo(group);
                    currentWorkspace.RemoveGroup(group);

                    group.GUID = Guid.NewGuid();
                    group.SelectedModels = group.DeletedModelBases;
                    newAnnotations.Add(group);
                }


                foreach (var conn in fullySelectedConns)
                {
                    ConnectorModel.Make(conn.Start.Owner, conn.End.Owner, conn.Start.Index, conn.End.Index);
                }

                #endregion

                #region Process inputs

                var inConnectors = new List<Tuple<NodeModel, int>>();
                var uniqueInputSenders = new Dictionary<Tuple<NodeModel, int>, Symbol>();

                //Step 3: insert variables (reference step 1)
                foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
                {
                    int inputIndex = input.Item1;

                    NodeModel inputReceiverNode = input.Item2.Item1;
                    int inputReceiverData = input.Item2.Item2;

                    NodeModel inputNode = input.Item2.Item3.Item2;
                    int inputData = input.Item2.Item3.Item1;

                    Symbol node;

                    var key = Tuple.Create(inputNode, inputData);
                    if (uniqueInputSenders.ContainsKey(key))
                    {
                        node = uniqueInputSenders[key];
                    }
                    else
                    {
                        inConnectors.Add(Tuple.Create(inputNode, inputData));

                        node = new Symbol
                        {
                            InputSymbol = inputReceiverNode.InPortData[inputReceiverData].NickName,
                            X = 0
                        };

                        // Try to figure out the type of input of custom node 
                        // from the type of input of selected node. There are
                        // two kinds of nodes whose input type are available:
                        // function node and custom node. 
                        List<Library.TypedParameter> parameters = null;
                        if (inputReceiverNode is Function) 
                        {
                            var func = inputReceiverNode as Function; 
                            parameters =  func.Controller.Definition.Parameters.ToList(); 
                        }
                        else if (inputReceiverNode is DSFunctionBase)
                        {
                            var dsFunc = inputReceiverNode as DSFunctionBase;
                            parameters = dsFunc.Controller.Definition.Parameters.ToList(); 
                        }

                        // so the input of custom node has format 
                        //    input_var_name : type
                        if (parameters != null && parameters.Count() > inputReceiverData)
                        {
                            var typeName = parameters[inputReceiverData].DisplayTypeName;
                            if (!string.IsNullOrEmpty(typeName))
                            {
                                node.InputSymbol += " : " + typeName;
                            }
                        }

                        node.SetNickNameFromAttribute();
                        node.Y = inputIndex*(50 + node.Height);

                        uniqueInputSenders[key] = node;

                        newNodes.Add(node);
                    }

                    //var curriedNode = curriedNodeArgs.FirstOrDefault(x => x.OuterNode == inputNode);

                    //if (curriedNode == null)
                    //{
                    ConnectorModel.Make(node, inputReceiverNode, 0, inputReceiverData);
                    //}
                    //else
                    //{
                    //    //Connect it to the applier
                    //    newNodeWorkspace.AddConnection(node, curriedNode.InnerNode, 0, 0);

                    //    //Connect applier to the inner input receive
                    //    newNodeWorkspace.AddConnection(
                    //        curriedNode.InnerNode,
                    //        inputReceiverNode,
                    //        0,
                    //        inputReceiverData);
                    //}
                }

                #endregion

                #region Process outputs

                //List of all inner nodes to connect an output. Unique.
                var outportList = new List<Tuple<NodeModel, int>>();

                var outConnectors = new List<Tuple<NodeModel, int, int>>();

                int i = 0;
                if (outputs.Any())
                {
                    foreach (var output in outputs)
                    {
                        if (outportList.All(x => !(x.Item1 == output.Item1 && x.Item2 == output.Item2)))
                        {
                            NodeModel outputSenderNode = output.Item1;
                            int outputSenderData = output.Item2;

                            //NodeModel outputReceiverNode = output.Item3.Item2;

                            //if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                            //    continue;

                            outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                            //Create Symbol Node
                            var node = new Output
                            {
                                Symbol = outputSenderNode.OutPortData[outputSenderData].NickName,
                                X = rightMost + 75 - leftShift
                            };

                            node.Y = i*(50 + node.Height);

                            node.SetNickNameFromAttribute();

                            newNodes.Add(node);
                            ConnectorModel.Make(outputSenderNode, node, outputSenderData, 0);

                            i++;
                        }
                    }

                    //Connect outputs to new node
                    outConnectors.AddRange(
                        from output in outputs
                        let outputSenderNode = output.Item1
                        let outputSenderData = output.Item2
                        let outputReceiverData = output.Item3.Item1
                        let outputReceiverNode = output.Item3.Item2
                        select
                            Tuple.Create(
                                outputReceiverNode,
                                outportList.FindIndex(
                                    x => x.Item1 == outputSenderNode && x.Item2 == outputSenderData),
                                outputReceiverData));
                }
                else
                {
                    foreach (var hanging in
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPortData.Count)
                                .Where(port => !node.HasOutput(port))
                                .Select(port => new { node, port })).Distinct())
                    {
                        //Create Symbol Node
                        var node = new Output
                        {
                            Symbol = hanging.node.OutPortData[hanging.port].NickName,
                            X = rightMost + 75 - leftShift
                        };
                        node.Y = i*(50 + node.Height);
                        node.SetNickNameFromAttribute();

                        newNodes.Add(node);
                        ConnectorModel.Make(hanging.node, node, hanging.port, 0);

                        i++;
                    }
                }

                #endregion

                var newId = Guid.NewGuid();
                newWorkspace = new CustomNodeWorkspaceModel(
                    nodeFactory,
                    newNodes,
                    Enumerable.Empty<NoteModel>(),
                    newAnnotations,                
                    new WorkspaceInfo()
                    {
                        X = 0,
                        Y = 0,
                        Name = args.Name,
                        Category = args.Category,
                        Description = args.Description,
                        ID = newId.ToString(),
                        FileName = string.Empty
                    },
                    currentWorkspace.ElementResolver);

                newWorkspace.HasUnsavedChanges = true;

                RegisterCustomNodeWorkspace(newWorkspace);

                var collapsedNode = CreateCustomNodeInstance(newId, isTestMode: isTestMode);
                collapsedNode.X = avgX;
                collapsedNode.Y = avgY;
                currentWorkspace.AddNode(collapsedNode, centered: false);
                undoRecorder.RecordCreationForUndo(collapsedNode);

                foreach (var connector in
                    inConnectors.Select((x, idx) => new { node = x.Item1, from = x.Item2, to = idx })
                        .Select(
                            nodeTuple =>
                                ConnectorModel.Make(
                                    nodeTuple.node,
                                    collapsedNode,
                                    nodeTuple.@from,
                                    nodeTuple.to))
                        .Where(connector => connector != null))
                {
                    undoRecorder.RecordCreationForUndo(connector);
                }

                foreach (var connector in
                    outConnectors.Select(
                        nodeTuple =>
                            ConnectorModel.Make(
                                collapsedNode,
                                nodeTuple.Item1,
                                nodeTuple.Item2,
                                nodeTuple.Item3)).Where(connector => connector != null))
                {
                    undoRecorder.RecordCreationForUndo(connector);
                }
            }
            return newWorkspace;
        }

        internal IEnumerable<Guid> GetAllDependenciesGuids(CustomNodeDefinition def)
        {
            var idSet = new HashSet<Guid>();
            idSet.Add(def.FunctionId);

            while (true)
            {
                bool isUpdated = false;
                foreach (var d in this.LoadedDefinitions)
                {
                    if (d.Dependencies.Any(x => idSet.Contains(x.FunctionId)))
                        isUpdated = isUpdated || idSet.Add(d.FunctionId);
                }

                if (!isUpdated)
                    break;
            }

            return idSet;
        }
    }
}
