using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Nodes;
using System.IO;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.ViewModels;
using NUnit.Framework;
using Enum = System.Enum;

namespace Dynamo.Utilities
{
    /// <summary>
    /// A simple class to keep track of custom nodes.
    /// </summary>
    public class CustomNodeInfo
    {
        public CustomNodeInfo(Guid guid, string name, string category, string description, string path)
        {
            this.Guid = guid;
            this.Name = name;
            this.Category = category;
            this.Description = description;
            this.Path = path;
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    public delegate void DefinitionLoadHandler(FunctionDefinition def);

    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeManager
    {

        #region Fields and properties

        /// <summary>
        /// An event that is fired when a definition is loaded (e.g. when the node is placed)
        /// </summary>
        public event DefinitionLoadHandler DefinitionLoaded;

        private Dictionary<Guid, FunctionDefinition> loadedNodes = new Dictionary<Guid, FunctionDefinition>();
        private Dictionary<Guid, string> nodePaths = new Dictionary<Guid, string>();

        /// <summary>
        /// NodeNames </summary>
        /// <value>Maps function names to function ids.</value>
        public ObservableDictionary<string, Guid> NodeNames
        {
            get;
            private set;
        }

        /// <summary>
        /// NodeCategories property </summary>
        /// <value>Maps function ids to categories. </value>
        public ObservableDictionary<Guid, string> NodeCategories
        {
            get;
            private set;
        }

        /// <summary>
        /// NodeDescriptions property </summary>
        /// <value>Maps function ids to descriptions. </value>
        public ObservableDictionary<Guid, string> NodeDescriptions
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
        /// <param name="searchPath">The path to search for definitions</param>
        public CustomNodeManager(string searchPath)
        {
            SearchPath = new ObservableCollection<string>();
            SearchPath.Add(searchPath);

            NodeNames = new ObservableDictionary<string, Guid>();
            NodeCategories = new ObservableDictionary<Guid, string>();
            NodeDescriptions = new ObservableDictionary<Guid, string>();

        }
        /// <summary> 
        /// Get a function id from a guid assuming that the file is already loaded.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Guid GuidFromPath(string path)
        {
            var pair = this.nodePaths.FirstOrDefault(x => x.Value == path);
            return pair.Key;
        }

        /// <summary>
        ///     Enumerates all of the node names.
        /// </summary>
        /// <returns>A list of all of the node names</returns>
        public IEnumerable<string> GetNodeNames()
        {
            return NodeNames.Keys.ToList();
        }

        /// <summary>
        ///     Enumerates all of the node name guid pairs
        /// </summary>
        /// <returns>A list of tuples with the name as first element and guid as second</returns>
        public IEnumerable<Tuple<string, string, Guid>> GetNodeNameCategoryAndGuidList()
        {
            return this.NodeNames.AsEnumerable().Select(first => new Tuple<string, string, Guid>(first.Key, NodeCategories[first.Value], first.Value));
        }

        /// <summary>
        ///     Enumerates all of the loaded custom node defs
        /// </summary>
        /// <returns>A list of the current loaded custom node defs</returns>
        public IEnumerable<FunctionDefinition> GetLoadedDefinitions()
        {
            return loadedNodes.Values;
        }

        /// <summary>
        ///     Manually add the FunctionDefinition to LoadedNodes, overwriting the existing FunctionDefinition
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public void AddFunctionDefinition(Guid id, FunctionDefinition def)
        {
            this.loadedNodes[id] = def;
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
                return GetNodeInfo(guid);
            }

            var info = new CustomNodeInfo(guid, name, category, description, file);
            this.SetNodeInfo(info);

            return info;
        }

        /// <summary>
        ///     Indicates whether a custom node from a particular folder is loaded.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="typeNames">The typenames from the folder that are in use.</param>
        /// <returns></returns>
        public bool TypesFromFolderAreInUse(string path, ref HashSet<Tuple<string, string>> whereTypesAreLoaded)
        {
            whereTypesAreLoaded.UnionWith(dynSettings.Controller.DynamoModel.AllNodes.Where((n) => n is Function)
                                           .Cast<Function>()
                                           .Where((func) => this.nodePaths[func.Definition.FunctionId].StartsWith(path))
                                           .Select((func) => new Tuple<string, string>(func.Name, func.WorkSpace.Name)));

            return whereTypesAreLoaded.Any();
        }

        public List<CustomNodeInfo> GetInfosFromFolder(string dir)
        {
            
            return Directory.Exists(dir) ? Directory.EnumerateFiles(dir, "*.dyf")
                     .Select( AddFileToPath )
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
            guidsToRemove.ToList().ForEach(this.Remove);

            return guidsToRemove.Any();

        }

        /// <summary>
        ///     Remove a folder and all of its elements from the search path
        ///     and the current Dynamo instance
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RemoveFolder(string path)
        {
            if (SearchPath.Contains(path))
                SearchPath.Remove(path);
            return RemoveTypesLoadedFromFolder(path);
        }

        /// <summary>
        ///     Attempts to remove all traces of a particular custom node from Dynamo, assuming the node is not in a loaded workspace.
        /// </summary>
        /// <param name="guid"></param>
        public void Remove(Guid guid)
        {
            if (loadedNodes.ContainsKey(guid)) 
                loadedNodes.Remove(guid);
            if (nodePaths.ContainsKey(guid))
                nodePaths.Remove(guid);
            if (NodeCategories.ContainsKey(guid))
                NodeCategories.Remove(guid);
            var nodeName = NodeNames.Where((x) => x.Value == guid).ToList();
            nodeName.ForEach((pair) =>
             {
                    NodeNames.Remove(pair.Key);
                    dynSettings.Controller.SearchViewModel.RemoveNodeAndEmptyParentCategory(pair.Key);
             });
            dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
            dynSettings.Controller.FSchemeEnvironment.RemoveSymbol(guid.ToString());
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
        ///     Update a FunctionDefinition amongst the loaded FunctionDefinitions, without
        ///     settings its path
        /// </summary>
        /// <param name="guid">The custom node id</param>
        /// <param name="def">The definition for the function</param>
        public void SetFunctionDefinition(Guid guid, FunctionDefinition def)
        {
            if (this.loadedNodes.ContainsKey(guid))
            {
                this.loadedNodes.Remove(guid);
            }
            this.loadedNodes.Add(guid, def);
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodePath(Guid id, string path)
        {
            if (this.Contains(id))
            {
                this.nodePaths[id] = path;
            }
            else
            {
                this.nodePaths.Add(id, path);
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing a node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodeInfo(CustomNodeInfo info)
        {
            this.SetNodeName(info.Guid, info.Name);
            this.SetNodeCategory(info.Guid, info.Category);
            this.SetNodeDescription(info.Guid, info.Description);
            this.SetNodePath(info.Guid, info.Path);
        }

        /// <summary>
        ///     Sets the category for a custom node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="category">The name for the node</param>
        public void SetNodeName(Guid id, string name)
        {
            // remove if the guid already has a name assigned
            this.NodeNames.Where(x => x.Value == id).ToList().ForEach(x=>this.NodeNames.Remove(x));
            this.NodeNames.Add(name, id);
        }

        /// <summary>
        ///     Sets the category for a custom node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="category">The category for the node</param>
        public void SetNodeCategory(Guid id, string category)
        {
            if (this.NodeCategories.ContainsKey(id))
            {
                this.NodeCategories[id] = category;
            }
            else
            {
                this.NodeCategories.Add(id, category);
            }
        }

        /// <summary>
        ///     Sets the description for a custom node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="category">The description for the node</param>
        public void SetNodeDescription(Guid id, string description)
        {
            if (this.NodeDescriptions.ContainsKey(id))
            {
                this.NodeDescriptions[id] = description;
            }
            else
            {
                this.NodeDescriptions.Add(id, description);
            }
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
        public FunctionDefinition GetFunctionDefinition(Guid id)
        {
            if (!this.Contains(id))
                return null;

            if (this.IsInitialized(id))
            {
                return loadedNodes[id];
            }
            else
            {
                FunctionDefinition def;
                if (this.GetDefinitionFromPath(id, out def))
                {
                    return def;
                }
            }
            return null;
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="id">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public string GetNodePath(Guid id)
        {
            if (this.Contains(id) && nodePaths.ContainsKey(id))
            {
                return nodePaths[id];
            }
            return null;
        }

        /// <summary>
        ///     Tells whether the custom node's unique identifier is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="guid">The FunctionId</param>
        public bool Contains(Guid guid)
        {
            return IsInitialized(guid) || nodePaths.ContainsKey(guid);
        }

        /// <summary>
        ///     Tells whether the custom node's name is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="name">The name of the custom node.</param>
        public bool Contains(string name)
        {
            return IsInitialized(name) || NodeNames.ContainsKey(name);
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="name">The name of the node</param>
        /// <returns>The name of the </returns>
        public bool IsInitialized(string name)
        {
            if (this.NodeNames.ContainsKey(name))
            {
                var guid = this.NodeNames[name];
                return this.IsInitialized(guid);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        public bool IsInitialized(Guid guid)
        {
            return loadedNodes.ContainsKey(guid);
        }

        /// <summary>
        ///     Get the guid from a name.
        /// </summary>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        /// <returns>False if the name doesn't exist in this</returns>
        public Guid GetGuidFromName(string name)
        {
            if (!this.Contains(name))
            {
                return Guid.Empty;
            }

            return this.NodeNames[name];

        }

        /// <summary>
        ///     Get a guid from the name of a node.  If it doesn't exist, returns Guid.Empty.
        /// </summary>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetNodeInstance(DynamoController controller, string name, out Function result)
        {
            if (!this.Contains(name))
            {
                result = null;
                return false;
            }

            return this.GetNodeInstance(GetGuidFromName(name), out result);

        }

        /// <summary>
        ///     Get a dynFunction from a guid, also stores type internally info for future instantiation.
        ///     And add the compiled node to the enviro
        ///     As a side effect, any of its dependent nodes are also initialized.
        /// </summary>
        /// <param name="environment">The environment from which to get the </param>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetNodeInstance(Guid guid, out Function result)
        {
            var controller = dynSettings.Controller;

            if (!this.Contains(guid))
            {
                result = null;
                return false;
            }

            FunctionDefinition def = null;
            if (!this.IsInitialized(guid))
            {
                if (!GetDefinitionFromPath(guid, out def))
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                def = this.loadedNodes[guid];
            }

            WorkspaceModel ws = def.Workspace;

            IEnumerable<string> inputs =
                ws.Nodes.Where(e => e is Symbol)
                    .Select(s => (s as Symbol).InputSymbol);

            IEnumerable<string> outputs =
                ws.Nodes.Where(e => e is Output)
                    .Select(o => (o as Output).Symbol);

            if (!outputs.Any())
            {
                var topMost = new List<Tuple<int, NodeModel>>();

                IEnumerable<NodeModel> topMostNodes = ws.GetTopMostNodes();

                foreach (NodeModel topNode in topMostNodes)
                {
                    foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                    {
                        if (!topNode.HasOutput(output))
                            topMost.Add(Tuple.Create(output, topNode));
                    }
                }

                outputs = topMost.Select(x => x.Item2.OutPortData[x.Item1].NickName);
            }

            result = controller.DynamoViewModel.CreateFunction(inputs, outputs, def);
            result.NickName = ws.Name;

            return true;
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <returns>The custom node info object - null if we failed</returns>
        public static CustomNodeInfo GetHeaderFromPath(string path)
        {
            string name, category, description;
            Guid id;
            if (CustomNodeManager.GetHeaderFromPath(path, out id, out name, out category, out description))
            {
                return new CustomNodeInfo(id, name, category, description, path);
            }
            else
            {
                return null;
            }
            
        }
        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="guid">A reference to the guid (OUT) Guid.Empty if function returns false. </param>
        /// <returns>Whether we successfully obtained the guid or not.  </returns>
        public static bool GetHeaderFromPath(string path, out Guid guid, out string name, out string category, out string description)
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
                DynamoLogger.Instance.Log("ERROR: The header for the custom node at " + path + " failed to load.  It will be left out of search.");
                DynamoLogger.Instance.Log(e.ToString());
                category = "";
                guid = Guid.Empty;
                name = "";
                description = "";
                return false;
            }

        }

        /// <summary>
        ///     Get a FunctionDefinition from a workspace.  Assumes the FunctionDefinition is already loaded.
        ///     Use IsInitialized to figure out if the FunctionDef is loaded.
        /// </summary>
        /// <param name="workspace">The workspace which you'd like to find the Definition for</param>
        /// <returns>A valid function definition if the FunctionDefinition is already loaded, otherwise null. </returns>
        public FunctionDefinition GetDefinitionFromWorkspace(WorkspaceModel workspace)
        {
            return this.loadedNodes.Values.FirstOrDefault((def) => def.Workspace == workspace);
        }

        /// <summary>
        ///     Deserialize a function definition from a given path.  A side effect of this function is that
        ///     the node is added to the dictionary of loadedNodes.  
        /// </summary>
        /// <param name="funcDefGuid">The function guid we're currently loading</param>
        /// <param name="controller">Reference to the calling controller</param>
        /// <param name="def">The resultant function definition</param>
        /// <returns></returns>
        private bool GetDefinitionFromPath(Guid funcDefGuid, out FunctionDefinition def)
        {
            var controller = dynSettings.Controller;

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

                double zoom = 1.0;
                string id = "";

                // load the header

                // handle legacy workspace nodes called dynWorkspace
                // and new workspaces without the dyn prefix
                XmlNodeList workspaceNodes = xmlDoc.GetElementsByTagName("Workspace");
                if(workspaceNodes.Count == 0)
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
                        {
                            id = att.Value;
                        }
                    }
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
                dynSettings.Controller.DynamoModel.WriteToLog("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                var ws = new FuncWorkspace(
                    funName, category.Length > 0
                    ? category
                    : BuiltinNodeCategories.SCRIPTING_CUSTOMNODES, description, cx, cy)
                {
                    WatchChanges = false,
                    FilePath = xmlPath,
                    Zoom = zoom
                };

                def = new FunctionDefinition(Guid.Parse(id))
                {
                    Workspace = ws
                };

                // load a dummy version, so any nodes depending on this node
                // will find an (empty) identifier on compilation
                FScheme.Expression dummyExpression = FScheme.Expression.NewNumber_E(0);
                controller.FSchemeEnvironment.DefineSymbol(def.FunctionId.ToString(), dummyExpression);

                // set the node as loaded
                this.loadedNodes.Add(def.FunctionId, def);

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

                var badNodes = new List<Guid>();

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

                    typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                    System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);
                    if (null == type)
                    {
                        badNodes.Add(guid);
                        continue;
                    }

                    NodeModel el = dynSettings.Controller.DynamoModel.CreateNodeInstance(type, nickname, guid);

                    if (lacingAttrib != null)
                    {
                        LacingStrategy lacing = LacingStrategy.First;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        el.ArgumentLacing = lacing;
                    }

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;

                    ws.Nodes.Add(el);
                    el.WorkSpace = ws;
                    var node = el;

                    node.X = x;
                    node.Y = y;

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    
                    // moved this logic to LoadNode in dynFunction --SJE

                    if (el is Function)
                    {
                        var fun = el as Function;
                        // we've found a custom node, we need to attempt to load its guid.  
                        // if it doesn't exist (i.e. its a legacy node), we need to assign it one,
                        // deterministically
                        //Guid funId;
                        //try
                        //{
                        //    funId = Guid.Parse(fun.Symbol);
                        //}
                        //catch
                        //{
                        //    funId = GuidUtility.Create(GuidUtility.UrlNamespace, nicknameAttrib.Value);
                        //    fun.Symbol = funId.ToString();
                        //}

                        // if it's not a recurisve node and it's not yet loaded, load it
                        //if (funcDefGuid != funId && !this.loadedNodes.ContainsKey(funId))
                        //{
                        //    dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funId);
                        //    fun.Definition = this.loadedNodes[funId];
                        //}
                        //else if (this.loadedNodes.ContainsKey(funId))
                        //{
                        //    fun.Definition = this.loadedNodes[funId];
                        //}

                    }

                    el.Load(elNode); // has load definition in it, which we have not yet completed.
                   
                    
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
                    int portType = Convert.ToInt16(portTypeAttrib.Value);

                    //find the elements to connect
                    NodeModel start = null;
                    NodeModel end = null;

                    if (badNodes.Contains(guidStart) || badNodes.Contains(guidEnd))
                        continue;

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
                        var newConnector = ConnectorModel.Make(
                            start, end,
                            startIndex, endIndex,
                            portType);
                        if (newConnector != null)
                            ws.Connectors.Add(newConnector);
                    }
                    catch
                    {
                        //DynamoCommands.WriteToLogCmd.Execute(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NickName, end.NickName));
                        dynSettings.Controller.DynamoModel.WriteToLog(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NickName, end.NickName));
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

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);

                        dynSettings.Controller.DynamoModel.AddNote(paramDict);
                    }
                }

                #endregion

                foreach (var e in ws.Nodes)
                    e.EnableReporting();

                var expression = CompileFunction(def);
                controller.FSchemeEnvironment.DefineSymbol(def.FunctionId.ToString(), expression);

                ws.WatchChanges = true;

                this.OnGetDefinitionFromPath(def);

            }
            catch (Exception ex)
            {
                dynSettings.Controller.DynamoModel.WriteToLog("There was an error opening the workbench.");
                dynSettings.Controller.DynamoModel.WriteToLog(ex);

                if (controller.Testing)
                    Assert.Fail(ex.Message);

                def = null;
                return false;
            }

            return true;
        }

        public void OnGetDefinitionFromPath(FunctionDefinition def)
        {
            if (DefinitionLoaded != null && def != null)
                DefinitionLoaded(def);
        }

        public static FScheme.Expression CompileFunction(FunctionDefinition definition)
        {
            IEnumerable<string> ins;
            IEnumerable<string> outs;

            return CompileFunction(definition, out ins, out outs);
        }

        public static FScheme.Expression CompileFunction(FunctionDefinition definition, out IEnumerable<string> inputNames, out IEnumerable<string> outputNames)
        {
            inputNames = null;
            outputNames = null;

            if (definition == null)
                return null;

            // Get the internal nodes for the function
            WorkspaceModel functionWorkspace = definition.Workspace;

            #region Find outputs

            // Find output elements for the node
            List<Output> outputs = functionWorkspace.Nodes.OfType<Output>().ToList();

            var topMost = new List<Tuple<int, NodeModel>>();

            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => x.Inputs[0]));

                outputNames = outputs.Select(x => x.Symbol);
            }
            else
            {
                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as the output
                IEnumerable<NodeModel> topMostNodes = functionWorkspace.GetTopMostNodes();

                var outNames = new List<string>();

                foreach (NodeModel topNode in topMostNodes)
                {
                    if (topNode is Function && (topNode as Function).Definition == definition)
                    {
                        topMost.Add(Tuple.Create(0, topNode));
                        outNames.Add("∞");
                        continue;
                    }

                    foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                    {
                        if (!topNode.HasOutput(output))
                        {
                            topMost.Add(Tuple.Create(output, topNode));
                            outNames.Add(topNode.OutPortData[output].NickName);
                        }
                    }
                }

                outputNames = outNames;
            }

