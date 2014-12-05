using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            NodeFactory = nodeFactory;
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

        public readonly NodeFactory NodeFactory;

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
        ///     Attempts to get the CustomNodeWorkspaceModel for a given Custom Node ID.
        /// </summary>
        /// <param name="guid">Unique identifier for the custom node.</param>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public bool TryGetCustomNodeWorkspace(Guid guid, out WorkspaceModel workspace)
        {
            CustomNodeWorkspaceModel customNodeWorkspace;
            if (!loadedWorkspaceModels.TryGetValue(guid, out customNodeWorkspace))
            {
                workspace = null;
                return false;
            }
            workspace = customNodeWorkspace;
            return true;
        }

        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="nickname"></param>
        /// <param name="isTestMode"></param>
        public Function CreateCustomNodeInstance(Guid id, string nickname=null, bool isTestMode=false)
        {
            CustomNodeDefinition def;
            if (!TryGetFunctionDefinition(id, isTestMode, out def))
            {
                CustomNodeInfo info;
                if (nickname == null || !TryGetNodeInfo(nickname, out info))
                {
                    Log(
                        "Unable to create instance of custom node with id: \"" + id + "\"",
                        WarningLevel.Error);
                    return null;
                }
                id = info.FunctionId;
                def = loadedCustomNodes[id] as CustomNodeDefinition;
            }

            var node = new Function(def);
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
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public bool TryGetFunctionDefinition(Guid id, bool isTestMode, out CustomNodeDefinition definition)
        {
            if (Contains(id))
            {
                CustomNodeWorkspaceModel ws;
                if (IsInitialized(id) || InitializeCustomNode(id, isTestMode, out ws))
                {
                    definition = loadedCustomNodes[id] as CustomNodeDefinition;
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

            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, NodeFactory);

            var newWorkspace = new CustomNodeWorkspaceModel(
                workspaceInfo.Name,
                workspaceInfo.Category,
                workspaceInfo.Description,
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
            var def = newWorkspace.CustomNodeDefinition;
            SetFunctionDefinition(def);
            OnDefinitionUpdated(def);
            newWorkspace.DefinitionUpdated += () =>
            {
                var newDef = newWorkspace.CustomNodeDefinition;
                SetFunctionDefinition(newDef);
                OnDefinitionUpdated(newDef);
            };

            SetNodeInfo(newWorkspace.CustomNodeInfo);
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
        /// <returns></returns>
        public WorkspaceModel CreateCustomNode(string name, string category, string description)
        {
            var newId = Guid.NewGuid();
            var workspace = new CustomNodeWorkspaceModel(name, category, description, 0, 0, newId);
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
    }
}
