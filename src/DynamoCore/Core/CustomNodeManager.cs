using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Nodes;
using System.IO;

using DynamoUtilities;

using Enum = System.Enum;
using Utils = Dynamo.Nodes.Utilities;
using Dynamo.DSEngine;

namespace Dynamo.Utilities
{
    /// <summary>
    /// A simple class to keep track of custom nodes.
    /// </summary>
    public class CustomNodeInfo
    {
        public CustomNodeInfo(Guid guid, string name, string category, string description, string path)
        {
            Guid = guid;
            Name = name;
            Category = category;
            Description = description;
            Path = path;
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    public delegate void DefinitionLoadHandler(CustomNodeDefinition def);

    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeManager
    {

        #region Fields and properties

        private readonly DynamoModel dynamoModel;

        /// <summary>
        /// An event that is fired when a definition is loaded (e.g. when the node is placed)
        /// </summary>
        public event DefinitionLoadHandler DefinitionLoaded;

        public OrderedDictionary LoadedCustomNodes = new OrderedDictionary();

        /// <summary>
        /// NodeNames </summary>
        /// <value>Maps function names to function ids.</value>
        public ObservableDictionary<Guid, CustomNodeInfo> NodeInfos
        {
            get;
            private set;
        }

        /// <summary>
        /// SearchPath property </summary>
        /// <value>This is a list of directories where this object will 
        /// search for dyf files.</value>
        public ObservableCollection<string> SearchPath { get; private set; }

        #endregion

        /// <summary>
        ///     Class Constructor
        /// </summary>
        /// <param name="dynamoModel"></param>
        /// <param name="searchPath">The path to search for definitions</param>
        public CustomNodeManager(DynamoModel dynamoModel, string searchPath)
        {
            this.dynamoModel = dynamoModel;
            SearchPath = new ObservableCollection<string> { searchPath };
            NodeInfos = new ObservableDictionary<Guid, CustomNodeInfo>();
            AddDirectoryToSearchPath(DynamoPathManager.Instance.CommonDefinitions);
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
        ///     Enumerates all of the loaded custom node defs
        /// </summary>
        /// <returns>A list of the current loaded custom node defs</returns>
        public IEnumerable<CustomNodeDefinition> GetLoadedDefinitions()
        {
            return LoadedCustomNodes.Values.Cast<CustomNodeDefinition>();
        }

        /// <summary>
        ///     Manually add the CustomNodeDefinition to LoadedNodes, overwriting the existing CustomNodeDefinition
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public void AddFunctionDefinition(Guid id, CustomNodeDefinition def)
        {
            if (def.IsBeingLoaded)
                return;

            if (LoadedCustomNodes.Contains(id))
            {
                LoadedCustomNodes[id] = def;
            }
            else
            {
                LoadedCustomNodes.Add(id, def);
            }
        }

        /// <summary>
        ///     Import a dyf file for eventual initialization.  
        /// </summary>
        /// <returns>null if we failed to get data from the path, otherwise the CustomNodeInfo object for the </returns>
        public CustomNodeInfo AddFileToPath(string file)
        {
            Guid guid;
            string name;
            string category;
            string description;
            if (!GetHeaderFromPath(file, out guid, out name, out category, out description))
            {
                return null;
            }

            // the node has already been loaded
            // from somewhere else
            if (Contains(guid))
            {
                var nodeInfo = GetNodeInfo(guid);
                if (nodeInfo != null)
                {
                    return nodeInfo;
                }
            }

            var info = new CustomNodeInfo(guid, name, category, description, file);
            SetNodeInfo(info);
            return info;
        }

        public List<CustomNodeInfo> GetInfosFromFolder(string dir)
        {
            return Directory.Exists(dir) ? Directory.EnumerateFiles(dir, "*.dyf")
                     .Select(AddFileToPath)
                     .Where(x => x != null)
                     .ToList() : new List<CustomNodeInfo>();

        }

        /// <summary>
        ///     Removes the custom nodes loaded from a particular folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RemoveTypesLoadedFromFolder(string path)
        {

            var guidsToRemove = GetInfosFromFolder(path).Select(x => x.Guid);
            guidsToRemove.ToList().ForEach(RemoveFromDynamo);

            return guidsToRemove.Any();

        }

        public CustomNodeInfo Remove(Guid guid)
        {
            var nodeInfo = GetNodeInfo(guid);

            if (LoadedCustomNodes.Contains(guid))
                LoadedCustomNodes.Remove(guid);

            if (NodeInfos.ContainsKey(guid))
                NodeInfos.Remove(guid);

            return nodeInfo;
        }

        /// <summary>
        ///     Attempts to remove all traces of a particular custom node from Dynamo, assuming the node is not in a loaded workspace.
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveFromDynamo(Guid guid)
        {
            var nodeInfo = Remove(guid);

            // remove from search
            dynamoModel.SearchModel.RemoveNodeAndEmptyParentCategory(nodeInfo.Guid);
            dynamoModel.SearchModel.OnRequestSync();

            // remove from fscheme environment
            //dynamoModel.FSchemeEnvironment.RemoveSymbol(guid.ToString());
        }

        /// <summary>
        ///     Enumerates all of the files in the search path and get's their guids.
        ///     Does not instantiate the nodes.
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public List<CustomNodeInfo> UpdateSearchPath()
        {
            return SearchPath.Select(ScanNodeHeadersInDirectory)
                             .SelectMany(x => x).ToList();
        }

        /// <summary>
        ///     Enumerates all of the files in the search path and get's their guids.
        ///     Does not instantiate the nodes.
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public IEnumerable<CustomNodeInfo> ScanNodeHeadersInDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return new List<CustomNodeInfo>();
            }

            return Directory.EnumerateFiles(dir, "*.dyf")
                            .Select(AddFileToPath)
                            .Where(nodeInfo => nodeInfo != null).ToList();
        }

        /// <summary>
        ///     Update a CustomNodeDefinition amongst the loaded FunctionDefinitions, without
        ///     settings its path
        /// </summary>
        /// <param name="guid">The custom node id</param>
        /// <param name="def">The definition for the function</param>
        public void SetFunctionDefinition(Guid guid, CustomNodeDefinition def)
        {
            if (LoadedCustomNodes.Contains(guid))
            {
                LoadedCustomNodes.Remove(guid);
            }
            LoadedCustomNodes.Add(guid, def);
        }

        /// <summary>
        /// Stores the path and function definition without initializing a node.  Overwrites
        /// the existing NodeInfo if necessary
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodeInfo(CustomNodeInfo newInfo)
        {
            var nodeInfo = GetNodeInfo(newInfo.Guid);
            if (nodeInfo == null)
            {
                NodeInfos.Add(newInfo.Guid, newInfo);
            }
            else
            {
                NodeInfos[newInfo.Guid] = newInfo;
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodePath(Guid id, string path)
        {
            var nodeInfo = GetNodeInfo(id);
            if (nodeInfo != null)
            {
                nodeInfo.Path = path;
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public string GetNodePath(Guid id)
        {
            var nodeInfo = GetNodeInfo(id);
            return nodeInfo != null ? nodeInfo.Path : null;
        }

        /// <summary>
        /// Return the default search path
        /// </summary>
        /// <returns>A string representing a path</returns>
        public string GetDefaultSearchPath()
        {
            return SearchPath[0];
        }

        /// <summary>
        ///     Get the function definition from a guid
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public CustomNodeDefinition GetFunctionDefinition(Guid id)
        {
            if (!Contains(id))
                return null;

            if (IsInitialized(id))
            {
                return LoadedCustomNodes[id] as CustomNodeDefinition;
            }
            else
            {
                CustomNodeDefinition def;
                if (GetDefinitionFromPath(id, out def))
                {
                    return def;
                }
            }
            return null;
        }

        /// <summary>
        ///     Try to reload function definition and set all custom node 
        ///     instances' defintion to the new one. It happens when loading
        ///     custom node for proxy node.
        /// </summary>
        /// <param name="id">The unique id for the node</param>
        /// <returns></returns>
        public CustomNodeDefinition ReloadFunctionDefintion(Guid id)
        {
            CustomNodeDefinition def;
            if (!GetDefinitionFromPath(id, out def))
                return null;

            if (def == null)
            {
                return null;
            }


            var customNodeInstances =
                    dynamoModel
                    .AllNodes
                    .OfType<Function>()
                    .Where(f => f.Definition != null &&
                                f.Definition.FunctionId.Equals(id));

            foreach (var item in customNodeInstances)
            {
                item.ResyncWithDefinition(def);
                item.State = ElementState.Dead;
                item.ValidateConnections();
            }

            return def;
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
        /// Recompile all custom nodes
        /// </summary>
        public void RecompileAllNodes(EngineController engine)
        {
            var compiledNodes = new HashSet<Guid>();

            var enumerator = LoadedCustomNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var guid = (Guid)enumerator.Key;
                var def = enumerator.Value as CustomNodeDefinition;

                if (!compiledNodes.Contains(guid))
                {
                    def.Compile(this.dynamoModel, engine);
                    compiledNodes.Add(guid);
                }
            }
        }

        /// <summary>
        ///     Tells whether the custom node's name is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="name">The name of the custom node.</param>
        public bool Contains(string name)
        {
            return IsInitialized(name) || GetNodeInfo(name) != null;
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="name">The name of the node</param>
        /// <returns>The name of the </returns>
        public bool IsInitialized(string name)
        {
            var info = GetNodeInfo(name);
            return info != null && IsInitialized(info.Guid);
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        public bool IsInitialized(Guid guid)
        {
            return LoadedCustomNodes.Contains(guid);
        }

        /// <summary>
        ///     Get the guid from a name.
        /// </summary>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        /// <returns>False if the name doesn't exist in this</returns>
        public Guid GetGuidFromName(string name)
        {
            return !Contains(name) ? Guid.Empty : GetNodeInfo(name).Guid;
        }

        /// <summary>
        ///     Get a CustomNodeDefinition from a name of a node, also stores type internally info for future instantiation.
        ///     And add the compiled node to the enviro.
        ///     As a side effect, any of its dependent nodes are also initialized.
        /// </summary>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetDefinition(string name, out CustomNodeDefinition result)
        {
            if (!Contains(name))
            {
                result = null;
                return false;
            }

            return GetDefinition(GetGuidFromName(name), out result);

        }

        /// <summary>
        ///     Get a CustomNodeDefinition from a guid, also stores type internally info for future instantiation.
        ///     And add the compiled node to the enviro.
        ///     As a side effect, any of its dependent nodes are also initialized.
        /// </summary>
        /// <param name="environment">The environment from which to get the </param>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetDefinition(Guid guid, out CustomNodeDefinition result)
        {
            if (!Contains(guid))
            {
                result = null;
                return false;
            }

            if (!IsInitialized(guid))
            {
                if (!GetDefinitionFromPath(guid, out result))
                {
                    return false;
                }
            }
            else
            {
                result = LoadedCustomNodes[guid] as CustomNodeDefinition;
            }

            return true;
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <returns>The custom node info object - null if we failed</returns>
        public CustomNodeInfo GetHeaderFromPath(string path)
        {
            string name, category, description;
            Guid id;
            return GetHeaderFromPath(path, out id, out name, out category, out description)
                ? new CustomNodeInfo(
                    id,
                    Path.GetFileNameWithoutExtension(path),
                    category,
                    description,
                    path)
                : null;
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="guid">A reference to the guid (OUT) Guid.Empty if function returns false. </param>
        /// <returns>Whether we successfully obtained the guid or not.  </returns>
        public bool GetHeaderFromPath(string path, out Guid guid, out string name, out string category, out string description)
        {

            try
            {
                var funName = "";
                var id = "";
                var cat = "";
                var des = "";

                #region Get xml document and parse

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                // load the header
                // handle legacy workspace nodes called dynWorkspace
                // and new workspaces without the dyn prefix
                XmlNodeList workspaceNodes = xmlDoc.GetElementsByTagName("Workspace");
                if (workspaceNodes.Count == 0)
                    workspaceNodes = xmlDoc.GetElementsByTagName("dynWorkspace");

                foreach (XmlNode node in workspaceNodes)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }
                        else if (att.Name.Equals("Category"))
                        {
                            cat = att.Value;
                        }
                        else if (att.Name.Equals("Description"))
                        {
                            des = att.Value;
                        }
                    }
                }

                #endregion

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName))
                {
                    guid = GuidUtility.Create(GuidUtility.UrlNamespace, funName);
                }
                else
                {
                    guid = Guid.Parse(id);
                }

                name = funName;
                category = cat;
                description = des;
                return true;

            }
            catch (Exception e)
            {
                this.dynamoModel.Logger.Log("ERROR: The header for the custom node at " + path + " failed to load.  It will be left out of search.");
                this.dynamoModel.Logger.Log(e.ToString());
                category = "";
                guid = Guid.Empty;
                name = "";
                description = "";
                return false;
            }

        }

        /// <summary>
        ///     Get a CustomNodeDefinition from a workspace.  Assumes the CustomNodeDefinition is already loaded.
        ///     Use IsInitialized to figure out if the FunctionDef is loaded.
        /// </summary>
        /// <param name="workspace">The workspace which you'd like to find the Definition for</param>
        /// <returns>A valid function definition if the CustomNodeDefinition is already loaded, otherwise null. </returns>
        public CustomNodeDefinition GetDefinitionFromWorkspace(WorkspaceModel workspace)
        {
            return LoadedCustomNodes.Values.Cast<CustomNodeDefinition>().FirstOrDefault((def) => def.WorkspaceModel == workspace);
        }

        /// <summary>
        ///     Deserialize a function definition from a given path.  A side effect of this function is that
        ///     the node is added to the dictionary of loadedNodes.  
        /// </summary>
        /// <param name="funcDefGuid">The function guid we're currently loading</param>
        /// <param name="def">The resultant function definition</param>
        /// <returns></returns>
        private bool GetDefinitionFromPath(Guid funcDefGuid, out CustomNodeDefinition def)
        {
            try
            {
                var xmlPath = GetNodePath(funcDefGuid);

                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = 0;
                double cy = 0;
                string description = "";
                string version = "";

                double zoom = 1.0;
                string id = "";

                // load the header

                // handle legacy workspace nodes called dynWorkspace
                // and new workspaces without the dyn prefix
                XmlNodeList workspaceNodes = xmlDoc.GetElementsByTagName("Workspace");
                if (workspaceNodes.Count == 0)
                    workspaceNodes = xmlDoc.GetElementsByTagName("dynWorkspace");

                foreach (XmlNode node in workspaceNodes)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Y"))
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("zoom"))
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
                        else if (att.Name.Equals("Description"))
                            description = att.Value;
                        else if (att.Name.Equals("ID"))
                            id = att.Value;
                        else if (att.Name.Equals("Version"))
                            version = att.Value;
                    }
                }

                Version fileVersion = MigrationManager.VersionFromString(version);

                var currentVersion = MigrationManager.VersionFromWorkspace(dynamoModel.HomeSpace);

                if (fileVersion > currentVersion)
                {
                    bool resume = Utils.DisplayFutureFileMessage(this.dynamoModel, xmlPath, fileVersion, currentVersion);
                    if (!resume)
                    {
                        def = null;
                        return false;
                    }
                }

                var decision = MigrationManager.ShouldMigrateFile(fileVersion, currentVersion);
                if (decision == MigrationManager.Decision.Abort)
                {
                    Utils.DisplayObsoleteFileMessage(this.dynamoModel, xmlPath, fileVersion, currentVersion);

                    def = null;
                    return false;
                }
                else if (decision == MigrationManager.Decision.Migrate)
                {
                    string backupPath = string.Empty;
                    bool isTesting = DynamoModel.IsTestMode; // No backup during test.
                    if (!isTesting && MigrationManager.BackupOriginalFile(xmlPath, ref backupPath))
                    {
                        string message = string.Format(
                            "Original file '{0}' gets backed up at '{1}'",
                            Path.GetFileName(xmlPath), backupPath);

                        dynamoModel.Logger.Log(message);
                    }

                    MigrationManager.Instance.ProcessWorkspaceMigrations(this.dynamoModel, xmlDoc, fileVersion);
                    MigrationManager.Instance.ProcessNodesInWorkspace(this.dynamoModel, xmlDoc, fileVersion);
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName))
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                #endregion

                //DynamoCommands.WriteToLogCmd.Execute("Loading node definition for \"" + funName + "\" from: " + xmlPath);
                this.dynamoModel.Logger.Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                var ws = new CustomNodeWorkspaceModel(dynamoModel,
                    funName, category.Length > 0
                    ? category
                    : "Custom Nodes", description, cx, cy)
                {
                    WatchChanges = false,
                    FileName = xmlPath,
                    Zoom = zoom
                };

                def = new CustomNodeDefinition(Guid.Parse(id))
                {
                    WorkspaceModel = ws,
                    IsBeingLoaded = true
                };

                // Add custom node definition firstly so that a recursive
                // custom node won't recursively load itself.
                SetFunctionDefinition(def.FunctionId, def);

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("Connectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("Notes");

                if (elNodes.Count == 0)
                    elNodes = xmlDoc.GetElementsByTagName("dynElements");
                if (cNodes.Count == 0)
                    cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                if (nNodes.Count == 0)
                    nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                #region instantiate nodes

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes["type"];
                    XmlAttribute guidAttrib = elNode.Attributes["guid"];
                    XmlAttribute nicknameAttrib = elNode.Attributes["nickname"];
                    XmlAttribute xAttrib = elNode.Attributes["x"];
                    XmlAttribute yAttrib = elNode.Attributes["y"];
                    XmlAttribute lacingAttrib = elNode.Attributes["lacing"];
                    XmlAttribute isVisAttrib = elNode.Attributes["isVisible"];
                    XmlAttribute isUpstreamVisAttrib = elNode.Attributes["isUpstreamVisible"];

                    string typeName = typeAttrib.Value;

                    //test the GUID to confirm that it is non-zero
                    //if it is zero, then we have to fix it
                    //this will break the connectors, but it won't keep
                    //propagating bad GUIDs
                    var guid = new Guid(guidAttrib.Value);
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                    }

                    string nickname = nicknameAttrib.Value;

                    double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                    double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                    bool isVisible = true;
                    if (isVisAttrib != null)
                        isVisible = isVisAttrib.Value == "true" ? true : false;

                    bool isUpstreamVisible = true;
                    if (isUpstreamVisAttrib != null)
                        isUpstreamVisible = isUpstreamVisAttrib.Value == "true" ? true : false;

                    // Retrieve optional 'function' attribute (only for DSFunction).
                    XmlAttribute signatureAttrib = elNode.Attributes["function"];
                    var signature = signatureAttrib == null ? null : signatureAttrib.Value;

                    NodeModel el = null;
                    XmlElement dummyElement = null;

                    try
                    {
                        // The attempt to create node instance may fail due to "type" being
                        // something else other than "NodeModel" derived object type. This 
                        // is possible since some legacy nodes have been made to derive from
                        // "MigrationNode" object type that is not derived from "NodeModel".
                        // 
                        typeName = Nodes.Utilities.PreprocessTypeName(typeName);
                        System.Type type = Nodes.Utilities.ResolveType(this.dynamoModel, typeName);
                        TypeLoadData data = Nodes.Utilities.GetDataForType(dynamoModel, type);
                        if (type != null)
                            el = ws.NodeFactory.CreateNodeInstance(data, nickname, signature, guid);

                        if (el != null)
                        {
                            el.Load(elNode);
                        }
                        else
                        {
                            var e = elNode as XmlElement;
                            dummyElement = MigrationManager.CreateMissingNode(e, 1, 1);
                        }
                    }
                    catch (UnresolvedFunctionException)
                    {
                        // If a given function is not found during file load, then convert the 
                        // function node into a dummy node (instead of crashing the workflow).
                        // 
                        var e = elNode as XmlElement;
                        dummyElement = MigrationManager.CreateUnresolvedFunctionNode(e);
                    }

                    if (dummyElement != null) // If a dummy node placement is desired.
                    {
                        // The new type representing the dummy node.
                        typeName = dummyElement.GetAttribute("type");
                        System.Type type = Dynamo.Nodes.Utilities.ResolveType(this.dynamoModel, typeName);
                        var tld = Nodes.Utilities.GetDataForType(dynamoModel, type);

                        el = ws.NodeFactory.CreateNodeInstance(tld, nickname, string.Empty, guid);
                        el.Load(dummyElement);
                    }

                    ws.Nodes.Add(el);

                    el.X = x;
                    el.Y = y;

                    if (lacingAttrib != null)
                    {
                        LacingStrategy lacing = LacingStrategy.First;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        el.ArgumentLacing = lacing;
                    }

                    el.DisableReporting();

                    // This is to fix MAGN-3648. Method reference in CBN that gets 
                    // loaded before method definition causes a CBN to be left in 
                    // a warning state. This is to clear such warnings and set the 
                    // node to "Dead" state (correct value of which will be set 
                    // later on with a call to "EnableReporting" below). Please 
                    // refer to the defect for details and other possible fixes.
                    // 
                    if (el.State == ElementState.Warning && (el is CodeBlockNodeModel))
                        el.State = ElementState.Dead;

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;
                }

                #endregion

                #region instantiate connectors

                foreach (XmlNode connector in cNodesList.ChildNodes)
                {
                    XmlAttribute guidStartAttrib = connector.Attributes[0];
                    XmlAttribute intStartAttrib = connector.Attributes[1];
                    XmlAttribute guidEndAttrib = connector.Attributes[2];
                    XmlAttribute intEndAttrib = connector.Attributes[3];
                    XmlAttribute portTypeAttrib = connector.Attributes[4];

                    var guidStart = new Guid(guidStartAttrib.Value);
                    var guidEnd = new Guid(guidEndAttrib.Value);
                    int startIndex = Convert.ToInt16(intStartAttrib.Value);
                    int endIndex = Convert.ToInt16(intEndAttrib.Value);
                    var portType = ((PortType)Convert.ToInt16(portTypeAttrib.Value));

                    //find the elements to connect
                    NodeModel start = null;
                    NodeModel end = null;

                    foreach (NodeModel e in ws.Nodes)
                    {
                        if (e.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.GUID == guidEnd)
                        {
                            end = e;
                        }
                        if (start != null && end != null)
                        {
                            break;
                        }
                    }

                    try
                    {
                        var newConnector = ws.AddConnection(
                            start, end,
                            startIndex, endIndex,
                            portType);
                    }
                    catch
                    {
                        dynamoModel.WriteToLog(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NickName, end.NickName));
                    }
                }

