using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Odbc;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using System.IO;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeManager : LogSourceBase
    {
        public CustomNodeManager(NodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        #region Fields and properties

        //TODO(Steve): We probably just want to store a list of guids to determine load order.
        private readonly OrderedDictionary loadedCustomNodes = new OrderedDictionary();
        private readonly Dictionary<Guid, CustomNodeWorkspaceModel> loadedWorkspaceModels =
            new Dictionary<Guid, CustomNodeWorkspaceModel>();

        /// <summary>
        ///     CustomNodeDefinitions for all loaded custom nodes, in load order.
        /// </summary>
        public IEnumerable<CustomNodeDefinition> LoadedDefinitions
        {
            get { return loadedCustomNodes.Values.Cast<CustomNodeDefinition>(); }
        }

        /// <summary>
        ///     Registry of all NodeInfos corresponding to discovered custom nodes. These
        ///     custom nodes are not all necessarily initialized.
        /// </summary>
        public readonly ObservableDictionary<Guid, CustomNodeInfo> NodeInfos =
            new ObservableDictionary<Guid, CustomNodeInfo>();

        /// <summary>
        ///     All loaded custom node workspaces.
        /// </summary>
        public IEnumerable<CustomNodeWorkspaceModel> Workspaces
        {
            get { return loadedWorkspaceModels.Values; }
        }

        private readonly NodeFactory nodeFactory;

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
        /// TODO
        /// </summary>
        public event Action<CustomNodeInfo> InfoUpdated;
        protected virtual void OnInfoUpdated(CustomNodeInfo obj)
        {
            var handler = InfoUpdated;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// TODO
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
        /// <param name="nickname"></param>
        /// <param name="isTestMode"></param>
        public Function CreateCustomNodeInstance(
            Guid id, string nickname = null, bool isTestMode = false)
        {
            CustomNodeWorkspaceModel workspace;
            CustomNodeDefinition def;
            CustomNodeInfo info;
            if (!TryGetFunctionWorkspace(id, isTestMode, out workspace))
            {
                if (nickname == null || !TryGetNodeInfo(nickname, out info))
                {
                    Log(
                        "Unable to create instance of custom node with id: \"" + id + "\"",
                        WarningLevel.Moderate);
                    info = new CustomNodeInfo(id, nickname ?? "", "", "", "");
                    def = null;
                }
                else
                {
                    id = info.FunctionId;
                    def = loadedCustomNodes[id] as CustomNodeDefinition;
                    workspace = loadedWorkspaceModels[id];
                }
            }
            else
            {
                
            }

            var node = new Function(def, info.Description, info.Category);
            Action<CustomNodeDefinition> defUpdatedHandler = definition =>
            {
                if (definition.FunctionId == id)
                    node.ResyncWithDefinition(definition);
            };
            DefinitionUpdated += defUpdatedHandler;
            node.Disposed += () => { DefinitionUpdated -= defUpdatedHandler; };

            return node;
        }

        /// <summary> 
        /// Get a function id from a guid assuming that the file is already loaded.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Guid GuidFromPath(string path)
        {
            var pair = NodeInfos.FirstOrDefault(x => x.Value.Path == path);
            return pair.Key;
        }
        
        /// <summary>
        ///     Manually add the CustomNodeDefinition to LoadedNodes, overwriting the existing CustomNodeDefinition
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public void SetFunctionDefinition(CustomNodeDefinition def)
        {
            var id = def.FunctionId;
            if (loadedCustomNodes.Contains(id))
            {
                loadedCustomNodes[id] = def;
            }
            else
            {
                loadedCustomNodes.Add(id, def);
            }
        }

        /// <summary>
        ///     Import a dyf file for eventual initialization.  
        /// </summary>
        /// <returns>null if we failed to get data from the path, otherwise the CustomNodeInfo object for the </returns>
        public void AddUninitializedCustomNode(string file, bool isTestMode)
        {
            CustomNodeInfo info;
            if (TryGetInfoFromPath(file, isTestMode, out info))
                SetNodeInfo(info);
        }

        /// <summary>
        ///     Attempts to remove all traces of a particular custom node from Dynamo, assuming the node is not in a loaded workspace.
        /// </summary>
        public void Remove(Guid guid)
        {
            if (loadedCustomNodes.Contains(guid))
                loadedCustomNodes.Remove(guid);

            NodeInfos.Remove(guid);
            loadedWorkspaceModels.Remove(guid);

            OnCustomNodeRemoved(guid);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isTestMode"></param>
        /// <returns></returns>
        public void AddUninitializedCustomNodesInPath(string path, bool isTestMode)
        {
            foreach (var info in ScanNodeHeadersInDirectory(path, isTestMode))
                SetNodeInfo(info);
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
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodePath(Guid id, string path)
        {
            CustomNodeInfo nodeInfo;
            if (TryGetNodeInfo(id, out nodeInfo))
            {
                nodeInfo.Path = path;
                OnInfoUpdated(nodeInfo);
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <param name="path"></param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public bool TryGetNodePath(Guid id, out string path)
        {
            CustomNodeInfo nodeInfo;
            if (TryGetNodeInfo(id, out nodeInfo))
            {
                path = nodeInfo.Path;
                return true;
            }
            path = null;
            return false;
        }

        /// <summary>
        ///     Get the function definition from a guid
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <param name="isTestMode"></param>
        /// <param name="ws"></param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public bool TryGetFunctionWorkspace(Guid id, bool isTestMode, out CustomNodeWorkspaceModel ws)
        {
            if (Contains(id))
            {
                if (IsInitialized(id))
                {
                    ws = loadedWorkspaceModels[id];
                    return true;
                }
                if (InitializeCustomNode(id, isTestMode, out ws))
                    return true;
            }
            ws = null;
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
            return loadedWorkspaceModels.ContainsKey(guid);
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="isTestMode"></param>
        /// <param name="info"></param>
        /// <returns>The custom node info object - null if we failed</returns>
        public bool TryGetInfoFromPath(string path, bool isTestMode, out CustomNodeInfo info)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                WorkspaceHeader header;
                if (!WorkspaceHeader.FromXmlDocument(xmlDoc, path, isTestMode, AsLogger(), out header))
                {
                    Log(
                        "ERROR: The header for the custom node at " + path
                            + " failed to load.  It will be left out of search.");
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
                Log(
                    "ERROR: The header for the custom node at " + path
                        + " failed to load.  It will be left out of search.");
                Log(e.ToString());
                info = null;
                return false;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="workspaceInfo"></param>
        /// <param name="isTestMode"></param>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public bool OpenCustomNodeWorkspace(
            XmlDocument xmlDoc, WorkspaceHeader workspaceInfo, bool isTestMode, out WorkspaceModel workspace)
        {
            CustomNodeWorkspaceModel customNodeWorkspace;
            if (InitializeCustomNode(
                Guid.Parse(workspaceInfo.ID),
                isTestMode,
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
            Guid functionId, bool isTestMode, WorkspaceHeader workspaceInfo,
            XmlDocument xmlDoc, out CustomNodeWorkspaceModel workspace)
        {
            // TODO(Steve): Refactor to remove dialogs. Results of various dialogs should be passed to this method.
            #region Migration

            Version fileVersion = MigrationManager.VersionFromString(workspaceInfo.Version);

            var currentVersion = AssemblyHelper.GetDynamoVersion(includeRevisionNumber: false);

            if (fileVersion > currentVersion)
            {
                bool resume = Utils.DisplayFutureFileMessage(
                    this.dynamoModel,
                    workspaceInfo.FileName,
                    fileVersion,
                    currentVersion);

                if (!resume)
                {
                    workspace = null;
                    return false;
                }
            }

            var decision = MigrationManager.ProcessWorkspace(
                xmlDoc,
                fileVersion,
                currentVersion,
                workspaceInfo.FileName,
                isTestMode,
                AsLogger());

            if (decision == MigrationManager.Decision.Abort)
            {
                Utils.DisplayObsoleteFileMessage(this.dynamoModel, workspaceInfo.FileName, fileVersion, currentVersion);

                workspace = null;
                return false;
            }

            #endregion

            // Add custom node definition firstly so that a recursive
            // custom node won't recursively load itself.
            SetFunctionDefinition(new CustomNodeDefinition(functionId));

            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, nodeFactory);

            var newWorkspace = new CustomNodeWorkspaceModel(
                workspaceInfo.Name,
                workspaceInfo.Category,
                workspaceInfo.Description,
                nodeFactory,
                nodeGraph.Nodes,
                nodeGraph.Connectors,
                nodeGraph.Notes,
                workspaceInfo.X,
                workspaceInfo.Y,
                functionId);
            
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
            SetFunctionDefinition(definition);
            OnDefinitionUpdated(definition);
            newWorkspace.DefinitionUpdated += () =>
            {
                var newDef = newWorkspace.CustomNodeDefinition;
                SetFunctionDefinition(newDef);
                OnDefinitionUpdated(newDef);
            };

            SetNodeInfo(info);
            newWorkspace.InfoChanged += () => OnInfoUpdated(newWorkspace.CustomNodeInfo);
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

                Log("Loading node definition for \"" + customNodeInfo + "\" from: " + xmlPath);

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                WorkspaceHeader header;
                if (!WorkspaceHeader.FromXmlDocument(xmlDoc, xmlPath, isTestMode, AsLogger(), out header)
                    || !header.IsCustomNodeWorkspace)
                {
                    Log(string.Format("Custom node \"{0}\" could not be initialized.", customNodeInfo.Name));
                    workspace = null;
                    return false;
                }

                return InitializeCustomNode(functionId, isTestMode, header, xmlDoc, out workspace);
            }
            catch (Exception ex)
            {
                Log("There was an error opening the workspace.");
                Log(ex);

                if (isTestMode)
                    throw; // Rethrow for NUnit.

                workspace = null;
                return false;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="functionId"></param>
        /// <returns></returns>
        public WorkspaceModel CreateCustomNode(string name, string category, string description, Guid? functionId = null)
        {
            var newId = functionId ?? Guid.NewGuid();
            var workspace = new CustomNodeWorkspaceModel(name, category, description, 0, 0, newId, nodeFactory);
            RegisterCustomNodeWorkspace(workspace);
            return workspace;
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryGetNodeInfo(Guid x, out CustomNodeInfo info)
        {
            return NodeInfos.TryGetValue(x, out info);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="name"></param>
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
        /// <param name="logger"></param>
        /// <param name="isTestMode"></param>
        /// <param name="args"></param>
        public WorkspaceModel Collapse(
            IEnumerable<NodeModel> selectedNodes, WorkspaceModel currentWorkspace, ILogger logger,
            bool isTestMode, FunctionNamePromptEventArgs args = null)
        {
            //TODO(Steve): Do this somewhere else, preferably after this has completed successfully.
            if (args == null || !args.Success)
            {
                args = new FunctionNamePromptEventArgs();
                dynamoModel.OnRequestsFunctionNamePrompt(null, args);

                if (!args.Success)
                {
                    return null;
                }
            }

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
                                .Select(data => Tuple.Create(node, data, node.Inputs[data]))
                                .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

                var outputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPortData.Count)
                                .Where(node.HasOutput)
                                .SelectMany(
                                    data =>
                                        node.Outputs[data].Where(
                                            output => !selectedNodeSet.Contains(output.Item2))
                                        .Select(output => Tuple.Create(node, data, output)))));

                #endregion

                #region Detect 1-node holes (higher-order function extraction)

                logger.LogWarning("Could not repair 1-node holes", WarningLevel.Mild);
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

                foreach (var ele in fullySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(ele);
                    currentWorkspace.Connectors.Remove(ele);
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

                foreach (ConnectorModel connector in partiallySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(connector);
                    connector.NotifyConnectedPortsOfDeletion();
                    currentWorkspace.Connectors.Remove(connector);
                }

                #endregion

                #region Transfer nodes and connectors to new workspace

                var newNodes = new List<NodeModel>();

                // Step 4: move all nodes to new workspace remove from old
                // PB: This could be more efficiently handled by a copy paste, but we
                // are preservering the node 
                foreach (var node in selectedNodeSet)
                {
                    undoRecorder.RecordDeletionForUndo(node);
                    currentWorkspace.Nodes.Remove(node);

                    // Assign a new guid to this node, otherwise when node is
                    // compiled to AST, literally it is still in global scope
                    // instead of in function scope.
                    node.GUID = Guid.NewGuid();
                    node.RenderPackages.Clear();

                    // shit nodes
                    node.X = node.X - leftShift;
                    node.Y = node.Y - topMost;

                    newNodes.Add(node);
                }

                var newConnectors = fullySelectedConns.ToList();

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

                        node.SetNickNameFromAttribute();
                        node.Y = inputIndex*(50 + node.Height);

                        uniqueInputSenders[key] = node;

                        newNodes.Add(node);
                    }

                    //var curriedNode = curriedNodeArgs.FirstOrDefault(x => x.OuterNode == inputNode);

                    //if (curriedNode == null)
                    //{
                    newConnectors.Add(ConnectorModel.Make(node, inputReceiverNode, 0, inputReceiverData));
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
                            newConnectors.Add(
                                ConnectorModel.Make(outputSenderNode, node, outputSenderData, 0));

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
                        newConnectors.Add(ConnectorModel.Make(hanging.node, node, hanging.port, 0));

                        i++;
                    }
                }

                #endregion

                var newId = Guid.NewGuid();
                newWorkspace = new CustomNodeWorkspaceModel(
                    args.Name,
                    args.Category,
                    args.Description,
                    nodeFactory,
                    newNodes,
                    newConnectors,
                    Enumerable.Empty<NoteModel>(),
                    0,
                    0,
                    newId);

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
                    currentWorkspace.AddConnection(connector);
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
                    currentWorkspace.AddConnection(connector);
                    undoRecorder.RecordCreationForUndo(connector);
                }
            }
            return newWorkspace;
        }
    }
}
