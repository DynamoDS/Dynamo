using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Utilities;

namespace Dynamo
{
    /// <summary>
    /// A singleton object representing a dynamo model.
    /// </summary>
    public class DynamoModel
    {
        private ObservableCollection<dynWorkspace> _workSpaces = new ObservableCollection<dynWorkspace>();
        private static DynamoModel _instance;
        private dynWorkspace _cspace;

        public event EventHandler CurrentOffsetChanged;
        protected virtual void OnCurrentOffsetChanged(object sender, EventArgs e)
        {
            if (CurrentOffsetChanged != null)
                CurrentOffsetChanged(this, e);
        }

        public static DynamoModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DynamoModel();
                }
                return _instance;
            }
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

        public IEnumerable<dynNode> AllNodes
        {
            get
            {
                return HomeSpace.Nodes.Concat(
                    dynSettings.FunctionDict.Values.Aggregate(
                        (IEnumerable<dynNode>)new List<dynNode>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        /// <summary>
        /// A collection of workspaces in the dynamo model.
        /// </summary>
        public ObservableCollection<dynWorkspace> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }

        public List<dynNode> Nodes
        {
            get { return CurrentSpace.Nodes; }
        }

        /// <summary>
        /// Construct a Dynamo Model and create a home space.
        /// </summary>
        public DynamoModel()
        {
            HomeSpace = CurrentSpace = new HomeWorkspace();
            _workSpaces.Add(HomeSpace);            
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddHomeWorkspace()
        {
            HomeWorkspace workspace = new HomeWorkspace();
            _workSpaces.Add(workspace);
        }

        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(dynWorkspace workspace)
        {
            _workSpaces.Remove(workspace);
        }
    
        public void SaveWorkspace()
        {
            throw new NotImplementedException();
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

        /// <summary>
        ///     Update a custom node after refactoring.  Updates search and all instances of the node.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void RefactorCustomNode()
        {
            string newName = Bench.editNameBox.Text;

            if (dynSettings.FunctionDict.Values.Any(x => x.Workspace.Name == newName))
            {
                Bench.Log("ERROR: Cannot rename to \"" + newName + "\", node with same name already exists.");
                return;
            }

            Bench.workspaceLabel.Content = Bench.editNameBox.Text;
            SearchViewModel.Refactor(CurrentSpace, newName);

            //Update existing function nodes
            foreach (dynNode el in AllNodes)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction)el;

                    if (node.Definition == null)
                    {
                        node.Definition = dynSettings.FunctionDict[Guid.Parse(node.Symbol)];
                    }

                    if (!node.Definition.Workspace.Name.Equals(CurrentSpace.Name))
                        continue;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NodeUI.NickName.Equals(CurrentSpace.Name))
                        node.NodeUI.NickName = newName;
                }
            }

            FSchemeEnvironment.RemoveSymbol(CurrentSpace.Name);

            //TODO: Delete old stored definition
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = Path.Combine(pluginsPath, CurrentSpace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath = FormatFileName(
                        Path.Combine(pluginsPath, newName + ".dyf")
                        );

                    File.Move(oldpath, newpath);
                }
            }

            (CurrentSpace).Name = newName;

            SaveFunction(dynSettings.FunctionDict.Values.First(x => x.Workspace == CurrentSpace));
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
                    SaveWorkspace(path, functionWorkspace);
                    SearchViewModel.Add(definition.Workspace);
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

                // make it accessible in the FScheme environment
                FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), expression);

                //Update existing function nodes which point to this function to match its changes
                foreach (dynNode el in AllNodes)
                {
                    if (el is dynFunction)
                    {
                        var node = (dynFunction)el;

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
                SaveWorkspace(path, functionWorkspace);
                return path;
            }
            catch (Exception e)
            {
                Bench.Log("Error saving:" + e.GetType());
                Bench.Log(e);
                return "";
            }

        }

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentSpace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        internal void SaveAs(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                if (!SaveWorkspace(path, CurrentSpace))
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
            if (!String.IsNullOrEmpty(CurrentSpace.FilePath))
                SaveAs(CurrentSpace.FilePath);
        }

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
            SaveFunction(dynSettings.FunctionDict.Values.FirstOrDefault(x => x.Workspace == CurrentSpace));

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

            // TODO: get this out of here
            PackageManagerClient.HidePackageControlInformation();

            Bench.workspaceLabel.Content = "Home";
            //Bench.editNameButton.Visibility = Visibility.Collapsed;
            //Bench.editNameButton.IsHitTestVisible = false;

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
                //Step 2: Store function workspace in the function dictionary
                //this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                //Step 3: Save function
                SaveFunction(dynSettings.FunctionDict.Values.First(x => x.Workspace == CurrentSpace));
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

                        FunctionDefinition funcDef;
                        if (dynSettings.FunctionDict.TryGetValue(funId, out funcDef))
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

                    foreach (dynNode e in Dynamo.Nodes)
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
            List<dynNode> elements = Dynamo.Nodes.ToList();

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
                foreach (dynPortModel p in el.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Kill();
                }
                foreach (dynPortModel port in el.OutPorts)
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
    }

    public class DynamoModelUpdateArgs : EventArgs
    {
        public object Item { get; set; }

        public DynamoModelUpdateArgs(object item)
        {
            Item = item;
        }
    }
}
