using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Nodes;
using System.IO;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
        private Dictionary<string, Guid> nodeNames = new Dictionary<string, Guid>();

        /// <summary>
        /// SearchPath property </summary>
        /// <value>
        /// The name of the node </value>
        public string SearchPath { get; set; }

        #endregion

        /// <summary>
        ///     Class Constructor
        /// </summary>
        /// <param name="searchPath">The path to search for definitions</param>
        public CustomNodeLoader(string searchPath) {
            SearchPath = searchPath;
        }

        /// <summary>
        ///     Enumerates all of the node names.
        /// </summary>
        /// <returns>A list of all of the node names</returns>
        public IEnumerable<string> GetNodeNames()
        {
            return nodeNames.Keys.ToList();
        }

        /// <summary>
        ///     Enumerates all of the node name guid pairs
        /// </summary>
        /// <returns>A list of tuples with the name as first element and guid as second</returns>
        public IEnumerable<Tuple<string, Guid>> GetNodeNameGuidPairs()
        {
            return nodeNames.Keys.Zip(nodeNames.Values, (first, second) => new Tuple<string, Guid>(first, second));
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
                if (GetHeaderFromPath(file, out guid, out name))
                {
                    this.SetNodeNameAndPath(name, guid, file);
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
        private void SetNodePath(Guid id, string path)
        {
            if ( this.Contains( id ) ) {
                this.nodePaths[id] = path;
            } else {
                this.nodePaths.Add(id, path);
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        private void SetNodeNameAndPath(string name, Guid id, string path)
        {
            if ( this.Contains(name) )
            {
                this.nodeNames[name] = id;
            }
            else
            {
                this.nodeNames.Add(name, id);
            }
            this.SetNodePath(id, path);
        }

        /// <summary>
        ///     Get the function definition from a guid
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public FunctionDefinition GetFunctionDefinition(Guid id)
        {
            if ( this.IsInitialized(id) )
            {
                return loadedNodes[id];
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
            return IsInitialized(name) || nodeNames.ContainsKey(name);
        }

        /// <summary>
        ///     Tells whether the custom node is initialized in the manager
        /// </summary>
        /// <param name="name">The name of the node</param>
        /// <returns>The name of the </returns>
        public bool IsInitialized(string name)
        {
            if (this.nodeNames.ContainsKey(name))
            {
                var guid = this.nodeNames[name];
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

            return this.nodeNames[name];

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
                if (!GetDefinitionFromPath(GetNodePath(guid), controller, out def))
                {
                    result = null;
                    return false;
                }
                this.loadedNodes.Add(guid, def);
            } else {
                def = this.loadedNodes[guid];
            }

            dynWorkspace ws = def.Workspace;

            //TODO: Update to base off of Definition

                IEnumerable<string> inputs =
                    ws.Nodes.Where(e => e is dynSymbol)
                        .Select(s => (s as dynSymbol).Symbol);

                IEnumerable<string> outputs =
                    ws.Nodes.Where(e => e is dynOutput)
                        .Select(o => (o as dynOutput).Symbol);

                if (!outputs.Any())
                {
                    var topMost = new List<Tuple<int, dynNode>>();

                    IEnumerable<dynNode> topMostNodes = ws.GetTopMostNodes();

                    foreach (dynNode topNode in topMostNodes)
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
            result.NodeUI.NickName = ws.Name;

            return true;
        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="guid">A reference to the guid (OUT) Guid.Empty if function returns false. </param>
        /// <returns>Whether we successfully obtained the guid or not.  </returns>
        public static bool GetHeaderFromPath(string path, out Guid guid, out string name) {

            try
            {
                var funName = "";
                var id = "";

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
                return true;

            }
            catch
            {

                guid = Guid.Empty;
                name = "";
                return false;

            }

        }

        private bool GetDefinitionFromPath(string xmlPath, DynamoController controller, out FunctionDefinition def)
        {
            return this.GetDefinitionFromPath(  xmlPath,
                                                new Dictionary<Guid, HashSet<FunctionDefinition>>(),
                                                new Dictionary<Guid, HashSet<Guid>>(),
                                                controller,
                                                out def);
        }

        private bool GetDefinitionFromPath( string xmlPath,
                                            Dictionary<Guid, HashSet<FunctionDefinition>> children,
                                            Dictionary<Guid, HashSet<Guid>> parents, 
                                            DynamoController controller,
                                            out FunctionDefinition def )
        {
            try
            {

                var funName = "";
                var category = "";
                var cx = 0.0;
                var cy = 0.0;
                var id = "";

                #region Get xml document and parse

                var xmlDoc = new XmlDocument();
                xmlDoc.Load( xmlPath );

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

                //If there is no function name, then we are opening a dyn
                if (string.IsNullOrEmpty(funName))
                {
                    def = null;
                    return false;
                }

                category = category.Length > 0 ? category : BuiltinNodeCategories.MISC;

                #endregion

                var workSpace = new FuncWorkspace(funName, category, cx, cy);
                def = new FunctionDefinition(Guid.Parse(id))
                {
                    Workspace = workSpace
                };

                dynWorkspace ws = def.Workspace;

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                var dependencies = new Stack<Guid>();

                #region instantiate nodes

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

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


                    TypeLoadData tData;
                    Type t;

                    if (!controller.builtinTypesByTypeName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNode el = DynamoController.CreateNodeInstance(t, nickname, guid);

                    // note - this is because the connectors fail to be created if there's not added
                    // to the canvas
                        ws.Nodes.Add(el);
                        el.WorkSpace = ws;
                        var nodeUI = el.NodeUI;

                        nodeUI.Visibility = Visibility.Visible;

                        dynSettings.Bench.WorkBench.Children.Add(nodeUI);

                        Canvas.SetLeft(nodeUI, x);
                        Canvas.SetTop(nodeUI, y);

                    if (el == null)
                        return false;

                    ws.Nodes.Add(el);
                    el.DisableReporting();
                    el.LoadElement(elNode);

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

                        if (this.IsInitialized(funId))
                        {
                            fun.Definition = this.GetFunctionDefinition(funId);
                        }
                        else
                        {
                            dependencies.Push(funId);
                        }

                        //if ( FunctionDict.TryGetValue(funId, out funcDef) )
                        //    fun.Definition = funcDef;
                        //else
                        //    dependencies.Push(funId);
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
                    dynNode start = null;
                    dynNode end = null;

                    foreach (dynNode e in ws.Nodes)
                    {
                        if (e.NodeUI.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.NodeUI.GUID == guidEnd)
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
                            var newConnector = new dynConnector(
                                start.NodeUI, end.NodeUI,
                                startIndex, endIndex,
                                portType, false
                                );

                            ws.Connectors.Add(newConnector);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }

                #endregion

                #region instantiate notes

                //if (nNodesList != null)
                //{
                //    foreach (XmlNode note in nNodesList.ChildNodes)
                //    {
                //        XmlAttribute textAttrib = note.Attributes[0];
                //        XmlAttribute xAttrib = note.Attributes[1];
                //        XmlAttribute yAttrib = note.Attributes[2];

                //        string text = textAttrib.Value;
                //        double x = Convert.ToDouble(xAttrib.Value);
                //        double y = Convert.ToDouble(yAttrib.Value);

                //        //dynNote n = Bench.AddNote(text, x, y, ws);
                //        //Bench.AddNote(text, x, y, ws);

                //        var paramDict = new Dictionary<string, object>();
                //        paramDict.Add("x", x);
                //        paramDict.Add("y", y);
                //        paramDict.Add("text", text);
                //        paramDict.Add("workspace", ws);
                //        DynamoCommands.AddNoteCmd.Execute(paramDict);
                //    }
                //}

                #endregion

                foreach (dynNode e in ws.Nodes)
                    e.EnableReporting();

                DynamoController.hideWorkspace(ws);

                ws.FilePath = xmlPath;

                bool canLoad = true;

                //For each node this workspace depends on...
                foreach (Guid dep in dependencies)
                {
                    canLoad = false;
                    
                    //Dep -> Ws
                    if (children.ContainsKey(dep))
                        children[dep].Add(def);
                    else
                        children[dep] = new HashSet<FunctionDefinition> { def };

                    //Ws -> Deps
                    if (parents.ContainsKey(def.FunctionId))
                        parents[def.FunctionId].Add(dep);
                    else
                        parents[def.FunctionId] = new HashSet<Guid> { dep };
                }

                if (canLoad) // if all its dependents are loaded, compile it
                {
                    var expression = CompileFunction(def);
                    controller.FSchemeEnvironment.DefineSymbol( def.FunctionId.ToString(), expression);
                }

                //PackageManagerClient.LoadPackageHeader(def, funName);
                nodeWorkspaceWasLoaded(def, children, parents, controller);

            }
            catch
            {
                def = null;
                return false;
            }

            return true;
        }

        private void nodeWorkspaceWasLoaded(
            FunctionDefinition def,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents,
            DynamoController controller )
        {
            //If there were some workspaces that depended on this node...
            if (children.ContainsKey(def.FunctionId))
            {
                //For each workspace...
                foreach (FunctionDefinition child in children[def.FunctionId])
                {
                    //Nodes the workspace depends on
                    HashSet<Guid> allParents = parents[child.FunctionId];
                    //Remove this workspace, since it's now loaded.
                    allParents.Remove(def.FunctionId);
                    //If everything the node depends on has been loaded...
                    if (!allParents.Any())
                    {
                        var expression = CompileFunction(def);
                        controller.FSchemeEnvironment.DefineSymbol(def.FunctionId.ToString(), expression);
                        nodeWorkspaceWasLoaded(child, children, parents, controller);
                    }
                }
            }
        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        /// <param name="definition">The definition to saveo</param>
        /// <param name="bool">Whether to write the function to file</param>
        /// <returns>Whether the operation was successful</returns>
        public void SaveFunction(FunctionDefinition definition, bool writeDefinition = true)
        {
            if (definition == null)
                return;

            // Get the internal nodes for the function
            dynWorkspace functionWorkspace = definition.Workspace;

            // If asked to, write the definition to file
            if (writeDefinition)
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "definitions");

                try
                {
                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    string path = Path.Combine(pluginsPath, FormatFileName(functionWorkspace.Name) + ".dyf");
                    DynamoController.GetXmlDocFromWorkspace(functionWorkspace, false);

                    //SearchViewModel.Add(definition.Workspace);
                }
                catch
                {
                    //Bench.Log("Error saving:" + e.GetType());
                    //Bench.Log(e);
                }
            }
        }

        private static FScheme.Expression CompileFunction( FunctionDefinition definition ) {

            if (definition == null)
                return null;

            // Get the internal nodes for the function
            dynWorkspace functionWorkspace = definition.Workspace;

            #region Find outputs

            // Find output elements for the node
            IEnumerable<dynNode> outputs = functionWorkspace.Nodes.Where(x => x is dynOutput);

            var topMost = new List<Tuple<int, dynNode>>();

            IEnumerable<string> outputNames;

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
                IEnumerable<dynNode> topMostNodes = functionWorkspace.GetTopMostNodes();

                var outNames = new List<string>();

                foreach (dynNode topNode in topMostNodes)
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
                ele.Item2.NodeUI.ValidateConnections();
            }

            //Find function entry point, and then compile the function and add it to our environment
            IEnumerable<dynNode> variables = functionWorkspace.Nodes.Where(x => x is dynSymbol);
            IEnumerable<string> inputNames = variables.Select(x => (x as dynSymbol).Symbol);

            INode top;
            var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();

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
            else
                top = topMost[0].Item2.BuildExpression(buildDict);

            // if the node has any outputs, we create a BeginNode in order to evaluate all of them
            // sequentially (begin evaluates a list of expressions)
            if (outputs.Any())
            {
                var beginNode = new BeginNode();
                List<dynNode> hangingNodes = functionWorkspace.GetTopMostNodes().ToList();
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
            FScheme.Expression expression = Utils.MakeAnon(variables.Select(x => x.NodeUI.GUID.ToString()),
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