                #endregion

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = Convert.ToDouble(xAttrib.Value, CultureInfo.InvariantCulture);
                        double y = Convert.ToDouble(yAttrib.Value, CultureInfo.InvariantCulture);

                        ws.AddNote(false, x, y, text, Guid.NewGuid());
                    }
                }

                #endregion

                foreach (var e in ws.Nodes)
                    e.EnableReporting();

                def.IsBeingLoaded = false;

                def.Compile(this.dynamoModel, this.dynamoModel.EngineController);

                SetFunctionDefinition(def.FunctionId, def);

                ws.WatchChanges = true;

                OnGetDefinitionFromPath(def);

            }
            catch (Exception ex)
            {
                dynamoModel.WriteToLog("There was an error opening the workbench.");
                dynamoModel.WriteToLog(ex);

                if (DynamoModel.IsTestMode)
                    throw ex; // Rethrow for NUnit.

                def = null;
                return false;
            }

            return true;
        }

        public void OnGetDefinitionFromPath(CustomNodeDefinition def)
        {
            if (DefinitionLoaded != null && def != null)
                DefinitionLoaded(def);
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }

        /// <summary>
        /// Adds a directory to the search path without adding a duplicates.
        /// </summary>
        /// <param name="p">The absolute path of the directory to add</param>
        /// <returns>False if the directory does not exist or it already exists in the 
        /// search path. </returns>
        internal bool AddDirectoryToSearchPath(string p)
        {
            if (!Directory.Exists(p) || SearchPath.Contains(p)) return false;
            SearchPath.Add(p);
            return true;
        }

        internal CustomNodeInfo GetNodeInfo(Guid x)
        {
            CustomNodeInfo info;
            NodeInfos.TryGetValue(x, out info);
            return info;
        }

        internal CustomNodeInfo GetNodeInfo(string name)
        {
            return NodeInfos.FirstOrDefault(x => x.Value.Name == name).Value;
        }


        public void Refactor(CustomNodeInfo nodeInfo)
        {
            Refactor(nodeInfo.Guid, nodeInfo.Name, nodeInfo.Category, nodeInfo.Description);
        }

        /// <summary>
        /// Refactor a custom node, including updating search
        /// </summary>
        /// <returns> Returns false if it fails.</returns>
        internal bool Refactor(Guid guid, string newName, string newCategory, string newDescription)
        {
            var nodeInfo = GetNodeInfo(guid);

            if (nodeInfo == null) return false;

            // rename the existing nodes - should be replaced with a proper binding
            dynamoModel.AllNodes
                       .Where(x => x is Function)
                       .Cast<Function>()
                       .Where(x => x.Definition.FunctionId == guid)
                       .ToList()
                       .ForEach(x =>
                       {
                           x.Name = newName;
                           x.NickName = newName;
                       });

            dynamoModel.SearchModel.RemoveNodeAndEmptyParentCategory(nodeInfo.Guid);

            nodeInfo.Name = newName;
            nodeInfo.Category = newCategory;
            nodeInfo.Description = newDescription;

            SetNodeInfo(nodeInfo);

            dynamoModel.SearchModel.Add(nodeInfo);
            dynamoModel.SearchModel.OnRequestSync();

            return true;
        }

        // this is a terrible hack
        internal ObservableDictionary<string, Guid> GetAllNodeNames()
        {
            var dict = new ObservableDictionary<string, Guid>();
            NodeInfos.Select(info => new KeyValuePair<string, Guid>(info.Value.Name, info.Value.Guid))
                .ToList()
                .ForEach(dict.Add);
            return dict;
        }

    }
}
