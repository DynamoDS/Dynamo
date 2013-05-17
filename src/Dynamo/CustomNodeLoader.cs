using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Nodes;
using System.IO;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.Commands;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeLoader {

        #region Fields and properties

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
        /// SearchPath property </summary>
        /// <value>This is where this object will search for dyf files.</value>
        public string SearchPath { get; set; }

        #endregion

        /// <summary>
        ///     Class Constructor
        /// </summary>
        /// <param name="searchPath">The path to search for definitions</param>
        public CustomNodeLoader(string searchPath) {
            SearchPath = searchPath;
            NodeNames = new ObservableDictionary<string, Guid>();
            NodeCategories = new ObservableDictionary<Guid, string>();
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
            return this.NodeNames.AsEnumerable().Select( (first) => new Tuple<string, string, Guid>(first.Key, NodeCategories[first.Value], first.Value));
        }

        /// <summary>
        ///     Enumerates all of the loaded custom node defs
        /// </summary>
        /// <returns>A list of the current loaded custom node defs</returns>
        public IEnumerable<FunctionDefinition> GetLoadedDefinitions()
        {
            return loadedNodes.Values.ToList();
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
        ///     Enumerates all of the files in the search path and get's their guids.
        ///     Does not instantiate the nodes.
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public bool UpdateSearchPath()
        {
            if (!Directory.Exists(SearchPath))
            {
                return false;
            }

            foreach (string file in Directory.EnumerateFiles(SearchPath, "*.dyf"))
            {
                Guid guid;
                string name;
                string category;
                if (GetHeaderFromPath(file, out guid, out name, out category))
                {
                    this.SetNodeInfo(name, category, guid, file);
                }
            }
            
            return true;
        }

        /// <summary>
        ///     Update a FunctionDefinition amongst the loaded FunctionDefinitions
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public bool SetFunctionDefinition(Guid guid, FunctionDefinition def)
        {
            return false;
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodePath(Guid id, string path)
        {
            if ( this.Contains( id ) ) {
                this.nodePaths[id] = path;
            } else {
                this.nodePaths.Add(id, path);
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing a node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        public void SetNodeInfo(string name, string category, Guid id, string path)
        {
            if ( this.Contains(name) )
            {
                this.NodeNames[name] = id;
            }
            else
            {
                this.NodeNames.Add(name, id);
            }
            this.SetNodeCategory(id, category);
            this.SetNodePath(id, path);
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
        ///     Get the function definition from a guid
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
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
                if ( this.GetDefinitionFromPath(id, dynSettings.Controller, out def) )
                {
                    return def;
                }
            }
            return null;
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public string GetNodePath(Guid id)
        {
            if (this.Contains(id))
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
        public bool GetNodeInstance(DynamoController controller, string name, out dynFunction result)
        {
            if (!this.Contains(name))
            {
                result = null;
                return false;
            }

            return this.GetNodeInstance(controller, GetGuidFromName(name), out result);

        }

        /// <summary>
        ///     Get a dynFunction from a guid, also stores type internally info for future instantiation.
        ///     And add the compiled node to the enviro
        ///     As a side effect, any of its dependent nodes are also initialized.
        /// </summary>
        /// <param name="environment">The environment from which to get the </param>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetNodeInstance(DynamoController controller, Guid guid, out dynFunction result)
        {
            if ( !this.Contains(guid) ) {
                result = null;
                return false;
            }

            FunctionDefinition def = null;
            if (!this.IsInitialized(guid))
            {
                if (!GetDefinitionFromPath(guid, controller, out def))
                {
                    result = null;
                    return false;
                }
            } else {
                def = this.loadedNodes[guid];
            }

            dynWorkspaceModel ws = def.Workspace;

            IEnumerable<string> inputs =
                ws.Nodes.Where(e => e is dynSymbol)
                    .Select(s => (s as dynSymbol).Symbol);

            IEnumerable<string> outputs =
                ws.Nodes.Where(e => e is dynOutput)
                    .Select(o => (o as dynOutput).Symbol);

            if (!outputs.Any())
            {
                var topMost = new List<Tuple<int, dynNodeModel>>();

                IEnumerable<dynNodeModel> topMostNodes = ws.GetTopMostNodes();

                foreach (dynNodeModel topNode in topMostNodes)
                {
                    foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                    {
                        if (!topNode.HasOutput(output))
                            topMost.Add(Tuple.Create(output, topNode));
                    }
                }

                outputs = topMost.Select(x => x.Item2.OutPortData[x.Item1].NickName);
            }

            result = new dynFunction(inputs, outputs, def);
            result.NickName = ws.Name;

            return true;
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="guid">A reference to the guid (OUT) Guid.Empty if function returns false. </param>
        /// <returns>Whether we successfully obtained the guid or not.  </returns>
        public static bool GetHeaderFromPath(string path, out Guid guid, out string name, out string category ) {

            try
            {
                var funName = "";
                var id = "";
                var cat = "";

                #region Get xml document and parse

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
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
                return true;

            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("ERROR: The header for the custom node at " + path + " failed to load.  It will be left out of search." );
                DynamoLogger.Instance.Log(e.ToString());
                category = "";
                guid = Guid.Empty;
                name = "";
                return false;
            }

        }

        /// <summary>
        ///     Get a FunctionDefinition from a workspace.  Assumes the FunctionDefinition is already loaded.
        ///     Use IsInitialized to figure out if the FunctionDef is loaded.
        /// </summary>
        /// <param name="workspace">The workspace which you'd like to find the Definition for</param>
        /// <returns>A valid function definition if the FunctionDefinition is already loaded, otherwise null. </returns>
        public FunctionDefinition GetDefinitionFromWorkspace(dynWorkspaceModel workspace)
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
        private bool GetDefinitionFromPath(Guid funcDefGuid, DynamoController controller, out FunctionDefinition def)
        {
            try
            {
                var xmlPath = GetNodePath(funcDefGuid);

                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = DynamoView.CANVAS_OFFSET_X;
                double cy = DynamoView.CANVAS_OFFSET_Y;
                string id = "";

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            cy = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
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

                DynamoCommands.WriteToLogCmd.Execute("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                var workSpace = new FuncWorkspace(
                    funName, category.Length > 0
                    ? category
                    : BuiltinNodeCategories.SCRIPTING_CUSTOMNODES, cx, cy);

                def = new FunctionDefinition(Guid.Parse(id))
                    {
                        Workspace = workSpace
                    };

                // load a dummy version, so any nodes depending on this node
                // will find an (empty) identifier on compilation
                FScheme.Expression dummyExpression = FScheme.Expression.NewNumber_E(0);
                controller.FSchemeEnvironment.DefineSymbol(def.FunctionId.ToString(), dummyExpression);
                this.loadedNodes.Add(def.FunctionId, def);

                dynWorkspaceModel ws = def.Workspace;

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                #region instantiate nodes

                List<Guid> badNodes = new List<Guid>();

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    XmlAttribute lacingAttrib = null;
                    if (elNode.Attributes.Count > 5)
                    {
                        lacingAttrib = elNode.Attributes[5];
                    }

                    string typeName = typeAttrib.Value;

                    string oldNamespace = "Dynamo.Elements.";
                    if (typeName.StartsWith(oldNamespace))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, oldNamespace.Length);

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

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    //Type t = Type.GetType(typeName);
                    TypeLoadData tData;
                    Type t;

                    if (!controller.BuiltInTypesByName.TryGetValue(typeName, out tData))
                    {
                        //try and get a system type by this name
                        t = Type.GetType(typeName);

                        //if we still can't find the type, try the also known as attributes
                        if (t == null)
                        {
                            //try to get the also known as values
                            foreach (KeyValuePair<string, TypeLoadData> kvp in controller.BuiltInTypesByName)
                            {
                                var akaAttribs = kvp.Value.Type.GetCustomAttributes(typeof(AlsoKnownAsAttribute), false);
                                if (akaAttribs.Count() > 0)
                                {
                                    if ((akaAttribs[0] as AlsoKnownAsAttribute).Values.Contains(typeName))
                                    {
                                        controller.DynamoViewModel.Log(string.Format("Found matching node for {0} also known as {1}", kvp.Key, typeName));
                                        t = kvp.Value.Type;
                                    }
                                }
                            }
                        }

                        if (t == null)
                        {
                            controller.DynamoViewModel.Log("Could not load node of type: " + typeName);
                            controller.DynamoViewModel.Log("Loading will continue but nodes might be missing from your workflow.");

                            //return false;
                            badNodes.Add(guid);
                            continue;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNodeModel el = dynSettings.Controller.DynamoViewModel.CreateNodeInstance(t, nickname, guid);

                    if (lacingAttrib != null)
                    {
                        LacingStrategy lacing = LacingStrategy.First;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        el.ArgumentLacing = lacing;
                    }

                    // note - this is because the connectors fail to be created if there's not added
                    // to the canvas
                    ws.Nodes.Add(el);
                    el.WorkSpace = ws;
                    var node = el;

                    node.X = x;
                    node.Y = y;

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    el.LoadElement(elNode); // inject the node properties from the xml

                    // it has no 
                    if (el is dynFunction)
                    {
                        var fun = el as dynFunction;

                        // we've found a custom node, we need to attempt to load its guid.  
                        // if it doesn't exist (i.e. its a legacy node), we need to assign it one,
                        // deterministically
                        Guid funId;
                        try
                        {
                            funId = Guid.Parse(fun.Symbol);
                        }
                        catch
                        {
                            funId = GuidUtility.Create(GuidUtility.UrlNamespace, nicknameAttrib.Value);
                            fun.Symbol = funId.ToString();
                        }

                        // if it's not a recurisve node and it's not yet loaded, load it
                        if (funcDefGuid != funId && !this.loadedNodes.ContainsKey(funId))
                        {
                            dynSettings.Controller.CustomNodeLoader.GetFunctionDefinition(funId);
                            fun.Definition = this.loadedNodes[funId];
                        }  
                        else if ( this.loadedNodes.ContainsKey(funId ))
                        {
                            fun.Definition = this.loadedNodes[funId];
                        }
                        
                    }
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
                    dynNodeModel start = null;
                    dynNodeModel end = null;

                    if (badNodes.Contains(guidStart) || badNodes.Contains(guidEnd))
                        continue;

                    foreach (dynNodeModel e in ws.Nodes)
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
                        if (start != null && end != null && start != end)
                        {
                            var newConnector = new dynConnectorModel(
                                start, end,
                                startIndex, endIndex,
                                portType, false
                                );

                            ws.Connectors.Add(newConnector);
                        }
                    }
                    catch
                    {
                        DynamoCommands.WriteToLogCmd.Execute(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NickName, end.NickName));
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
                        double x = Convert.ToDouble(xAttrib.Value);
                        double y = Convert.ToDouble(yAttrib.Value);

                        //dynNoteView n = Bench.AddNote(text, x, y, ws);
                        //Bench.AddNote(text, x, y, ws);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);
                        dynSettings.Controller.DynamoViewModel.AddNoteCommand.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNodeModel e in ws.Nodes)
                    e.EnableReporting();

                ws.FilePath = xmlPath;

                controller.PackageManagerClient.LoadPackageHeader(def, funName);

                var expression = CompileFunction(def);
                controller.FSchemeEnvironment.DefineSymbol(def.FunctionId.ToString(), expression);

            }
            catch (Exception ex)
            {
                DynamoCommands.WriteToLogCmd.Execute("There was an error opening the workbench.");
                DynamoCommands.WriteToLogCmd.Execute(ex);
                def = null;
                return false;
            }

            return true;
        }

        public static FScheme.Expression CompileFunction( FunctionDefinition definition )
        {
            IEnumerable<string> ins = new List<string>();
            IEnumerable<string> outs = new List<string>();

            return CompileFunction(definition, ref ins, ref outs);
        }

        public static FScheme.Expression CompileFunction( FunctionDefinition definition, ref IEnumerable<string> inputNames, ref IEnumerable<string> outputNames )
        {
       
            if (definition == null)
                return null;

            // Get the internal nodes for the function
            dynWorkspaceModel functionWorkspace = definition.Workspace;

            #region Find outputs

            // Find output elements for the node
            IEnumerable<dynNodeModel> outputs = functionWorkspace.Nodes.Where(x => x is dynOutput);

            var topMost = new List<Tuple<int, dynNodeModel>>();

            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => x.Inputs[0]));

                outputNames = outputs.Select(x => (x as dynOutput).Symbol);
            }
            else
            {
                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as tht output
                IEnumerable<dynNodeModel> topMostNodes = functionWorkspace.GetTopMostNodes();

                var outNames = new List<string>();

                foreach (dynNodeModel topNode in topMostNodes)
                {
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

            //Find function entry point, and then compile the function and add it to our environment
            IEnumerable<dynNodeModel> variables = functionWorkspace.Nodes.Where(x => x is dynSymbol);
            inputNames = variables.Select(x => (x as dynSymbol).Symbol);

            INode top;
            var buildDict = new Dictionary<dynNodeModel, Dictionary<int, INode>>();

            if (topMost.Count > 1)
            {
                InputNode node = new ExternalFunctionNode(
                    FScheme.Value.NewList,
                    Enumerable.Range(0, topMost.Count).Select(x => x.ToString()));

                int i = 0;
                foreach (var topNode in topMost)
                {
                    string inputName = i.ToString();
                    node.ConnectInput(inputName, topNode.Item2.Build(buildDict, topNode.Item1));
                    i++;
                }

                top = node;
            }
            else if (topMost.Count == 1)
            {
                top = topMost[0].Item2.BuildExpression(buildDict);
            }
            else
            {
                // if the custom node is empty, it will initially be a number node
                top = new NumberNode(0);
            }
                
            // if the node has any outputs, we create a BeginNode in order to evaluate all of them
            // sequentially (begin evaluates a list of expressions)
            if (outputs.Any())
            {
                var beginNode = new BeginNode();
                List<dynNodeModel> hangingNodes = functionWorkspace.GetTopMostNodes().ToList();

                foreach (var tNode in hangingNodes.Select((x, index) => new { Index = index, Node = x }))
                {
                    beginNode.AddInput(tNode.Index.ToString());
                    beginNode.ConnectInput(tNode.Index.ToString(), tNode.Node.Build(buildDict, 0));
                }

                beginNode.AddInput(hangingNodes.Count.ToString());
                beginNode.ConnectInput(hangingNodes.Count.ToString(), top);


                top = beginNode;
            }

            // make the anonymous function
            FScheme.Expression expression = Utils.MakeAnon(variables.Select(x => x.GUID.ToString()),
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
            foreach (string c in chars)
                s = s.Replace(c, "");
            return s;
        }



    }
}
