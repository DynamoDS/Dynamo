using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.Utilties;

namespace Dynamo
{
    public class DynamoController : INotifyPropertyChanged
    {

        #region properties

        public readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        public readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        private readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        private string UnlockLoadPath;
        private dynWorkspace _cspace;
        private bool isProcessingCommandQueue = false;

        public CustomNodeLoader CustomNodeLoader { get; internal set; }
        public SearchViewModel SearchViewModel { get; internal set; }
        public PackageManagerLoginViewModel PackageManagerLoginViewModel { get; internal set; }
        public PackageManagerPublishViewModel PackageManagerPublishViewModel { get; internal set; }
        public PackageManagerClient PackageManagerClient { get; internal set; }

        private bool runEnabled = true;
        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                runEnabled = value;
                NotifyPropertyChanged("RunEnabled");
            }
        }

        List<UIElement> clipBoard = new List<UIElement>();
        public List<UIElement> ClipBoard
        {
            get { return clipBoard; }
            set { clipBoard = value; }
        }

        public bool IsProcessingCommandQueue
        {
            get { return isProcessingCommandQueue; }
        }

        public Queue<Tuple<object, object>> CommandQueue
        {
            get { return commandQueue; }
        }

        public dynBench Bench { get; private set; }

        public IEnumerable<dynNode> AllNodes
        {
            get
            {
                return HomeSpace.Nodes.Concat(
                    this.CustomNodeLoader.GetLoadedDefinitions().Aggregate(
                        (IEnumerable<dynNode>)new List<dynNode>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        public dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                _cspace = value;
                //Bench.CurrentX = _cspace.PositionX;
                //Bench.CurrentY = _cspace.PositionY;
                if (Bench != null)
                    Bench.CurrentOffset = new Point(_cspace.PositionX, _cspace.PositionY);

                //TODO: Also set the name here.
            }
        }

        public dynWorkspace HomeSpace { get; protected set; }

        public ExecutionEnvironment FSchemeEnvironment { get; private set; }

        public List<dynNode> Nodes
        {
            get { return CurrentSpace.Nodes; }
        }

        public bool ViewingHomespace
        {
            get { return CurrentSpace == HomeSpace; }
        }

        private bool _benchActivated;
        public DynamoSplash SplashScreen { get; set; }

        #endregion

        #region Constructor and Initialization

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(ExecutionEnvironment env)
        {
            dynSettings.Controller = this;

            this.RunEnabled = true;
            this.CanRunDynamically = true;

            Bench = new dynBench(this);
            dynSettings.Bench = Bench;

            // custom node loader
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            CustomNodeLoader = new CustomNodeLoader(pluginsPath);

            SearchViewModel = new SearchViewModel();
            PackageManagerClient = new PackageManagerClient(this);
            PackageManagerLoginViewModel = new PackageManagerLoginViewModel(PackageManagerClient);
            PackageManagerPublishViewModel = new PackageManagerPublishViewModel(PackageManagerClient);

            FSchemeEnvironment = env;

            HomeSpace = CurrentSpace = new HomeWorkspace();
            Bench.CurrentOffset = new Point(dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);

            Bench.InitializeComponent();
            Bench.Log(String.Format(
                "Dynamo -- Build {0}.",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.LoadBuiltinTypes(SearchViewModel, this, Bench);
            DynamoLoader.LoadSamplesMenu(Bench);

            Bench.settings_curves.IsChecked = true;
            Bench.settings_curves.IsChecked = false;

            Bench.LockUI();

            Bench.Activated += OnBenchActivated;
            dynSettings.Workbench = Bench.WorkBench;

            //run tests
            if (FScheme.RunTests(Bench.Log))
            {
                if (Bench != null)
                    Bench.Log("All Tests Passed. Core library loaded OK.");
            }
        }

        /// <summary>
        ///     This callback is executed when the BenchUI has completed activation.
        /// </summary>
        /// <parameter>The sender (presumably the Bench) </parameter>
        /// <parameter>Any arguments it passes</parameter>
        private void OnBenchActivated(object sender, EventArgs e)
        {
            if (!_benchActivated)
            {
                _benchActivated = true;

                DynamoLoader.LoadCustomNodes(dynSettings.Bench, this.CustomNodeLoader, this.SearchViewModel);

                Bench.Log("Welcome to Dynamo!");

                if (UnlockLoadPath != null && !OpenWorkbench(UnlockLoadPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Bench.Log("Workbench could not be opened.");

                    //dynSettings.Writer.WriteLine("Workbench could not be opened.");
                    //dynSettings.Writer.WriteLine(UnlockLoadPath);

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(UnlockLoadPath);
                    }
                }

                UnlockLoadPath = null;

                Bench.UnlockUI();
                DynamoCommands.ShowSearch.Execute(null);
                
                HomeSpace.OnDisplayed();

                Bench.WorkBench.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region CommandQueue
    
        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            ProcessCommandQueue();
        }

        /// <summary>
        ///     Run all of the commands in the CommandQueue
        /// </summary>
        public void ProcessCommandQueue()
        {
            if (Bench != null)
            {
                dynSettings.Writer.WriteLine(string.Format("Bench Thread : {0}",
                                                       Bench.Dispatcher.Thread.ManagedThreadId.ToString()));
            }

            while (commandQueue.Count > 0)
            {
                var cmdData = commandQueue.Dequeue();
                var cmd = cmdData.Item1 as ICommand;
                if (cmd != null)
                {
                    if (cmd.CanExecute(cmdData.Item2))
                    {
                        cmd.Execute(cmdData.Item2);
                    }
                }
            }
            commandQueue.Clear();
        }

        /// <summary>
        ///     Sets the load path
        /// </summary>
        internal void QueueLoad(string path)
        {
            UnlockLoadPath = path;
        }

        #endregion




        #region Node Initialization

        /// <summary>
        ///     Create a build-in node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated dynNode</returns>
        public static dynNode CreateBuiltInNodeInstance(Type elementType, string nickName, Guid guid )
        {
            var node = (dynNode)Activator.CreateInstance(elementType);

            dynNodeUI nodeUI = node.NodeUI;

            if (!string.IsNullOrEmpty(nickName))
            {
                nodeUI.NickName = nickName;
            }
            else
            {
                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    nodeUI.NickName = elNameAttrib.Name;
                }
            }

            nodeUI.GUID = guid;

            string name = nodeUI.NickName;
            return node;
        }


        /// <summary>
        ///     Create a node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <param name="x"> The x coordinate where the dynNodeUI will be placed </param>
        /// <param name="y"> The x coordinate where the dynNodeUI will be placed</param>
        /// <returns> The newly instantiate dynNode</returns>
        public dynNode CreateInstanceAndAddNodeToWorkspace( Type elementType, string nickName, Guid guid,
            double x, double y, dynWorkspace ws,
            Visibility vis = Visibility.Visible)
        {
            try
            {
                var node = DynamoController.CreateBuiltInNodeInstance(elementType, nickName, guid);
                var nodeUI = node.NodeUI;

                //store the element in the elements list
                ws.Nodes.Add(node);
                node.WorkSpace = ws;

                nodeUI.Visibility = vis;

                Bench.WorkBench.Children.Add(nodeUI);

                Canvas.SetLeft(nodeUI, x);
                Canvas.SetTop(nodeUI, y);

                //create an event on the element itself
                //to update the elements ports and connectors
                //nodeUI.PreviewMouseRightButtonDown += new MouseButtonEventHandler(UpdateElement);

                return node;
            }
            catch (Exception e)
            {
                Bench.Log("Could not create an instance of the selected type: " + elementType);
                Bench.Log(e);
                return null;
            }
        }

        internal FunctionDefinition NewFunction(Guid id, 
                                                string name, 
                                                string category, 
                                                bool display, 
                                                double workspaceOffsetX = dynBench.CANVAS_OFFSET_X, 
                                                double workspaceOffsetY = dynBench.CANVAS_OFFSET_Y )
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(
                name, category, workspaceOffsetX, workspaceOffsetY);

            List<dynNode> newElements = workSpace.Nodes;
            List<dynConnector> newConnectors = workSpace.Connectors;

            var functionDefinition = new FunctionDefinition(id)
                {
                    Workspace = workSpace
                };

            this.CustomNodeLoader.AddFunctionDefinition(functionDefinition.FunctionId, functionDefinition);

            // add the element to search
            SearchViewModel.Add(workSpace);

            if (display)
            {
                if (!ViewingHomespace)
                {
                    var def = dynSettings.Controller.CustomNodeLoader.GetDefinitionFromWorkspace(CurrentSpace);
                    if (def != null)
                        SaveFunction( def );
                }

                DynamoController.hideWorkspace(CurrentSpace);
                CurrentSpace = workSpace;

                Bench.homeButton.IsEnabled = true;

                Bench.workspaceLabel.Content = CurrentSpace.Name;

                Bench.editNameButton.Visibility = Visibility.Visible;
                Bench.editNameButton.IsHitTestVisible = true;
                Bench.setFunctionBackground();
                dynSettings.ReturnFocusToSearch();
            }

            return functionDefinition;
        }

        protected virtual dynFunction CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs,
                                                     FunctionDefinition functionDefinition)
        {
            return new dynFunction(inputs, outputs, functionDefinition);
        }

        internal dynNode CreateNodeInstance(string name)
        {
            dynNode result;

            if (builtinTypesByTypeName.ContainsKey(name))
            {
                TypeLoadData tld = builtinTypesByTypeName[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (dynNode) obj.Unwrap();
                newEl.NodeUI.DisableInteraction();
                result = newEl;
            }
            else if (builtinTypesByNickname.ContainsKey(name))
            {
                TypeLoadData tld = builtinTypesByNickname[name];

                try
                {

                    ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                    var newEl = (dynNode)obj.Unwrap();
                    newEl.NodeUI.DisableInteraction();
                    result = newEl;
                }
                catch (Exception ex)
                {
                    Bench.Log("Failed to load built-in type");
                    Bench.Log(ex);
                    result = null;
                }
            }
            else
            {
                dynFunction func;

                if (CustomNodeLoader.GetNodeInstance(this, Guid.Parse(name), out func))
                {
                    result = func;
                }
                else
                {
                    Bench.Log("Failed to find FunctionDefinition.");
                    return null;
                }

            }

            return result;
        }

        #endregion

        #region Saving and Opening Workspaces

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentSpace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        internal void SaveAs(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var guid = Guid.Empty;
                if (CurrentSpace != this.HomeSpace)
                    guid = this.CustomNodeLoader.GetGuidFromName(CurrentSpace.Name);

                if (!SaveWorkspace(path, CurrentSpace, guid))
                {
                    Bench.Log("Workbench could not be saved.");
                }
                else
                {
                    CurrentSpace.FilePath = path;
                }
            }
        }

        /// <summary>
        ///     Attempts to save an element, assuming that the CurrentSpace.FilePath 
        ///     field is already  populated with a path has a filename associated with it. 
        /// </summary>
        internal void Save()
        {
            if (!string.IsNullOrEmpty(CurrentSpace.FilePath))
                SaveAs(CurrentSpace.FilePath);
        }

        /// <summary>
        ///     Generate the xml doc of the workspace from memory
        /// </summary>
        /// <param name="workSpace">The workspace</param>
        /// <returns>The generated xmldoc</returns>
        public static XmlDocument GetXmlDocFromWorkspace(dynWorkspace workSpace, bool savingHomespace, Guid functionId )
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement("dynWorkspace"); //write the root element
                root.SetAttribute("X", workSpace.PositionX.ToString());
                root.SetAttribute("Y", workSpace.PositionY.ToString());

                if (!savingHomespace) //If we are not saving the home space
                {
                    root.SetAttribute("Name", workSpace.Name);
                    root.SetAttribute("Category", ((FuncWorkspace) workSpace).Category);
                    root.SetAttribute("ID", functionId.ToString() );
                }

                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements"); //write the root element
                root.AppendChild(elementList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    XmlElement dynEl = xmlDoc.CreateElement(el.GetType().ToString());
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.NodeUI.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NodeUI.NickName);
                    dynEl.SetAttribute("x", Canvas.GetLeft(el.NodeUI).ToString());
                    dynEl.SetAttribute("y", Canvas.GetTop(el.NodeUI).ToString());

                    el.SaveElement(xmlDoc, dynEl);
                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors"); //write the root element
                root.AppendChild(connectorList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    foreach (dynPort port in el.NodeUI.OutPorts)
                    {
                        foreach (dynConnector c in port.Connectors.Where(c => c.Start != null && c.End != null))
                        {
                            XmlElement connector = xmlDoc.CreateElement(c.GetType().ToString());
                            connectorList.AppendChild(connector);
                            connector.SetAttribute("start", c.Start.Owner.GUID.ToString());
                            connector.SetAttribute("start_index", c.Start.Index.ToString());
                            connector.SetAttribute("end", c.End.Owner.GUID.ToString());
                            connector.SetAttribute("end_index", c.End.Index.ToString());

                            if (c.End.PortType == PortType.INPUT)
                                connector.SetAttribute("portType", "0");
                        }
                    }
                }

                //save the notes
                XmlElement noteList = xmlDoc.CreateElement("dynNotes"); //write the root element
                root.AppendChild(noteList);
                foreach (dynNote n in workSpace.Notes)
                {
                    XmlElement note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.noteText.Text);
                    note.SetAttribute("x", Canvas.GetLeft(n).ToString());
                    note.SetAttribute("y", Canvas.GetTop(n).ToString());
                }

                return xmlDoc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        ///     Generate an xml doc and write the workspace to the given path
        /// </summary>
        /// <param name="xmlPath">The path to save to</param>
        /// <param name="workSpace">The workspace</param>
        /// <returns>Whether the operation was successful</returns>
        private bool SaveWorkspace(string xmlPath, dynWorkspace workSpace, Guid funId)
        {
            Bench.Log("Saving " + xmlPath + "...");
            try
            {
                var xmlDoc = GetXmlDocFromWorkspace(workSpace, workSpace == HomeSpace, funId);
                xmlDoc.Save(xmlPath);

                //cache the file path for future save operations
                workSpace.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
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
                    SaveWorkspace(path, functionWorkspace, definition.FunctionId);
                }
                catch (Exception e)
                {
                    Bench.Log("Error saving:" + e.GetType());
                    Bench.Log(e);
                }
            }

            
            try
            {
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
                    foreach (var tNode in hangingNodes.Select((x, index) => new {Index = index, Node = x}))
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

                // make it accessible in the FScheme environment
                FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), expression);

                //Update existing function nodes which point to this function to match its changes
                foreach (dynNode el in AllNodes)
                {
                    if (el is dynFunction)
                    {
                        var node = (dynFunction) el;

                        if (node.Definition != definition)
                            continue;

                        node.SetInputs(inputNames);
                        node.SetOutputs(outputNames);
                        el.NodeUI.RegisterAllPorts();
                    }
                }

                //Call OnSave for all saved elements
                foreach (dynNode el in functionWorkspace.Nodes)
                    el.onSave();

            }
            catch (Exception ex)
            {
                Bench.Log(ex.GetType() + ": " + ex.Message);
            }

        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        /// <param name="definition">The definition to saveo</param>
        /// <param name="bool">Whether to write the function to file</param>
        /// <returns>Whether the operation was successful</returns>
        public string SaveFunctionOnly(FunctionDefinition definition)
        {
            if (definition == null)
                return "";

            // Get the internal nodes for the function
            dynWorkspace functionWorkspace = definition.Workspace;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            try
            {
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                string path = Path.Combine(pluginsPath, FormatFileName(functionWorkspace.Name) + ".dyf");
                SaveWorkspace(path, functionWorkspace, definition.FunctionId);
                return path;
            }
            catch (Exception e)
            {
                Bench.Log("Error saving:" + e.GetType());
                Bench.Log(e);
                return "";
            }

        }

        private static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] {"\\", "/", ":", "*", "?", "\"", "<", ">", "|"}
                );
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            foreach (string c in chars)
                s = s.Replace(c, "");
            return s;
        }

        internal bool OpenDefinition(string xmlPath)
        {
            return OpenDefinition(
                xmlPath,
                new Dictionary<Guid, HashSet<FunctionDefinition>>(),
                new Dictionary<Guid, HashSet<Guid>>());
        }

        internal bool OpenDefinition(
            string xmlPath,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
        {
            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = dynBench.CANVAS_OFFSET_X;
                double cy = dynBench.CANVAS_OFFSET_Y;
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

                //If there is no function name, then we are opening a home definition
                if (funName == null)
                {
                    //View the home workspace, then open the bench file
                    if (!ViewingHomespace)
                        ViewHomeWorkspace(); //TODO: Refactor
                    return OpenWorkbench(xmlPath);
                }
                else if ( this.CustomNodeLoader.Contains(funName) )
                {
                    Bench.Log("ERROR: Could not load definition for \"" + funName +
                              "\", a node with this name already exists.");
                    return false;
                }

                Bench.Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                FunctionDefinition def = NewFunction(
                    Guid.Parse(id),
                    funName,
                    category.Length > 0
                        ? category
                        : BuiltinNodeCategories.MISC,
                    false, cx, cy
                    );

                dynWorkspace ws = def.Workspace;

                //this.Log("Opening definition " + xmlPath + "...");

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

                    //Type t = Type.GetType(typeName);
                    TypeLoadData tData;
                    Type t;

                    if (!builtinTypesByTypeName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Bench.Log("Error loading definition. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNode el = CreateInstanceAndAddNodeToWorkspace(t, nickname, guid, x, y, ws, Visibility.Hidden);

                    if (el == null)
                        return false;

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

                        if (dynSettings.Controller.CustomNodeLoader.IsInitialized(funId))
                            fun.Definition = dynSettings.Controller.CustomNodeLoader.GetFunctionDefinition(funId);
                        else
                            dependencies.Push(funId);
                    }
                }

                #endregion

                Bench.WorkBench.UpdateLayout();

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

                    //don't connect if the end element is an instance map
                    //those have a morphing set of inputs
                    //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                    //if (endTest != null)
                    //{
                    //    continue;
                    //}

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
                    catch
                    {
                        Bench.Log(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NodeUI.GUID, end.NodeUI.GUID));
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

                        //dynNote n = Bench.AddNote(text, x, y, ws);
                        //Bench.AddNote(text, x, y, ws);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNode e in ws.Nodes)
                    e.EnableReporting();

                hideWorkspace(ws);

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
                        children[dep] = new HashSet<FunctionDefinition> {def};

                    //Ws -> Deps
                    if (parents.ContainsKey(def.FunctionId))
                        parents[def.FunctionId].Add(dep);
                    else
                        parents[def.FunctionId] = new HashSet<Guid> {dep};
                }

                if (canLoad)
                    SaveFunction(def, false);

                PackageManagerClient.LoadPackageHeader(def, funName);
                nodeWorkspaceWasLoaded(def, children, parents);

            }
            catch (Exception ex)
            {
                Bench.Log("There was an error opening the workbench.");
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }

            return true;
        }
        
        public void nodeWorkspaceWasLoaded(
            FunctionDefinition def,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
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
                        SaveFunction(child, false);
                        nodeWorkspaceWasLoaded(child, children, parents);
                    }
                }
            }
        }

        public static void hideWorkspace(dynWorkspace ws)
        {
            foreach (dynNode e in ws.Nodes)
                e.NodeUI.Visibility = Visibility.Collapsed;
            foreach (dynConnector c in ws.Connectors)
                c.Visible = false;
            foreach (dynNote n in ws.Notes)
                n.Visibility = Visibility.Hidden;
        }

        public bool OpenWorkbench(string xmlPath)
        {
            Bench.Log("Opening home workspace " + xmlPath + "...");
            CleanWorkbench();

            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                        {
                            //Bench.CurrentX = Convert.ToDouble(att.Value);
                            Bench.CurrentOffset = new Point(Convert.ToDouble(att.Value), Bench.CurrentOffset.Y);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            //Bench.CurrentY = Convert.ToDouble(att.Value);
                            Bench.CurrentOffset = new Point(Bench.CurrentOffset.X, Convert.ToDouble(att.Value));
                        }
                    }
                }

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

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

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    if (typeName.StartsWith("Dynamo.Elements."))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, 16);

                    TypeLoadData tData;
                    Type t;

                    if (!builtinTypesByTypeName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Bench.Log("Error loading workspace. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNode el = CreateInstanceAndAddNodeToWorkspace(
                        t, nickname, guid, x, y,
                        CurrentSpace
                        );

                    el.DisableReporting();

                    el.LoadElement(elNode);

                    if (ViewingHomespace)
                        el.SaveResult = true;

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

                        FunctionDefinition funcDef = this.CustomNodeLoader.GetFunctionDefinition(funId);
                        if (funcDef != null)
                            fun.Definition = funcDef;
                        else
                            fun.NodeUI.Error("No definition found.");
                    }

                    //read the sub elements
                    //set any numeric values 
                    //foreach (XmlNode subNode in elNode.ChildNodes)
                    //{
                    //   if (subNode.Name == "System.Double")
                    //   {
                    //      double val = Convert.ToDouble(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //   else if (subNode.Name == "System.Int32")
                    //   {
                    //      int val = Convert.ToInt32(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //}
                }

                dynSettings.Workbench.UpdateLayout();

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

                    foreach (dynNode e in Nodes)
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

                    //don't connect if the end element is an instance map
                    //those have a morphing set of inputs
                    //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                    //if (endTest != null)
                    //{
                    //    continue;
                    //}

                    if (start != null && end != null && start != end)
                    {
                        var newConnector = new dynConnector(start.NodeUI, end.NodeUI,
                                                            startIndex, endIndex, portType);

                        CurrentSpace.Connectors.Add(newConnector);
                    }
                }

                CurrentSpace.Connectors.ForEach(x => x.Redraw());

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

                        //dynNote n = Bench.AddNote(text, x, y, this.CurrentSpace);
                        //Bench.AddNote(text, x, y, this.CurrentSpace);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", CurrentSpace);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNode e in CurrentSpace.Nodes)
                    e.EnableReporting();

                #endregion

                HomeSpace.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Bench.Log("There was an error opening the workbench.");
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            return true;
        }

        internal void CleanWorkbench()
        {
            Bench.Log("Clearing workflow...");

            //Copy locally
            List<dynNode> elements = Nodes.ToList();

            foreach (dynNode el in elements)
            {
                el.DisableReporting();
                try
                {
                    el.Destroy();
                }
                catch
                {
                }
            }

            foreach (dynNode el in elements)
            {
                foreach (dynPort p in el.NodeUI.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Kill();
                }
                foreach (dynPort port in el.NodeUI.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].Kill();
                }

                dynSettings.Workbench.Children.Remove(el.NodeUI);
            }

            foreach (dynNote n in CurrentSpace.Notes)
            {
                dynSettings.Workbench.Children.Remove(n);
            }

            CurrentSpace.Nodes.Clear();
            CurrentSpace.Connectors.Clear();
            CurrentSpace.Notes.Clear();
            CurrentSpace.Modified();
        }

        #endregion

        #region Running

        //protected bool _debug;
        private bool _showErrors;

        protected bool canRunDynamically = true;
        protected bool debug = false;

        protected bool dynamicRun = false;
        private bool runAgain;
        public bool Running { get; protected set; }

        public bool RunCancelled { get; protected internal set; }

        public virtual bool CanRunDynamically
        {
            get
            {
                //we don't want to be able to run
                //dynamically if we're in debug mode
                return !debug;
            }
            set
            {
                canRunDynamically = value;
                NotifyPropertyChanged("CanRunDynamically");
            }
        }

        public virtual bool DynamicRunEnabled
        {
            get { return dynamicRun; //selecting debug now toggles this on/off
            }
            set
            {
                dynamicRun = value;
                NotifyPropertyChanged("DynamicRunEnabled");
            }
        }

        public virtual bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                //toggle off dynamic run
                CanRunDynamically = !debug;

                if (debug)
                    DynamicRunEnabled = false;

                NotifyPropertyChanged("RunInDebug");
            }
        }

        internal void QueueRun()
        {
            RunCancelled = true;
            runAgain = true;
        }

        public void RunExpression(bool showErrors = true)
        {
            //If we're already running, do nothing.
            if (Running)
                return;

            _showErrors = showErrors;

            //TODO: Hack. Might cause things to break later on...
            //Reset Cancel and Rerun flags
            RunCancelled = false;
            runAgain = false;

            //We are now considered running
            Running = true;

            //Set run auto flag
            //this.DynamicRunEnabled = !showErrors;

            //Setup background worker
            var worker = new BackgroundWorker();
            worker.DoWork += EvaluationThread;

            //Disable Run Button

            //Bench.Dispatcher.Invoke(new Action(
            //   delegate { Bench.RunButton.IsEnabled = false; }
            //));

            this.RunEnabled = false;

            //Let's start
            worker.RunWorkerAsync();
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            IEnumerable<dynNode> topElements = HomeSpace.GetTopMostNodes();

            //Mark the topmost as dirty/clean
            foreach (dynNode topMost in topElements)
                topMost.MarkDirty();

            //TODO: Flesh out error handling
            try
            {
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();
                foreach (dynNode topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));

                    i++;
                }
                

                FScheme.Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.CancelRun = false; //Reset cancel flag
                RunCancelled = true;

                //If we are forcing this, then make sure we don't run again either.
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { Bench.Log(ex); }
                                                ));
                }

                OnRunCancelled(true);

                //Reset the flags
                runAgain = false;
                RunCancelled = true;
            }
            finally
            {
                /* Post-evaluation cleanup */

                //Re-enable run button
                //Bench.Dispatcher.Invoke(new Action(
                //   delegate
                //   {
                //       Bench.RunButton.IsEnabled = true;
                //   }
                //));

                this.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (FunctionDefinition def in dynSettings.FunctionWasEvaluated)
                    def.RequiresRecalc = false;

                //If we should run again...
                if (runAgain)
                {
                    //Reset flag
                    runAgain = false;

                    //Run this method again from the main thread
                    Bench.Dispatcher.BeginInvoke(new Action(
                                                     delegate { RunExpression(_showErrors); }
                                                     ));
                }
            }
        }

        protected internal virtual void Run(IEnumerable<dynNode> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (debug)
            {
                //string exp = FScheme.print(runningExpression);
                Bench.Dispatcher.Invoke(new Action(
                                            delegate
                                                {
                                                    foreach (dynNode node in topElements)
                                                    {
                                                        string exp = node.PrintExpression();
                                                        Bench.Log("> " + exp);
                                                    }
                                                }
                                            ));
            }

            try
            {
                //Evaluate the expression
                FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

                //Print some more stuff if we're in debug mode
                if (debug && expr != null)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                () => Bench.Log(FScheme.print(expr))
                                                ));
                }
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.RunCancelled = false;
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                //Print unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { Bench.Log(ex); }
                                                ));
                }
                OnRunCancelled(true);
                RunCancelled = true;
                runAgain = false;
            }

            OnEvaluationCompleted();
        }

        protected virtual void OnRunCancelled(bool error)
        {
            if (error)
                dynSettings.FunctionWasEvaluated.Clear();
        }

        protected virtual void OnEvaluationCompleted()
        {
        }

        internal void ShowElement(dynNode e)
        {
            if (dynamicRun)
                return;

            if (!Nodes.Contains(e))
            {
                if (HomeSpace != null && HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    ViewHomeWorkspace();
                }
                else
                {
                    foreach (FunctionDefinition funcDef in this.CustomNodeLoader.GetLoadedDefinitions() )
                    {
                        if (funcDef.Workspace.Nodes.Contains(e))
                        {
                            ViewCustomNodeWorkspace(funcDef);
                            break;
                        }
                    }
                }
            }

            Bench.CenterViewOnElement(e.NodeUI);
        }

        #endregion

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewHomeWorkspace()
        {
            //Step 1: Make function workspace invisible
            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Collapsed;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Hidden;
            }

            //Step 3: Save function
            SaveFunction( this.CustomNodeLoader.GetDefinitionFromWorkspace(CurrentSpace) );//dynSettings.FunctionDict.Values.FirstOrDefault(x => x.Workspace == CurrentSpace) );

            //Step 4: Make home workspace visible
            CurrentSpace = HomeSpace;

            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Visible;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = true;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Visible;
            }

            Bench.homeButton.IsEnabled = false;

            PackageManagerClient.HidePackageControlInformation();

            Bench.workspaceLabel.Content = "Home";
            Bench.editNameButton.Visibility = Visibility.Collapsed;
            Bench.editNameButton.IsHitTestVisible = false;

            Bench.setHomeBackground();

            CurrentSpace.OnDisplayed();
        }

        /// <summary>
        ///     Change the currently visible workspace to a custom node's workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewCustomNodeWorkspace(FunctionDefinition symbol)
        {
            if (symbol == null || CurrentSpace.Name.Equals(symbol.Workspace.Name))
                return;

            dynWorkspace newWs = symbol.Workspace;

            //Make sure we aren't dragging
            Bench.WorkBench.isDragInProgress = false;
            Bench.WorkBench.ignoreClick = true;

            //Step 1: Make function workspace invisible
            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Collapsed;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Hidden;
            }
            //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

            if (!ViewingHomespace)
            {
                //Step 3: Save function
                SaveFunction(CustomNodeLoader.GetDefinitionFromWorkspace(newWs));
            }

            CurrentSpace = newWs;

            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Visible;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = true;
            }

            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Visible;
            }

            //this.saveFuncItem.IsEnabled = true;
            Bench.homeButton.IsEnabled = true;
            //this.varItem.IsEnabled = true;

            Bench.workspaceLabel.Content = symbol.Workspace.Name;

            Bench.editNameButton.Visibility = Visibility.Visible;
            Bench.editNameButton.IsHitTestVisible = true;

            Bench.setFunctionBackground();

            PackageManagerClient.ShowPackageControlInformation();

            CurrentSpace.OnDisplayed();
        }

        /// <summary>
        ///     Update a custom node after refactoring.  Updates search and all instances of the node.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void RefactorCustomNode()
        {
            string newName = Bench.editNameBox.Text;

            if ( this.CustomNodeLoader.Contains(newName) )
            {
                Bench.Log("ERROR: Cannot rename to \"" + newName + "\", node with same name already exists.");
                return;
            }

            var def = CustomNodeLoader.GetDefinitionFromWorkspace(CurrentSpace);
            var oldName = def.Workspace.Name;

            // this is a load of crap.  nodes should observe the name property of the node and update without the need for this
            Bench.workspaceLabel.Content = Bench.editNameBox.Text;

            //Update existing function nodes
            foreach (dynNode el in AllNodes)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction) el;

                    if (node.Definition == null)
                    {
                        node.Definition = this.CustomNodeLoader.GetFunctionDefinition(Guid.Parse(node.Symbol)); //dynSettings.FunctionDict[Guid.Parse( node.Symbol ) ];
                    }

                    if (!node.Definition.Workspace.Name.Equals(CurrentSpace.Name))
                        continue;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NodeUI.NickName.Equals(CurrentSpace.Name))
                        node.NodeUI.NickName = newName;
                }
            }

            FSchemeEnvironment.RemoveSymbol(CurrentSpace.Name);

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = Path.Combine(pluginsPath, CurrentSpace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath =
                        Path.Combine(pluginsPath, FormatFileName(newName) + ".dyf");

                    File.Move(oldpath, newpath);
                }
            }

            (CurrentSpace).Name = newName;
            SaveFunction( def );

            SearchViewModel.Refactor(def, oldName, newName);
        }

        /// <summary>
        ///     Collapse a set of nodes in the current workspace.  Has the side effects of prompting the user
        ///     first in order to obtain the name and category for the new node, 
        ///     writes the function to a dyf file, adds it to the FunctionDict, adds it to search, and compiles and 
        ///     places the newly created symbol (defining a lambda) in the Controller's FScheme Environment.  
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void CollapseNodes(IEnumerable<dynNode> selectedNodes)
        {
            Dynamo.Utilities.NodeCollapser.Collapse(selectedNodes, CurrentSpace);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Used by various properties to notify observers that a property has changed.
        /// </summary>
        /// <param name="info">What changed.</param>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