            #endregion

            // color the node to define its connectivity
            foreach (var ele in topMost)
            {
                ele.Item2.ValidateConnections();
            }

            //Find function entry point, and then compile the function and add it to our environmen
            var variables = functionWorkspace.Nodes.OfType<Symbol>().ToList();
            inputNames = variables.Select(x => x.InputSymbol);

            INode top;
            var buildDict = new Dictionary<NodeModel, Dictionary<int, INode>>();

            if (topMost.Count > 1)
            {
                InputNode node = new ExternalFunctionNode(FScheme.Value.NewList);

                int i = 0;
                foreach (var topNode in topMost)
                {
                    string inputName = i.ToString(CultureInfo.InvariantCulture);
                    node.AddInput(inputName);
                    node.ConnectInput(inputName, new BeginNode());
                    try
                    {
                        var exp = topNode.Item2.Build(buildDict, topNode.Item1);
                        node.ConnectInput(inputName, exp);
                    }
                    catch
                    {

                    }

                    i++;
                }

                top = node;
            }
            else if (topMost.Count == 1)
            {
                top = topMost[0].Item2.Build(buildDict, topMost[0].Item1);
            }
            else
            {
                // if the custom node is empty, it will initially be an empty begin
                top = new BeginNode();
            }

            // if the node has any outputs, we create a BeginNode in order to evaluate all of them
            // sequentially (begin evaluates a list of expressions)
            if (outputs.Any())
            {
                var beginNode = new BeginNode();
                List<NodeModel> hangingNodes = functionWorkspace.GetHangingNodes().ToList();

                foreach (var tNode in hangingNodes.Select((x, index) => new { Index = index, Node = x }))
                {
                    beginNode.AddInput(tNode.Index.ToString(CultureInfo.InvariantCulture));
                    beginNode.ConnectInput(
                        tNode.Index.ToString(CultureInfo.InvariantCulture),
                        tNode.Node.Build(buildDict, 0));
                }

                beginNode.AddInput(hangingNodes.Count.ToString(CultureInfo.InvariantCulture));
                beginNode.ConnectInput(hangingNodes.Count.ToString(CultureInfo.InvariantCulture), top);

                top = beginNode;
            }

