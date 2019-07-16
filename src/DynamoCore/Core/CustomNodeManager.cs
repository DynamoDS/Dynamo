using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using Dynamo.Exceptions;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Graph.Workspaces;
using Dynamo.Library;
using Dynamo.Logging;
using Dynamo.Migration;
using Dynamo.Models;
using Dynamo.Properties;
using Dynamo.Selection;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Symbol = Dynamo.Graph.Nodes.CustomNodes.Symbol;

namespace Dynamo.Core
{
    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeManager : LogSourceBase, ICustomNodeSource, ICustomNodeManager
    {
        /// <summary>
        /// This function creates CustomNodeManager
        /// </summary>
        /// <param name="nodeFactory">NodeFactory</param>
        /// <param name="migrationManager">MigrationManager</param>
        /// <param name="libraryServices">LibraryServices</param>
        public CustomNodeManager(NodeFactory nodeFactory, MigrationManager migrationManager, LibraryServices libraryServices)
        {
            this.nodeFactory = nodeFactory;
            this.migrationManager = migrationManager;
            this.libraryServices = libraryServices;
        }

        #region Fields and properties

        private LibraryServices libraryServices;

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
        /// Returns custom node workspace by a specified custom node ID
        /// </summary>
        /// <param name="customNodeId">Custom node ID of a requested workspace</param>
        /// <returns>Custom node workspace by a specified ID</returns>
        public CustomNodeWorkspaceModel GetWorkspaceById(Guid customNodeId)
        {
            return loadedWorkspaceModels.ContainsKey(customNodeId) ? loadedWorkspaceModels[customNodeId] : null;
        }

        #endregion

        #region Events

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

        internal event Func<Guid, PackageInfo> RequestCustomNodeOwner;

        private PackageInfo OnRequestCustomNodeOwner(Guid FunctionId)
        {
            return RequestCustomNodeOwner?.Invoke(FunctionId);
        }

        #endregion

        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="name">
        ///     Name for the custom node to be instantiated, used for error recovery if
        ///     the given id could not be found.
        /// </param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <returns>Custom Node Instance</returns>
        public Function CreateCustomNodeInstance(
            Guid id,
            string name = null,
            bool isTestMode = false)
        {
            CustomNodeDefinition def = null;
            CustomNodeInfo info = null;
            TryGetCustomNodeData(id, name, isTestMode, out def, out info);

            return CreateCustomNodeInstance(id, name, isTestMode, def, info);
        }

        /// <summary>
        ///     Attempts to get custom node info and definition data.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="name">
        ///     Name for the custom node to be instantiated, used for error recovery if
        ///     the given id could not be found.
        /// </param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="def">
        ///     Custom node definition data
        /// </param>
        /// <param name="info">
        ///     Custom node information data
        /// </param>
        public bool TryGetCustomNodeData(
            Guid id,
            string name,
            bool isTestMode,
            out CustomNodeDefinition def,
            out CustomNodeInfo info)
        {
            def = null;
            info = null;

            // Try to get the definition, initializing the custom node if necessary
            if (TryGetFunctionDefinition(id, isTestMode, out def))
            {
                // Got the definition, proceed as planned.
                info = NodeInfos[id];
                return true;
            }

            // Couldn't get the workspace with the given ID, try a name lookup instead.
            if (name != null && !TryGetNodeInfo(name, out info))
                return false;

            // Try to get the definition using the function ID, initializing the custom node if necessary
            if (info != null && TryGetFunctionDefinition(info.FunctionId, isTestMode, out def))
                return true;

            return false;
        }

        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="name">
        ///     Name for the custom node to be instantiated, used for error recovery if
        ///     the given id could not be found.
        /// </param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="def">
        ///     Custom node definition data
        /// </param>
        /// <param name="info">
        ///     Custom node information data
        /// </param>
        /// <returns>Custom Node Instance</returns>
        public Function CreateCustomNodeInstance(
            Guid id,
            string name,
            bool isTestMode,
            CustomNodeDefinition def,
            CustomNodeInfo info)
        {
            if (info == null)
            {
                // Couldn't find the workspace at all, prepare for a late initialization.
                Log(Properties.Resources.UnableToCreateCustomNodeID + id + "\"",
                    WarningLevel.Moderate);
                info = new CustomNodeInfo(id, name ?? "", "", "", "");
            }

            if (def == null)
                def = CustomNodeDefinition.MakeProxy(id, info.Name);

            var node = new Function(def, info.Name, info.Description, info.Category);

            CustomNodeWorkspaceModel workspace = null;
            if (loadedWorkspaceModels.TryGetValue(id, out workspace))
                RegisterCustomNodeInstanceForUpdates(node, workspace);
            else
                RegisterCustomNodeInstanceForLateInitialization(node, id, name, isTestMode);

            return node;
        }

        private void RegisterCustomNodeInstanceForLateInitialization(
            Function node,
            Guid id,
            string name,
            bool isTestMode)
        {
            var disposed = false;
            Action<CustomNodeInfo> infoUpdatedHandler = null;
            infoUpdatedHandler = newInfo =>
            {
                if (newInfo.FunctionId == id || newInfo.Name == name)
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
                node.Name = info.Name;
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
        ///     Returns a function id from a guid assuming that the file is already loaded.
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
        internal bool Uninitialize(Guid guid)
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
        ///     Scans the given path for custom node files, retaining their information in the manager for later
        ///     potential initialization. Should be used when packages load or reload customNodes.
        /// </summary>
        /// <param name="path">Path on disk to scan for custom nodes.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="PackageInfo">
        ///     Info about the package that requested this customNode to be loaded or to which the customNode belongs.
        ///     Is PackageMember property will be true if this property is not null.
        /// </param>
        /// <returns></returns>
        public IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(string path, bool isTestMode, PackageInfo packageInfo)
        {
            var result = new List<CustomNodeInfo>();
            foreach (var info in ScanNodeHeadersInDirectory(path, isTestMode))
            {
                info.IsPackageMember = true;
                info.PackageInfo = packageInfo;
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
            {
                Log(string.Format(Resources.InvalidCustomNodeFolderWarning, dir));
                yield break;
            }

            // Will throw exception if we don't have write access
            IEnumerable<string> dyfs;
            try
            {
                dyfs = Directory.EnumerateFiles(dir, "*.dyf");
            }
            catch (Exception e)
            {
                Log(string.Format(Resources.CustomNodeFolderLoadFailure, dir));
                Log(e);
                yield break;
            }

            foreach (var file in dyfs)
            {
                CustomNodeInfo info;
                if (TryGetInfoFromPath(file, isTestMode, out info))
                    yield return info;
            }

        }

        /// <summary>
        /// Stores the path and function definition without initializing a node.  
        /// Overwrites the existing NodeInfo if necessary!
        /// </summary>
        private void SetNodeInfo(CustomNodeInfo newInfo)
        {
            var guids = NodeInfos.Where(x =>
                        {
                            return !string.IsNullOrEmpty(x.Value.Path) &&
                                string.Compare(x.Value.Path, newInfo.Path, StringComparison.OrdinalIgnoreCase) == 0;
                        }).Select(x => x.Key).ToList();

           
            foreach (var guid in guids)
            {
                NodeInfos.Remove(guid);
            }

            // we need to check with the packageManager that this node if this node is in a package or not - 
            // currently the package data is lost when the customNode workspace is loaded.
            // we'll only do this check for customNode infos which don't have a package currently to verify if this
            // is correct.
            if(newInfo.IsPackageMember == false)
            {
                var owningPackage = this.OnRequestCustomNodeOwner(newInfo.FunctionId);
                
                //we found a real package.
                if(owningPackage != null)
                {
                    newInfo.IsPackageMember = true;
                    newInfo.PackageInfo = owningPackage;
                }
            }


            CustomNodeInfo info;
            // if the custom node is part of a package make sure it does not overwrite another node
            if (newInfo.IsPackageMember && NodeInfos.TryGetValue(newInfo.FunctionId, out info))
            {
                var newInfoPath = String.IsNullOrEmpty(newInfo.Path) ? string.Empty : Path.GetDirectoryName(newInfo.Path);
                var infoPath = String.IsNullOrEmpty(info.Path) ? string.Empty : Path.GetDirectoryName(info.Path);
                var message = string.Format(Resources.MessageCustomNodePackageFailedToLoad,
                    infoPath, newInfoPath);

               
                //only try to compare package info if both customNodeInfos have package info.
                if(info.IsPackageMember && info.PackageInfo != null)
                {
                    // if these are different packages raise an error.
                    // TODO (for now we don't raise an error for different
                    //versions of the same package, don't want to effect publish new version workflows.

                    if (newInfo.PackageInfo.Name != info.PackageInfo.Name)
                    {
                        var ex = new CustomNodePackageLoadException(newInfoPath, infoPath, message);
                        Log(ex.Message, WarningLevel.Moderate);

                        // Log to notification view extension
                        Log(ex);
                        throw ex;
                    }
                }
                   else //(newInfo has owning Package, oldInfo does not)
                {
                   
                    // This represents the case where a previous info was not from a package, but the current info
                    // has an owning package.
                    var looseCustomNodeToPackageMessage = String.Format(Properties.Resources.FunctionDefinitionOverwrittenMessage, newInfo.Name, newInfo.PackageInfo, info.Name);

                    var ex = new CustomNodePackageLoadException(newInfoPath, infoPath, looseCustomNodeToPackageMessage);
                    Log(ex.Message, WarningLevel.Mild);
                    Log(ex);
                }


            }

            NodeInfos[newInfo.FunctionId] = newInfo;
            OnInfoUpdated(newInfo);
        }

        /// <summary>
        ///     Returns the function workspace from a guid
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="ws"></param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public bool TryGetFunctionWorkspace(
            Guid id,
            bool isTestMode,
            out CustomNodeWorkspaceModel ws)
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
        /// Returns the function workspace.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="isTestMode">if set to <c>true</c> [is test mode].</param>
        /// <param name="ws">The workspace.</param>
        /// <returns>Boolean indicating if Custom Node Workspace defination is loaded.</returns>
        public bool TryGetFunctionWorkspace(
            Guid id,
            bool isTestMode,
            out ICustomNodeWorkspaceModel ws)
        {
            CustomNodeWorkspaceModel workSpace;
            var result = TryGetFunctionWorkspace(id, isTestMode, out workSpace);
            ws = workSpace;
            return result;
        }

        /// <summary>
        ///     Returns the function definition from a guid.
        /// </summary>
        /// <param name="id">Custom node identifier.</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="definition"></param>
        /// <returns>Boolean indicating if Custom Node Workspace defination is loaded.</returns>
        public bool TryGetFunctionDefinition(
            Guid id,
            bool isTestMode,
            out CustomNodeDefinition definition)
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
        ///     Returns true if the custom node's unique identifier is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="guid">The FunctionId</param>
        public bool Contains(Guid guid)
        {
            return IsInitialized(guid) || NodeInfos.ContainsKey(guid);
        }

        /// <summary>
        ///     Returns true if the custom node's name is inside the manager (initialized or not)
        /// </summary>
        /// <param name="name">The name of the custom node.</param>
        public bool Contains(string name)
        {
            CustomNodeInfo info;
            return TryGetNodeInfo(name, out info);
        }

        /// <summary>
        ///     Indicates whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="name">The name of the node</param>
        /// <returns>The name of the </returns>
        internal bool IsInitialized(string name)
        {
            CustomNodeInfo info;
            return TryGetNodeInfo(name, out info) && IsInitialized(info.FunctionId);
        }

        /// <summary>
        ///     Indicates whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        internal bool IsInitialized(Guid guid)
        {
            return loadedCustomNodes.ContainsKey(guid);
        }

        /// <summary>
        ///     Returns a boolean indicating if successfully get a CustomNodeInfo object from a workspace path
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="info"></param>
        /// <returns>The custom node info object - null if we failed</returns>
        internal bool TryGetInfoFromPath(string path, bool isTestMode, out CustomNodeInfo info)
        {
            WorkspaceInfo header = null;
            XmlDocument xmlDoc;
            string jsonDoc;
            Exception ex;
            try
            {
                if (DynamoUtilities.PathHelper.isValidXML(path, out xmlDoc, out ex))
                {
                    if (!WorkspaceInfo.FromXmlDocument(xmlDoc, path, isTestMode, false, AsLogger(), out header))
                    {
                        Log(String.Format(Properties.Resources.FailedToLoadHeader, path));
                        info = null;
                        return false;
                    }
                }
                else if (DynamoUtilities.PathHelper.isValidJson(path, out jsonDoc, out ex))
                {
                    if (!WorkspaceInfo.FromJsonDocument(jsonDoc, path, isTestMode, false, AsLogger(), out header))
                    {
                        Log(String.Format(Properties.Resources.FailedToLoadHeader, path));
                        info = null;
                        return false;
                    }
                }
                else throw ex;
                info = new CustomNodeInfo(
                    Guid.Parse(header.ID),
                    header.Name,
                    header.Category,
                    header.Description,
                    path,
                    header.IsVisibleInDynamoLibrary);
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
        /// <param name="workspaceInfo">Workspace header describing the custom node file.</param>
        /// <param name="xmlDoc">Xml content of workspace</param>
        /// <param name="isTestMode">
        ///     Flag specifying whether or not this should operate in "test mode".
        /// </param>
        /// <param name="workspace"></param>
        /// <returns>Boolean indicating if Custom Node Workspace opened.</returns>
        public bool OpenCustomNodeWorkspace(
            XmlDocument xmlDoc,
            WorkspaceInfo workspaceInfo,
            bool isTestMode,
            out WorkspaceModel workspace)
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
            XmlDocument xmlDoc,
            out CustomNodeWorkspaceModel workspace)
        {
            // Add custom node definition firstly so that a recursive
            // custom node won't recursively load itself.
            SetPreloadFunctionDefinition(Guid.Parse(workspaceInfo.ID));
            string jsonDoc;
            CustomNodeWorkspaceModel newWorkspace = null;
            if (xmlDoc is XmlDocument)
            {
                var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, nodeFactory);
                newWorkspace = new CustomNodeWorkspaceModel(
                nodeFactory,
                nodeGraph.Nodes,
                nodeGraph.Notes,
                nodeGraph.Annotations,
                nodeGraph.Presets,
                nodeGraph.ElementResolver,
                workspaceInfo);
            }
            else
            {
                Exception ex;
                if (DynamoUtilities.PathHelper.isValidJson(workspaceInfo.FileName, out jsonDoc, out ex))
                {
                    //we pass null for engine and scheduler as apparently the custom node constructor doesn't need them.
                    newWorkspace = (CustomNodeWorkspaceModel)WorkspaceModel.FromJson(jsonDoc, this.libraryServices, null, null, nodeFactory, false, true, this);
                    newWorkspace.FileName = workspaceInfo.FileName;
                    newWorkspace.Category = workspaceInfo.Category;
                    // Mark the custom node workspace as having no changes - when we set the category on the above line
                    // this marks the workspace as changed.
                    newWorkspace.HasUnsavedChanges = false;
                }
            }

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
        /// <returns>Boolean indicating if Custom Node initialized.</returns>
        private bool InitializeCustomNode(
            Guid functionId,
            bool isTestMode,
            out CustomNodeWorkspaceModel workspace)
        {
            try
            {
                var customNodeInfo = NodeInfos[functionId];
                var path = customNodeInfo.Path;
                Log(String.Format(Properties.Resources.LoadingNodeDefinition, customNodeInfo, path));
                WorkspaceInfo info;
                XmlDocument xmlDoc;
                string strInput;
                Exception ex;
                if (DynamoUtilities.PathHelper.isValidXML(path, out xmlDoc, out ex))
                {
                    if (WorkspaceInfo.FromXmlDocument(xmlDoc, path, isTestMode, false, AsLogger(), out info))
                    {
                        info.ID = functionId.ToString();
                        if (migrationManager.ProcessWorkspace(info, xmlDoc, isTestMode, nodeFactory))
                        {
                            return InitializeCustomNode(info, xmlDoc, out workspace);
                        }
                    }
                }
                else if (DynamoUtilities.PathHelper.isValidJson(path, out strInput, out ex))
                {
                    // TODO: Skip Json migration for now
                    WorkspaceInfo.FromJsonDocument(strInput, path, isTestMode, false, AsLogger(), out info);
                    info.ID = functionId.ToString();
                    return InitializeCustomNode(info, null, out workspace);
                }
                else throw ex;
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
        internal WorkspaceModel CreateCustomNode(string name, string category, string description, Guid? functionId = null)
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
                FileName = string.Empty,
                IsVisibleInDynamoLibrary = true
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
        internal bool TryGetNodeInfo(Guid id, out CustomNodeInfo info)
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
        internal bool TryGetNodeInfo(string name, out CustomNodeInfo info)
        {
            info = NodeInfos.Values.FirstOrDefault(x => x.Name == name);
            return info != null;
        }

        /// <summary>
        ///     Collapse a set of nodes in a given workspace.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        /// <param name="selectedNotes"> The note models in current selection </param>
        /// <param name="currentWorkspace"> The workspace where</param>
        /// <param name="isTestMode"></param>
        /// <param name="args"></param>
        internal CustomNodeWorkspaceModel Collapse(
            IEnumerable<NodeModel> selectedNodes,
            IEnumerable<NoteModel> selectedNotes,
            WorkspaceModel currentWorkspace,
            bool isTestMode,
            FunctionNamePromptEventArgs args)
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

            Debug.WriteLine("Current workspace has {0} nodes and {1} connectors",
                currentWorkspace.Nodes.Count(), currentWorkspace.Connectors.Count());

            using (undoRecorder.BeginActionGroup())
            {
                #region Determine Inputs and Outputs

                //Step 1: determine which nodes will be inputs to the new node
                var inputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.InPorts.Count)
                                .Where(index => node.InPorts[index].Connectors.Any())
                                .Select(data => Tuple.Create(node, data, node.InputNodes[data]))
                                .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

                var outputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPorts.Count)
                                .Where(index => node.OutPorts[index].Connectors.Any())
                                .SelectMany(
                                    data =>
                                        node.OutputNodes[data].Where(
                                            output => !selectedNodeSet.Contains(output.Item2))
                                        .Select(output => Tuple.Create(node, data, output)))));

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
                var newNotes = new List<NoteModel>();
                var newAnnotations = new List<AnnotationModel>();

                // Step 4: move all nodes and notes to new workspace remove from old
                // PB: This could be more efficiently handled by a copy paste, but we
                // are preservering the node 
                foreach (var node in selectedNodeSet)
                {
                    undoRecorder.RecordDeletionForUndo(node);
                    currentWorkspace.RemoveAndDisposeNode(node);

                    // Assign a new guid to this node, otherwise when node is
                    // compiled to AST, literally it is still in global scope
                    // instead of in function scope.
                    node.GUID = Guid.NewGuid();

                    // shift nodes
                    node.X = node.X - leftShift;
                    node.Y = node.Y - topMost;

                    newNodes.Add(node);
                }

                foreach (var note in selectedNotes)
                {
                    undoRecorder.RecordDeletionForUndo(note);
                    currentWorkspace.RemoveNote(note);

                    note.GUID = Guid.NewGuid();
                    note.X = note.X - leftShift;
                    note.Y = note.Y - topMost;
                    newNotes.Add(note);
                }

                //Copy the group from newNodes
                foreach (var group in DynamoSelection.Instance.Selection.OfType<AnnotationModel>())
                {
                    undoRecorder.RecordDeletionForUndo(group);
                    currentWorkspace.RemoveGroup(group);

                    group.GUID = Guid.NewGuid();
                    group.Nodes = group.DeletedModelBases;
                    newAnnotations.Add(group);
                }

                // Now all selected nodes already moved to custom workspace,
                // clear the selection.
                DynamoSelection.Instance.ClearSelection();

                foreach (var conn in fullySelectedConns)
                {
                    ConnectorModel.Make(conn.Start.Owner, conn.End.Owner, conn.Start.Index, conn.End.Index);
                }

                #endregion

                #region Process inputs

                var inConnectors = new List<Tuple<NodeModel, int>>();
                var uniqueInputSenders = new Dictionary<Tuple<NodeModel, int>, Symbol>();
                var classTable = this.libraryServices.LibraryManagementCore.ClassTable;

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
                            InputSymbol = inputReceiverNode.InPorts[inputReceiverData].Name,
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
                            parameters = func.Controller.Definition.Parameters.ToList();
                        }
                        else if (inputReceiverNode is DSFunctionBase)
                        {
                            var dsFunc = inputReceiverNode as DSFunctionBase;
                            var funcDesc = dsFunc.Controller.Definition;
                            parameters = funcDesc.Parameters.ToList();

                            // if the node is an instance member the function won't contain a 
                            // parameter for this type so we need to generate a new typedParameter.
                            if (funcDesc.Type == Engine.FunctionType.InstanceMethod ||
                                funcDesc.Type == Engine.FunctionType.InstanceProperty)
                            {
                                var dummyType = new ProtoCore.Type
                                {
                                    Name = funcDesc.ClassName,
                                    UID = classTable.IndexOf(funcDesc.ClassName)
                                };

                                var instanceParam = new TypedParameter(funcDesc.ClassName, dummyType);
                                parameters.Insert(0, instanceParam);
                            }
                        }

                        // so the input of custom node has format 
                        //    input_var_name : type
                        if (parameters != null && parameters.Count() > inputReceiverData)
                        {
                            var port = inputReceiverNode.InPorts[inputReceiverData];
                            var typedParameter = parameters[inputReceiverData];
                            // initially set the type name to the full type name
                            // then try to shorten it.
                            if (!string.IsNullOrEmpty(typedParameter.Type.Name))
                            {
                                try
                                {

                                    var typedNode = new TypedIdentifierNode
                                    {
                                        Name = port.Name,
                                        Value = port.Name,
                                        datatype = typedParameter.Type,
                                        TypeAlias = typedParameter.Type.Name
                                    };

                                    NodeToCodeCompiler.ReplaceWithShortestQualifiedName(
                                        classTable,
                                        new List<AssociativeNode> { typedNode },
                                        currentWorkspace.ElementResolver);

                                    node.InputSymbol = $"{typedNode.Value} :{typedNode.TypeAlias}";
                                }
                                catch(Exception e)
                                {
                                    node.InputSymbol += ":" + typedParameter.Type.Name;
                                    this.AsLogger().LogError($"{e.Message}: could not generate a short type name for {typedParameter.Type.Name}");
                                }
                            }
                        }

                        node.SetNameFromNodeNameAttribute();
                        node.Y = inputIndex * (50 + node.Height);

                        uniqueInputSenders[key] = node;

                        newNodes.Add(node);
                    }

                    ConnectorModel.Make(node, inputReceiverNode, 0, inputReceiverData);
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

                            outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                            //Create Symbol Node
                            var node = new Output
                            {
                                Symbol = outputSenderNode.OutPorts[outputSenderData].Name,
                                X = rightMost + 75 - leftShift
                            };

                            node.Y = i * (50 + node.Height);

                            node.SetNameFromNodeNameAttribute();

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
                                Enumerable.Range(0, node.OutPorts.Count)
                                .Where(index => !node.OutPorts[index].IsConnected)
                                .Select(port => new { node, port })).Distinct())
                    {
                        //Create Symbol Node
                        var node = new Output
                        {
                            Symbol = hanging.node.OutPorts[hanging.port].Name,
                            X = rightMost + 75 - leftShift
                        };
                        node.Y = i * (50 + node.Height);
                        node.SetNameFromNodeNameAttribute();

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
                    newNotes,
                    newAnnotations,
                    Enumerable.Empty<PresetModel>(),
                    currentWorkspace.ElementResolver,
                    new WorkspaceInfo()
                    {
                        X = 0,
                        Y = 0,
                        Name = args.Name,
                        Category = args.Category,
                        Description = args.Description,
                        ID = newId.ToString(),
                        FileName = string.Empty,
                        IsVisibleInDynamoLibrary = true
                    });

                newWorkspace.HasUnsavedChanges = true;

                RegisterCustomNodeWorkspace(newWorkspace);

                Debug.WriteLine("Collapsed workspace has {0} nodes and {1} connectors",
                    newWorkspace.Nodes.Count(), newWorkspace.Connectors.Count());

                var collapsedNode = CreateCustomNodeInstance(newId, isTestMode: isTestMode);
                collapsedNode.X = avgX;
                collapsedNode.Y = avgY;
                currentWorkspace.AddAndRegisterNode(collapsedNode, centered: false);
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
