using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Microsoft.Practices.Prism;
using NUnit.Framework;
using Enum = System.Enum;
using String = System.String;

namespace Dynamo.Models
{
     
    public delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);
    public delegate void CleanupHandler(object sender, EventArgs e);
    public delegate void NodeHandler(NodeModel node);
    public delegate void WorkspaceHandler(WorkspaceModel model);

    #region Helper types

    public class WorkspaceHeader
    {
        private WorkspaceHeader()
        {

        }

        public static WorkspaceHeader FromPath(string path)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                string funName = null;
                double cx = 0;
                double cy = 0;
                double zoom = 1.0;
                string id = "";

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("Workspace"))
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
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }
                    }
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName) && funName != "Home")
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }


                return new WorkspaceHeader() { ID = id, Name = funName, X = cx, Y = cy, Zoom = zoom, FilePath = path };


            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("There was an error opening the workbench.");
                DynamoLogger.Instance.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);

                if (dynSettings.Controller.Testing)
                    Assert.Fail(ex.Message);

                return null;
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Zoom { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string FilePath { get; set; }

        public bool IsCustomNodeWorkspace()
        {
            return !String.IsNullOrEmpty(ID);
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

    public class FunctionNamePromptEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool Success { get; set; }

        public FunctionNamePromptEventArgs()
        {
            Name = "";
            Category = "";
            Description = "";
        }
    }

    #endregion

    /// <summary>
    /// The Dynamo model.
    /// </summary>
    public class DynamoModel : ModelBase
    {
        #region properties and fields

        public event EventHandler RequestLayoutUpdate;
        public virtual void OnRequestLayoutUpdate(object sender, EventArgs e)
        {
            if (RequestLayoutUpdate != null)
                RequestLayoutUpdate(this, e);
        }

        public event FunctionNamePromptRequestHandler RequestsFunctionNamePrompt;
        public virtual void OnRequestsFunctionNamePrompt(Object sender, FunctionNamePromptEventArgs e)
        {
            if (RequestsFunctionNamePrompt != null)
            {
                RequestsFunctionNamePrompt(this, e);
            }
        }

        private ObservableCollection<WorkspaceModel> _workSpaces = new ObservableCollection<WorkspaceModel>();
        private ObservableCollection<WorkspaceModel> _hiddenWorkspaces = new ObservableCollection<WorkspaceModel>();
        public string UnlockLoadPath { get; set; }
        private WorkspaceModel _cspace;
        internal string editName = "";
        private List<Migration> _migrations = new List<Migration>();

        /// <summary>
        /// Event called when a workspace is hidden
        /// </summary>
        public event WorkspaceHandler WorkspaceHidden;

        /// <summary>
        /// Event called when a node is added to a workspace
        /// </summary>
        public event NodeHandler NodeAdded;

        /// <summary>
        /// Event called when a node is deleted
        /// </summary>
        public event NodeHandler NodeDeleted;

        public WorkspaceModel CurrentWorkspace
        {
            get { return _cspace; }
            internal set
            {
                if (_cspace != null)
                    _cspace.IsCurrentSpace = false;
                _cspace = value;
                _cspace.IsCurrentSpace = true;
                RaisePropertyChanged("CurrentWorkspace");
            }
        }

        public WorkspaceModel HomeSpace { get; protected set; }

        /// <summary>
        ///     The collection of visible workspaces in Dynamo
        /// </summary>
        public ObservableCollection<WorkspaceModel> Workspaces
        {
            get { return _workSpaces; }
            set 
            { 
                _workSpaces = value;
            }
        }

        public List<NodeModel> Nodes
        {
            get { return CurrentWorkspace.Nodes.ToList(); }
        }

        public static bool RunEnabled { get; set; }

        public static bool RunInDebug { get; set; }

        /// <summary>
        /// All nodes in all workspaces. 
        /// </summary>
        public IEnumerable<NodeModel> AllNodes
        {
            get
            {
                return Workspaces.Aggregate((IEnumerable<NodeModel>)new List<NodeModel>(), (a, x) => a.Concat(x.Nodes))
                    .Concat(dynSettings.Controller.CustomNodeManager.GetLoadedDefinitions().Aggregate(
                        (IEnumerable<NodeModel>)new List<NodeModel>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        /// <summary>
        /// An event triggered when the workspace is being cleaned.
        /// </summary>
        public event CleanupHandler CleaningUp;

        internal List<Migration> Migrations
        {
            get { return _migrations; }
            set { _migrations = value; }
        }

        #endregion

        public DynamoModel()
        {
            Migrations.Add(new Migration(new Version("0.5.3.0"), new Action(Migrate_0_5_3_to_0_6_0)));
        }

        /// <summary>
        /// Run every migration for a model version before current.
        /// </summary>
        public void ProcessMigrations()
        {
            var migrations =
                Migrations.Where(x => x.Version < HomeSpace.WorkspaceVersion || x.Version == null)
                          .OrderBy(x => x.Version);
            
            foreach (var migration in migrations)
            {
                migration.Upgrade.Invoke();
            }
        }

        private void Migrate_0_5_3_to_0_6_0()
        {
            DynamoLogger.Instance.LogWarning("Applying model migration from 0.5.3.x to 0.6.0.x", WarningLevel.Mild);
        }

        public virtual void OnCleanup(EventArgs e)
        {
            if (CleaningUp != null)
                CleaningUp(this, e);
        }

        /// <summary>
        /// Present the open dialogue and open the workspace that is selected.
        /// </summary>
        /// <param name="parameter"></param>
        public void ShowOpenDialogAndOpenResult(object parameter)
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            if (vm.Model.HomeSpace.HasUnsavedChanges && !vm.AskUserToSaveWorkspaceOrCancel(vm.Model.HomeSpace))
            {
                return;
            }

            FileDialog _fileDialog = null;

            if (_fileDialog == null)
            {
                _fileDialog = new OpenFileDialog()
                {
                    Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*",
                    Title = "Open Dynamo Definition..."
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FilePath))
            {
                var fi = new FileInfo(vm.Model.CurrentWorkspace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }
            else // use the samples directory, if it exists
            {
                Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
                string location = Path.GetDirectoryName(dynamoAssembly.Location);
                string path = Path.Combine(location, "samples");

                if (Directory.Exists(path))
                {
                    _fileDialog.InitialDirectory = path;
                }
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                //if (OpenCommand.CanExecute(_fileDialog.FileName))
                //    OpenCommand.Execute(_fileDialog.FileName);
                if (CanOpen(_fileDialog.FileName))
                    Open(_fileDialog.FileName);
            }
        }

        internal bool CanShowOpenDialogAndOpenResultCommand(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Open a definition or workspace.
        /// </summary>
        /// <param name="parameters">The path the the file.</param>
        public void Open(object parameters)
        {
            string xmlPath = parameters as string;

            dynSettings.Controller.IsUILocked = true;

            if (!OpenDefinition(xmlPath))
            {
                DynamoLogger.Instance.Log("Workbench could not be opened.");

                if (CanWriteToLog(null))
                {
                    WriteToLog("Workbench could not be opened.");
                    WriteToLog(xmlPath);
                }
            }

            dynSettings.Controller.IsUILocked = false;

            //clear the clipboard to avoid copying between dyns
            dynSettings.Controller.ClipBoard.Clear();
        }

        internal bool CanOpen(object parameters)
        {
            if (string.IsNullOrEmpty(parameters.ToString()))
                return false;
            return true;
        }

        internal void PostUIActivation(object parameter)
        {

            DynamoLoader.LoadCustomNodes();

            DynamoLogger.Instance.Log("Welcome to Dynamo!");

            if (UnlockLoadPath != null && !OpenWorkspace(UnlockLoadPath))
            {
                DynamoLogger.Instance.Log("Workbench could not be opened.");

                if (CanWriteToLog(null))
                {
                    WriteToLog("Workbench could not be opened.");
                    WriteToLog(UnlockLoadPath);
                }
            }

            UnlockLoadPath = null;
            dynSettings.Controller.IsUILocked = false;
            HomeSpace.OnDisplayed();

        }

        internal bool CanDoPostUIActivation(object parameter)
        {
            return true;
        }

        internal void OpenCustomNodeAndFocus( WorkspaceHeader workspaceHeader )
        {
            // load custom node
            var manager = dynSettings.Controller.CustomNodeManager;
            var info = manager.AddFileToPath(workspaceHeader.FilePath);
            var funcDef = manager.GetFunctionDefinition(info.Guid);
            var ws = funcDef.Workspace;
            ws.Zoom = workspaceHeader.Zoom;
            ws.HasUnsavedChanges = false;

            if (!this.Workspaces.Contains(ws))
            {
                this.Workspaces.Add(ws);
            }

            var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == ws);
            vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(workspaceHeader.X, workspaceHeader.Y)));

            this.CurrentWorkspace = ws;
        }   
        
        internal bool OpenDefinition( string xmlPath )
        {
            var workspaceInfo = WorkspaceHeader.FromPath(xmlPath);

            if (workspaceInfo == null)
            {
                return false;
            }

            if (workspaceInfo.IsCustomNodeWorkspace())
            {
                OpenCustomNodeAndFocus(workspaceInfo);
                return true;
            }
            else
            {
                //View the home workspace, then open the bench file
                if (!dynSettings.Controller.DynamoViewModel.ViewingHomespace)
                    ViewHomeWorkspace();

                var dirName = Path.GetDirectoryName(xmlPath);
                dynSettings.Controller.CustomNodeManager.AddDirectoryToSearchPath(dirName);
                dynSettings.Controller.CustomNodeManager.UpdateSearchPath();

                return OpenWorkspace(xmlPath);
            }

        }

        public void HideWorkspace(WorkspaceModel workspace)
        {
            this.CurrentWorkspace = _workSpaces[0];  // go home
            _workSpaces.Remove(workspace);
            OnWorkspaceHidden(workspace);
            _hiddenWorkspaces.Add(workspace);
        }

        /// <summary>
        /// Called when a workspace is hidden
        /// </summary>
        /// <param name="workspace"></param>
        private void OnWorkspaceHidden(WorkspaceModel workspace)
        {
            if (WorkspaceHidden != null)
            {
                WorkspaceHidden(workspace);
            }
        }

        /// <summary>
        /// Replace the home workspace with a new 
        /// workspace. Only valid if the home workspace is already
        /// defined (usually by calling AddHomeWorkspace).
        /// </summary>
        public void NewHomeWorkspace()
        {
            if (this.Workspaces.Count > 0 && this.HomeSpace != null)
            {
                //var homeIndex = this._workSpaces.IndexOf(this.HomeSpace);
                //var newHomespace = new HomeWorkspace();
                //this.Workspaces[0] = newHomespace;
                //this.HomeSpace = newHomespace;
                //this.CurrentWorkspace = newHomespace;

                this.AddHomeWorkspace();
                _cspace = this.HomeSpace;
                this.CurrentWorkspace = this.HomeSpace;
                this.Workspaces.RemoveAt(1);
            }
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddHomeWorkspace()
        {
            var workspace = new HomeWorkspace()
            {
                WatchChanges = true
            };
            HomeSpace = workspace;
            _workSpaces.Insert(0, workspace); // to front
        }

        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            _workSpaces.Remove(workspace);
        }

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewHomeWorkspace()
        {
            CurrentWorkspace = HomeSpace;
            CurrentWorkspace.OnDisplayed();
        }

        /// <summary>
        ///     Create a node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <param name="x"> The x coordinate where the dynNodeView will be placed </param>
        /// <param name="y"> The x coordinate where the dynNodeView will be placed</param>
        /// <returns> The newly instantiate dynNode</returns>
        public NodeModel CreateInstanceAndAddNodeToWorkspace(Type elementType, string nickName, Guid guid,
            double x, double y, WorkspaceModel ws, bool isVisible = true, bool isUpstreamVisible = true)    //Visibility vis = Visibility.Visible)
        {
            try
            {
                NodeModel node = CreateNodeInstance(elementType, nickName, guid);

                ws.Nodes.Add(node);
                node.WorkSpace = ws;

                node.X = x;
                node.Y = y;

                node.IsVisible = isVisible;
                node.IsUpstreamVisible = isUpstreamVisible;

                OnNodeAdded(node);

                return node;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Could not create an instance of the selected type: " + elementType);
                DynamoLogger.Instance.Log(e);
                return null;
            }
        }

        /// <summary>
        /// Called when a node is added to a workspace
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ws"></param>
        private void OnNodeAdded(NodeModel node)
        {
            if (NodeAdded != null && node != null)
            {
                NodeAdded(node);
            }
        }

        /// <summary>
        ///     Create a build-in node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated dynNode</returns>
        public NodeModel CreateNodeInstance(Type elementType, string nickName, Guid guid)
        {
            var node = (NodeModel)Activator.CreateInstance(elementType);

            if (!string.IsNullOrEmpty(nickName))
            {
                node.NickName = nickName;
            }
            else
            {
                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    node.NickName = elNameAttrib.Name;
                }
            }

            node.GUID = guid;

            //string name = nodeUI.NickName;
            return node;
        }

        internal void CleanWorkbench()
        {
            DynamoLogger.Instance.Log("Clearing workflow...");

            //Copy locally
            List<NodeModel> elements = Nodes.ToList();

            foreach (NodeModel el in elements)
            {
                el.DisableReporting();
                //try
                //{
                //    el.Destroy();
                //}
                //catch
                //{
                //}
            }

            foreach (NodeModel el in elements)
            {
                foreach (PortModel p in el.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].NotifyConnectedPortsOfDeletion();
                }
                foreach (PortModel port in el.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].NotifyConnectedPortsOfDeletion();
                }
            }

            CurrentWorkspace.Connectors.Clear();
            CurrentWorkspace.Nodes.Clear();
            CurrentWorkspace.Notes.Clear();
        }

        /// <summary>
        /// Open a workspace from a path.
        /// </summary>
        /// <param name="xmlPath">The path to the workspace.</param>
        /// <returns></returns>
        public bool OpenWorkspace(string xmlPath)
        {
            DynamoLogger.Instance.Log("Opening home workspace " + xmlPath + "...");

            CleanWorkbench();

            //clear the renderables
            dynSettings.Controller.RenderDescriptions.Clear();
            dynSettings.Controller.OnRequestsRedraw(dynSettings.Controller, EventArgs.Empty);

            Stopwatch sw = new Stopwatch();

            try
            {
                #region read xml file

                sw.Start();

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                TimeSpan previousElapsed = sw.Elapsed;
                DynamoLogger.Instance.Log(string.Format("{0} elapsed for loading xml.", sw.Elapsed));

                double cx = 0;
                double cy = 0;
                double zoom = 1.0;
                string version = "";

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
                        {
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("zoom"))
                        {
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Version"))
                        {
                            version = att.Value;
                        }
                    }
                }

                //set the zoom and offsets and trigger events
                //to get the view to position iteself
                CurrentWorkspace.X = cx;
                CurrentWorkspace.Y = cy;
                CurrentWorkspace.Zoom = zoom;

                var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == CurrentWorkspace);
                vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(cx, cy)));

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

                //if there is any problem loading a node, then
                //add the node's guid to the bad nodes collection
                //so we can avoid attempting to make connections to it
                List<Guid> badNodes = new List<Guid>();

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes["type"];
                    XmlAttribute guidAttrib = elNode.Attributes["guid"];
                    XmlAttribute nicknameAttrib = elNode.Attributes["nickname"];
                    XmlAttribute xAttrib = elNode.Attributes["x"];
                    XmlAttribute yAttrib = elNode.Attributes["y"];
                    XmlAttribute isVisAttrib = elNode.Attributes["isVisible"];
                    XmlAttribute isUpstreamVisAttrib = elNode.Attributes["isUpstreamVisible"];
                    XmlAttribute lacingAttrib = elNode.Attributes["lacing"];

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

                    typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                    System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);
                    if (null == type)
                    {
                        badNodes.Add(guid);
                        continue;
                    }

                    bool isVisible = true;
                    if (isVisAttrib != null)
                        isVisible = isVisAttrib.Value == "true" ? true : false;

                    bool isUpstreamVisible = true;
                    if (isUpstreamVisAttrib != null)
                        isUpstreamVisible = isUpstreamVisAttrib.Value == "true" ? true : false;

                    NodeModel el = CreateNodeInstance(type, nickname, guid);
                    el.WorkSpace = CurrentWorkspace;
                    el.Load(elNode);

                    CurrentWorkspace.Nodes.Add(el);

                    el.X = x;
                    el.Y = y;

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;

                    if (lacingAttrib != null)
                    {
                        if (el.ArgumentLacing != LacingStrategy.Disabled)
                        {
                            LacingStrategy lacing = LacingStrategy.Disabled;
                            Enum.TryParse(lacingAttrib.Value, out lacing);
                            el.ArgumentLacing = lacing;
                        }
                    }

                    el.DisableReporting();

                    if (CurrentWorkspace == HomeSpace)
                        el.SaveResult = true;
                }

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading nodes.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                OnRequestLayoutUpdate(this, EventArgs.Empty);

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for updating layout.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

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

                    foreach (NodeModel e in Nodes)
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

                    var newConnector = ConnectorModel.Make(start, end,
                                                        startIndex, endIndex, portType);

                    Stopwatch addTimer = new Stopwatch();
                    addTimer.Start();
                    if (newConnector != null)
                        CurrentWorkspace.Connectors.Add(newConnector);
                    addTimer.Stop();
                    Debug.WriteLine(string.Format("{0} elapsed for add connector to collection.", addTimer.Elapsed));

                }

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading connectors.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                        double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", CurrentWorkspace);
                        
                        AddNote(paramDict);
                    }
                }

                #endregion

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading notes.", sw.Elapsed - previousElapsed));

                foreach (NodeModel e in CurrentWorkspace.Nodes)
                    e.EnableReporting();

                if(!string.IsNullOrEmpty(version))
                    CurrentWorkspace.WorkspaceVersion = new Version(version);
                dynSettings.Controller.DynamoModel.ProcessMigrations();

                #endregion

                HomeSpace.FilePath = xmlPath;

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading workspace.", sw.Elapsed));
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("There was an error opening the workbench.");
                DynamoLogger.Instance.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            CurrentWorkspace.HasUnsavedChanges = false;
            return true;
        }

        internal FunctionDefinition NewFunction(Guid id,
                                        string name,
                                        string category,
                                        string description,
                                        bool display,
                                        double workspaceOffsetX = 0,
                                        double workspaceOffsetY = 0)
        {

            var workSpace = new FuncWorkspace(
                name, category, description, workspaceOffsetX, workspaceOffsetY)
            {
                WatchChanges = true
            };

            Workspaces.Add(workSpace);

            workSpace.Nodes.ToList();
            workSpace.Connectors.ToList();

            var functionDefinition = new FunctionDefinition(id)
            {
                Workspace = workSpace
            };

            dynSettings.Controller.DynamoModel.SaveFunction(functionDefinition, false, true, true);

            if (display)
            {
                if (CurrentWorkspace != HomeSpace)
                {
                    var def = dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(CurrentWorkspace);
                    if (def != null)
                        SaveFunction(def, false, true, true);
                }

                CurrentWorkspace = workSpace;
            }

            return functionDefinition;
        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        public void SaveFunction(FunctionDefinition definition, bool writeDefinition = true, bool addToSearch = false, bool compileFunction = true)
        {
            if (definition == null)
                return;

            // Get the internal nodes for the function
            var functionWorkspace = definition.Workspace;

            string path = definition.Workspace.FilePath;
            // If asked to, write the definition to file
            if (writeDefinition && !String.IsNullOrEmpty(path))
            {
                //var pluginsPath = dynSettings.Controller.CustomNodeManager.GetDefaultSearchPath();

                //if (!Directory.Exists(pluginsPath))
                //    Directory.CreateDirectory(pluginsPath);

                //path = Path.Combine(pluginsPath, dynSettings.FormatFileName(functionWorkspace.Name) + ".dyf");

                WorkspaceModel.SaveWorkspace(path, functionWorkspace);
            }

            try
            {
                dynSettings.Controller.CustomNodeManager.AddFunctionDefinition(definition.FunctionId, definition);

                if (addToSearch)
                {
                    dynSettings.Controller.SearchViewModel.Add(
                        functionWorkspace.Name, 
                        functionWorkspace.Category,
                        functionWorkspace.Description, 
                        definition.FunctionId);
                }

                var info = new CustomNodeInfo(definition.FunctionId, functionWorkspace.Name, functionWorkspace.Category,
                    functionWorkspace.Description, path);
                dynSettings.Controller.CustomNodeManager.SetNodeInfo(info);

                #region Compile Function and update all nodes

                IEnumerable<string> inputNames;
                IEnumerable<string> outputNames;

                var compiledFunction = CustomNodeManager.CompileFunction(definition, out inputNames, out outputNames);

                if (compiledFunction == null)
                    return;

                dynSettings.Controller.FSchemeEnvironment.DefineSymbol(
                    definition.FunctionId.ToString(),
                    compiledFunction);

                //Update existing function nodes which point to this function to match its changes
                foreach (Function node in AllNodes.OfType<Function>().Where(el => el.Definition == definition))
                {
                    node.SetInputs(inputNames);
                    node.SetOutputs(outputNames);
                    node.RegisterAllPorts();
                }

                //Call OnSave for all saved elements
                foreach (NodeModel el in functionWorkspace.Nodes)
                    el.onSave();

                #endregion

            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Error saving:" + e.GetType());
                DynamoLogger.Instance.Log(e);
            }


        }

        /// <summary>
        /// Write a message to the log.
        /// </summary>
        /// <param name="parameters">The message.</param>
        public void WriteToLog(object parameters)
        {
            if (parameters == null) return;
            string logText = parameters.ToString();
            DynamoLogger.Instance.Log(logText);
        }

        internal bool CanWriteToLog(object parameters)
        {
            if (DynamoLogger.Instance != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a note to the workspace.
        /// </summary>
        /// <param name="parameters">A dictionary containing placement data for the note</param>
        /// <example>{"x":1234.0,"y":1234.0, "guid":1234-1234-...,"text":"the note's text","workspace":workspace </example>
        public void AddNote(object parameters)
        {
            NoteModel noteModel = AddNoteInternal(parameters);
            if (null != noteModel)
                CurrentWorkspace.RecordCreatedModel(noteModel);
        }

        internal bool CanAddNote(object parameters)
        {
            return true;
        }

        #region Undo/Redo Supporting Methods

        internal void Undo(object parameters)
        {
            if (null != _cspace)
                _cspace.Undo();

            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        internal bool CanUndo(object parameters)
        {
            return ((null == _cspace) ? false : _cspace.CanUndo);
        }

        internal void Redo(object parameters)
        {
            if (null != _cspace)
                _cspace.Redo();

            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        internal bool CanRedo(object parameters)
        {
            return ((null == _cspace) ? false : _cspace.CanRedo);
        }

        #endregion

        /// <summary>
        /// Copy selected ISelectable objects to the clipboard.
        /// </summary>
        /// <param name="parameters"></param>
        public void Copy(object parameters)
        {
            dynSettings.Controller.ClipBoard.Clear();

            foreach (ISelectable sel in DynamoSelection.Instance.Selection)
            {
                //MVVM : selection and clipboard now hold view model objects
                //UIElement el = sel as UIElement;
                ModelBase el = sel as ModelBase;
                if (el != null)
                {
                    if (!dynSettings.Controller.ClipBoard.Contains(el))
                    {
                        dynSettings.Controller.ClipBoard.Add(el);

                        //dynNodeView n = el as dynNodeView;
                        NodeModel n = el as NodeModel;
                        if (n != null)
                        {
                            var connectors = n.InPorts.ToList().SelectMany(x => x.Connectors)
                                .Concat(n.OutPorts.ToList().SelectMany(x => x.Connectors))
                                .Where(x => x.End != null &&
                                    x.End.Owner.IsSelected &&
                                    !dynSettings.Controller.ClipBoard.Contains(x));

                            dynSettings.Controller.ClipBoard.AddRange(connectors);
                        }
                    }
                }
            }
        }

        internal bool CanCopy(object parameters)
        {
            if (DynamoSelection.Instance.Selection.Count == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Paste ISelectable objects from the clipboard to the workspace.
        /// </summary>
        /// <param name="parameters"></param>
        public void Paste(object parameters)
        {
            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            var nodeLookup = new Dictionary<Guid, Guid>();

            //make a list of all newly created models so that their
            //creations can be recorded in the undo recorder.
            var createdModels = new List<ModelBase>();

            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.Selection.RemoveAll();

            var nodes = dynSettings.Controller.ClipBoard.OfType<NodeModel>();

            var connectors = dynSettings.Controller.ClipBoard.OfType<ConnectorModel>();

            foreach (NodeModel node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();
                nodeLookup.Add(node.GUID, newGuid);

                var nodeData = new Dictionary<string, object>();
                nodeData.Add("x", node.X);
                nodeData.Add("y", node.Y + 100);
                if (node is Function)
                    nodeData.Add("name", (node as Function).Definition.FunctionId);
                else
                    nodeData.Add("name", node.GetType());
                nodeData.Add("guid", newGuid);

                var xmlDoc = new XmlDocument();
                var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
                xmlDoc.AppendChild(dynEl);
                node.Save(xmlDoc, dynEl, SaveContext.Copy);

                nodeData.Add("data", dynEl);
                createdModels.Add(CreateNode_Internal(nodeData));
            }

            //process the command queue so we have 
            //nodes to connect to
            //DynamoCommands.ProcessCommandQueue();

            //update the layout to ensure that the visuals
            //are present in the tree to connect to
            //dynSettings.Bench.UpdateLayout();
            OnRequestLayoutUpdate(this, EventArgs.Empty);

            foreach (ConnectorModel c in connectors)
            {
                var connectionData = new Dictionary<string, object>();

                // if in nodeLookup, the node is paste.  otherwise, use the existing node guid
                Guid startGuid = Guid.Empty;
                Guid endGuid = Guid.Empty;

                startGuid = nodeLookup.TryGetValue(c.Start.Owner.GUID, out startGuid) ? startGuid : c.Start.Owner.GUID;
                endGuid = nodeLookup.TryGetValue(c.End.Owner.GUID, out endGuid) ? endGuid : c.End.Owner.GUID;

                var startNode = CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == startGuid);
                var endNode = CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == endGuid);

                // do not form connector if the end nodes are null
                if (startNode == null || endNode == null)
                {
                    continue;
                }

                //don't let users paste connectors between workspaces
                if (startNode.WorkSpace != CurrentWorkspace)
                {
                    continue;
                }

                connectionData.Add("start", startNode);
                connectionData.Add("end", endNode);

                connectionData.Add("port_start", c.Start.Index);
                connectionData.Add("port_end", c.End.Index);

                createdModels.Add(CreateConnectionInternal(connectionData));
            }

            //process the queue again to create the connectors
            //DynamoCommands.ProcessCommandQueue();

            var notes = dynSettings.Controller.ClipBoard.OfType<NoteModel>();

            foreach (NoteModel note in notes)
            {
                var newGUID = Guid.NewGuid();

                var sameSpace = CurrentWorkspace.Notes.Any(x => x.GUID == note.GUID);
                var newX = sameSpace ? note.X + 20 : note.X;
                var newY = sameSpace ? note.Y + 20 : note.Y;

                var noteData = new Dictionary<string, object>()
                {
                    { "x", newX },
                    { "y", newY },
                    { "text", note.Text },
                    { "guid", newGUID }
                };

                createdModels.Add(AddNoteInternal(noteData));

                // TODO: Why can't we just add "noteData" instead of doing a look-up?
                AddToSelection(CurrentWorkspace.Notes.FirstOrDefault(x => x.GUID == newGUID));
            }

            foreach (var de in nodeLookup)
            {
                AddToSelection(CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == de.Value));
            }

            // Record models that are created as part of the command.
            CurrentWorkspace.RecordCreatedModels(createdModels);
        }

        internal bool CanPaste(object parameters)
        {
            if (dynSettings.Controller.ClipBoard.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add an ISelectable object to the selection.
        /// </summary>
        /// <param name="parameters">The object to add to the selection.</param>
        public void AddToSelection(object parameters)
        {
            var node = parameters as NodeModel;
            
            //don't add if the object is null
            if (node == null)
                return;

            if (!node.IsSelected)
            {
                if (!DynamoSelection.Instance.Selection.Contains(node))
                    DynamoSelection.Instance.Selection.Add(node);
            }
        }

        internal bool CanAddToSelection(object parameters)
        {
            var node = parameters as NodeModel;
            if (node == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Present the new function dialogue and create a custom function.
        /// </summary>
        /// <param name="parameter"></param>
        public void ShowNewFunctionDialogAndMakeFunction(object parameter)
        {
            //trigger the event to request the display
            //of the function name dialogue
            var args = new FunctionNamePromptEventArgs();
            OnRequestsFunctionNamePrompt(this, args);

            //string name = "", category = "";
            //if (ShowNewFunctionDialog(ref name, ref category))
            if (args.Success)
            {
                //NewFunction(Guid.NewGuid(), name, category, true);
                NewFunction(Guid.NewGuid(), args.Name, args.Category, args.Description, true);
            }
        }

        internal bool CanShowNewFunctionDialogCommand(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Create a node.
        /// </summary>
        /// <param name="parameters">A dictionary containing data about the node.</param>
        public void CreateNode(object parameters)
        {
            NodeModel nodeModel = CreateNode_Internal(parameters);
            if (null != nodeModel)
                nodeModel.WorkSpace.RecordCreatedModel(nodeModel);
        }

        internal NodeModel CreateNode_Internal(object parameters)
        {
            var data = parameters as Dictionary<string, object>;
            if (data == null)
            {
                return null;
            }

            NodeModel node = CreateNode(data["name"].ToString());
            if (node == null)
            {
                dynSettings.Controller.DynamoModel.WriteToLog("Failed to create the node");
                return null;
            }

            if ((node is Symbol || node is Output) && CurrentWorkspace is HomeWorkspace)
            {
                dynSettings.Controller.DynamoModel.WriteToLog("Cannot place dynSymbol or dynOutput in HomeWorkspace");
                return null;
            }

            CurrentWorkspace.Nodes.Add(node);
            node.WorkSpace = CurrentWorkspace;

            //if we've received a value in the dictionary
            //try to set the value on the node
            if (data.ContainsKey("data"))
            {
                node.Load(data["data"] as XmlNode);
            }

            //override the guid so we can store
            //for connection lookup
            if (data.ContainsKey("guid"))
            {
                node.GUID = (Guid)data["guid"];
            }
            else
            {
                node.GUID = Guid.NewGuid();
            }

            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestNodeCentered(this, new ModelEventArgs(node, data));

            node.EnableInteraction();

            if (CurrentWorkspace == HomeSpace)
            {
                node.SaveResult = true;
            }

            OnNodeAdded(node);

            return node;
        }

        internal bool CanCreateNode(object parameters)
        {
            var data = parameters as Dictionary<string, object>;

            if (data == null)
                return false;

            Guid guid;
            var name = data["name"].ToString();

            if (dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(name)
                    || dynSettings.Controller.BuiltInTypesByName.ContainsKey(name)
                    || (Guid.TryParse(name, out guid) && dynSettings.Controller.CustomNodeManager.Contains(guid)))
            {
                return true;
            }

            string message = string.Format("Can not create instance of node {0}.", data["name"]);
            dynSettings.Controller.DynamoModel.WriteToLog(message);
            DynamoLogger.Instance.Log(message);

            return false;
        }

        internal NodeModel CreateNode(string name)
        {
            NodeModel result;

            if (dynSettings.Controller.BuiltInTypesByName.ContainsKey(name))
            {
                TypeLoadData tld = dynSettings.Controller.BuiltInTypesByName[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (NodeModel)obj.Unwrap();
                newEl.DisableInteraction();
                result = newEl;
            }
            else if (dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(name))
            {
                TypeLoadData tld = dynSettings.Controller.BuiltInTypesByNickname[name];
                try
                {

                    ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                    var newEl = (NodeModel)obj.Unwrap();
                    newEl.DisableInteraction();
                    result = newEl;
                }
                catch (Exception ex)
                {
                    DynamoLogger.Instance.Log("Failed to load built-in type");
                    DynamoLogger.Instance.Log(ex);
                    result = null;
                }
            }
            else
            {
                Function func;

                if (dynSettings.Controller.CustomNodeManager.GetNodeInstance(Guid.Parse(name), out func))
                {
                    result = func;
                }
                else
                {
                    DynamoLogger.Instance.Log("Failed to find FunctionDefinition.");
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Create a connector.
        /// </summary>
        /// <param name="parameters">A dictionary containing data about the connection.</param>
        public void CreateConnection(object parameters)
        {
            CreateConnectionInternal(parameters);
        }

        /// <summary>
        ///     Save the current workspace to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        internal void SaveAs(string path)
        {
            this.SaveAs(path, CurrentWorkspace);
        }

        /// <summary>
        /// Save the current workspace.
        /// </summary>
        /// <param name="parameters">The file path.</param>
        public void SaveAs(object parameters)
        {
            if (parameters == null)
                return;

            var fi = new FileInfo(parameters.ToString());

            SaveAs(fi.FullName);
        }

        internal bool CanSaveAs(object parameters)
        {
            if (parameters == null)
                return false;

            return true;
        }

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        /// <param name="workspace">The workspace to save</param>
        internal void SaveAs(string path, WorkspaceModel workspace)
        {
            if (!String.IsNullOrEmpty(path))
            {
                // if it's a custom node
                if (workspace is FuncWorkspace)
                {
                    var def = dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(workspace);
                    def.Workspace.FilePath = path;

                    if (def != null)
                    {
                        this.SaveFunction(def, true);
                        workspace.FilePath = path;
                    }
                    return;
                }

                if (!WorkspaceModel.SaveWorkspace(path, workspace))
                {
                    DynamoLogger.Instance.Log("Workbench could not be saved.");
                }
                else
                {
                    workspace.FilePath = path;
                }

            }
        }

        /// <summary>
        ///     Attempts to save an element, assuming that the CurrentWorkspace.FilePath 
        ///     field is already  populated with a path has a filename associated with it. 
        /// </summary>
        public void Save(object parameter)
        {
            if (!String.IsNullOrEmpty(CurrentWorkspace.FilePath))
                SaveAs(CurrentWorkspace.FilePath);
        }

        internal bool CanSave(object parameter)
        {
            return true;
        }

        internal bool CanDelete(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        /// <summary>
        /// Called when a node is deleted
        /// </summary>
        /// <param name="node"></param>
        public void OnNodeDeleted(NodeModel node)
        {
            if (NodeDeleted != null)
            {
                NodeDeleted(node);
            }
        }

        /// <summary>
        ///     Update a custom node after refactoring.  Updates search and all instances of the node.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        public void RefactorCustomNode(object parameter)
        {
            //Bench.workspaceLabel.Content = Bench.editNameBox.Text;
            var def = dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(CurrentWorkspace);

            //TODO: UI Refactor - Is this the right data for refactor?
            var info = new CustomNodeInfo(def.FunctionId, editName, CurrentWorkspace.Category, CurrentWorkspace.Description, CurrentWorkspace.FilePath);

            dynSettings.Controller.SearchViewModel.Refactor(info);

            //Update existing function nodes
            foreach (NodeModel el in AllNodes)
            {
                if (el is Function)
                {
                    var node = (Function)el;

                    if (node.Definition == null)
                    {
                        node.Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(Guid.Parse(node.Symbol));
                    }

                    if (!node.Definition.Workspace.Name.Equals(CurrentWorkspace.Name))
                        continue;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NickName.Equals(CurrentWorkspace.Name))
                        node.NickName = editName;
                }
            }

            dynSettings.Controller.FSchemeEnvironment.RemoveSymbol(CurrentWorkspace.Name);

            //TODO: Delete old stored definition
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = Path.Combine(pluginsPath, CurrentWorkspace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath = dynSettings.FormatFileName(
                        Path.Combine(pluginsPath, editName + ".dyf")
                        );

                    File.Move(oldpath, newpath);
                }
            }

            (CurrentWorkspace).Name = editName;

            SaveFunction(def);
        }

        internal bool CanRefactorCustomNode(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Clear the workspace. Removes all nodes, notes, and connectors from the current workspace.
        /// </summary>
        /// <param name="parameter"></param>
        public void Clear(object parameter)
        {
            dynSettings.Controller.IsUILocked = true;

            CleanWorkbench();

            //don't save the file path
            CurrentWorkspace.FilePath = "";
            CurrentWorkspace.HasUnsavedChanges = false;

            // Clear undo/redo stacks.
            CurrentWorkspace.ClearUndoRecorder();
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();

            //clear the renderables
            dynSettings.Controller.RenderDescriptions.Clear();
            dynSettings.Controller.OnRequestsRedraw(dynSettings.Controller, EventArgs.Empty);

            dynSettings.Controller.IsUILocked = false;
        }

        internal bool CanClear(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Call this method to delete a given ModelBase, or all the models 
        /// in the current selection set within the current active workspace.
        /// </summary>
        /// <param name="parameters">An instance of ModelBase to be deleted 
        /// from the current workspace. If this parameter is null, then all 
        /// the selected nodes will be deleted.</param>
        public void Delete(object parameters)
        {
            if (null == this._cspace)
                return;

            List<ModelBase> modelsToDelete = new List<ModelBase>();

            if (null != parameters) // Something is specified in parameters.
            {
                if (parameters is ModelBase)
                    modelsToDelete.Add(parameters as ModelBase);
            }
            else
            {
                // When 'parameters' is 'null', then it means all selected models.
                foreach (ISelectable selectable in DynamoSelection.Instance.Selection)
                {
                    if (selectable is ModelBase)
                        modelsToDelete.Add(selectable as ModelBase);
                }
            }

            this._cspace.RecordAndDeleteModels(modelsToDelete);

            var selection = DynamoSelection.Instance.Selection;
            foreach (ModelBase model in modelsToDelete)
            {
                selection.Remove(model); // Remove from selection set.
                if (model is NodeModel)
                    OnNodeDeleted(model as NodeModel);
            }
        }

        /// <summary>
        /// View the home workspace.
        /// </summary>
        /// <param name="parameter"></param>
        public void Home(object parameter)
        {
            ViewHomeWorkspace();
        }

        internal bool CanGoHome(object parameter)
        {
            return CurrentWorkspace != HomeSpace;
        }

        #region Private Helper Methods

        private ConnectorModel CreateConnectionInternal(object parameters)
        {
            try
            {
                Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;

                NodeModel start = (NodeModel)connectionData["start"];
                NodeModel end = (NodeModel)connectionData["end"];
                int startIndex = (int)connectionData["port_start"];
                int endIndex = (int)connectionData["port_end"];

                var c = ConnectorModel.Make(start, end, startIndex, endIndex, 0);

                if (c != null)
                    CurrentWorkspace.Connectors.Add(c);

                return c;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e.Message);
                DynamoLogger.Instance.Log(e);
            }

            return null;
        }

        private NoteModel AddNoteInternal(object parameters)
        {
            var inputs = parameters as Dictionary<string, object> ?? new Dictionary<string, object>();

            // by default place note at center
            var x = 0.0;
            var y = 0.0;

            if (inputs != null && inputs.ContainsKey("x"))
                x = (double)inputs["x"];

            if (inputs != null && inputs.ContainsKey("y"))

                y = (double)inputs["y"];

            var n = new NoteModel(x, y);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            if (parameters == null)
            {
                inputs.Add("transformFromOuterCanvasCoordinates", true);
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestNodeCentered(this, new ModelEventArgs(n, inputs));
            }

            object id;
            if (inputs.TryGetValue("guid", out id))
                n.GUID = (Guid)id;

            n.Text = (inputs == null || !inputs.ContainsKey("text")) ? "New Note" : inputs["text"].ToString();
            var ws = (inputs == null || !inputs.ContainsKey("workspace")) ? CurrentWorkspace : (WorkspaceModel)inputs["workspace"];

            ws.Notes.Add(n);
            return n;
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            // I don't think anyone is serializing/deserializing DynamoModel 
            // directly. If that is not the case, please let me know and I'll 
            // fix it.
            throw new NotImplementedException();
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            // I don't think anyone is serializing/deserializing DynamoModel 
            // directly. If that is not the case, please let me know and I'll 
            // fix it.
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PointEventArgs : EventArgs
    {
        public Point Point { get; set; }

        public PointEventArgs(Point p)
        {
            Point = p;
        }
    }

    public class ModelEventArgs : EventArgs
    {
        public ModelBase Model { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public ModelEventArgs(ModelBase n, Dictionary<string, object> d)
        {
            Model = n;
            Data = d;
        }
    }

    public class TypeLoadData
    {
        public Assembly Assembly;
        public Type Type;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            Assembly = assemblyIn;
            Type = typeIn;
        }
    }
}