            // make the anonymous function
            FScheme.Expression expression = Utils.MakeAnon(
                variables.Select(x => x.GUID.ToString()),
                top.Compile());

            return expression;

        }

        private static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" }
                );
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
            var path = GetNodePath(x);
            var des = NodeDescriptions[x];
            var cat = NodeCategories[x];
            var name = this.NodeNames.FirstOrDefault(pair => pair.Value == x).Key;
            return new CustomNodeInfo(x, name, cat, des, path);

        }

        /// <summary>
        /// Refactor a custom node
        /// </summary>
        /// <returns> Returns false if it fails.</returns>
        internal bool Refactor(Guid guid, string newName, string newCategory, string newDescription)
        {
            var nodeInfo = GetNodeInfo(guid);

            if (nodeInfo == null) return false;

            // rename the existing nodes - should be replaced with a proper binding
            dynSettings.Controller.DynamoModel.AllNodes
                       .Where(x => x is Function)
                       .Cast<Function>()
                       .Where(x => x.Definition.FunctionId == guid)
                       .Where(x => x.Name == nodeInfo.Name)
                       .ToList()
                       .ForEach(x =>
                           {
                               x.Name = newName;
                               x.NickName = newName;
                           });

            dynSettings.Controller.SearchViewModel.RemoveNodeAndEmptyParentCategory(nodeInfo.Name);

            nodeInfo.Name = newName;
            nodeInfo.Category = newCategory;
            nodeInfo.Description = newDescription;

            this.SetNodeInfo(nodeInfo);

            dynSettings.Controller.SearchViewModel.Add(nodeInfo);
            dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();

            return true;
        }
    }
}
