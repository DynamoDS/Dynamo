using System;
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
using System.Windows.Threading;
using System.Xml;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Microsoft.Practices.Prism;
using NUnit.Framework;
using Enum = System.Enum;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Utils = Dynamo.Nodes.Utilities;
using ProtoCore.DSASM;
using Dynamo.ViewModels;
using Dynamo.DSEngine;

namespace Dynamo.Models
{
     
    public delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);
    public delegate void CleanupHandler(object sender, EventArgs e);
    public delegate void NodeHandler(NodeModel node);
    public delegate void ConnectorHandler(ConnectorModel connector);
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

                var topNode = xmlDoc.GetElementsByTagName("Workspace");

                // legacy support
                if (topNode.Count == 0)
                {
                    topNode = xmlDoc.GetElementsByTagName("dynWorkspace");
                }

                // load the header
                foreach (XmlNode node in topNode)
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

                return new WorkspaceHeader() { ID = id, Name = funName, X = cx, Y = cy, Zoom = zoom, FileName = path };

            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("There was an error opening the workbench.");
                DynamoLogger.Instance.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);

                if (DynamoController.IsTestMode)
                    Assert.Fail(ex.Message);

                return null;
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Zoom { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string FileName { get; set; }

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
        public bool CanEditName { get; set; }
        public bool Success { get; set; }

        public FunctionNamePromptEventArgs()
        {
            Name = "";
            Category = "";
            Description = "";
            CanEditName = true;
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

        public event EventHandler WorkspaceOpening;
        public virtual void OnWorkspaceOpening(object sender, EventArgs e)
        {
            if (WorkspaceOpening != null)
            {
                WorkspaceOpening(this, e);
            }
        }

        public event EventHandler WorkspaceOpened;
        public virtual void OnWorkspaceOpened(object sender, EventArgs e)
        {
            if (WorkspaceOpened != null)
            {
                WorkspaceOpened(this, e);
            }
        }

        public event EventHandler WorkspaceClearing;
        public virtual void OnWorkspaceClearing(object sender, EventArgs e)
        {
            if (WorkspaceClearing != null)
            {
                WorkspaceClearing(this, e);
            }
        }

        public event EventHandler WorkspaceCleared;
        public virtual void OnWorkspaceCleared(object sender, EventArgs e)
        {
            if (WorkspaceCleared != null)
            {
                WorkspaceCleared(this, e);
            }
        }

        public event EventHandler DeletionStarted;
        public virtual void OnDeletionStarted(object sender, EventArgs e)
        {
            if (DeletionStarted != null)
            {
                DeletionStarted(this, e);
            }
        }

        public event EventHandler DeletionComplete;
        public virtual void OnDeletionComplete(object sender, EventArgs e)
        {
            if (DeletionComplete != null)
            {
                DeletionComplete(this, e);
            }
        }

        /// <summary>
        /// An event triggered when the workspace is being cleaned.
        /// </summary>
        public event CleanupHandler CleaningUp;
        public virtual void OnCleanup(EventArgs e)
        {
            if (CleaningUp != null)
                CleaningUp(this, e);
        }

        private ObservableCollection<WorkspaceModel> _workSpaces = new ObservableCollection<WorkspaceModel>();
        private ObservableCollection<WorkspaceModel> _hiddenWorkspaces = new ObservableCollection<WorkspaceModel>();
        public string UnlockLoadPath { get; set; }
        private WorkspaceModel _cspace;
        internal string editName = "";

        /// <summary>
        /// Event called when a workspace is hidden
        /// </summary>
        public event WorkspaceHandler WorkspaceHidden;

        /// <summary>
        /// Event triggered when a node is added to a workspace
        /// </summary>
        public event NodeHandler NodeAdded;

        /// <summary>
        /// Event triggered when a node is deleted
        /// </summary>
        public event NodeHandler NodeDeleted;

        /// <summary>
        /// Event triggered when a connector is added.
        /// </summary>
        public event ConnectorHandler ConnectorAdded;

        /// <summary>
        /// Event triggered when a connector is deleted.
        /// </summary>
        public event ConnectorHandler ConnectorDeleted;

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
                        (a, x) => a.Concat(x.WorkspaceModel.Nodes)
                        )
                    );
            }
        }

        #endregion

        public DynamoModel()
        {
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
            if (!string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(vm.Model.CurrentWorkspace.FileName);
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
            string xmlFilePath = parameters as string;
            var command = new DynCmd.OpenFileCommand(xmlFilePath);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanOpen(object parameters)
        {
            if (File.Exists(parameters.ToString()))
                return true;
            return false;
        }

        internal void OpenInternal(string xmlPath)
        {
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

        internal void PostUIActivation(object parameter)
        {

            DynamoLoader.LoadCustomNodes();

            dynSettings.Controller.SearchViewModel.RemoveEmptyCategories();
            dynSettings.Controller.SearchViewModel.SortCategoryChildren();

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
            var info = manager.AddFileToPath(workspaceHeader.FileName);
            var funcDef = manager.GetFunctionDefinition(info.Guid);
            if (funcDef == null) // Fail to load custom function.
                return;

            funcDef.AddToSearch();

            var ws = funcDef.WorkspaceModel;
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

                // add custom nodes in dyn directory to path
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
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddHomeWorkspace()
        {
            var workspace = new HomeWorkspaceModel()
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
        ///     Create a build-in node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="signature"> The signature of the function along with parameter information </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated dynNode</returns>
        public NodeModel CreateNodeInstance(Type elementType, string nickName, string signature, Guid guid)
        {
            object createdNode = null;

            if (elementType.IsAssignableFrom(typeof(DSVarArgFunction)))
            {
                // If we are looking at a 'DSVarArgFunction', we'd better had 
                // 'signature' readily available, otherwise we have a problem.
                if (string.IsNullOrEmpty(signature))
                {
                    var message = "Unknown function signature";
                    throw new ArgumentException(message, "signature");
                }

                // Invoke the constructor that takes in a 'FunctionDescriptor'.
                var engine = dynSettings.Controller.EngineController;
                var functionDescriptor = engine.GetFunctionDescriptor(signature);

                if (functionDescriptor == null)
                    throw new UnresolvedFunctionException(signature);

                createdNode = Activator.CreateInstance(elementType,
                    new object[] { functionDescriptor });
            }
            else
            {
                createdNode = Activator.CreateInstance(elementType);
            }

            // The attempt to create node instance may fail due to "elementType"
            // being something else other than "NodeModel" derived object type. 
            // This is possible since some legacy nodes have been made to derive
            // from "MigrationNode" object that is not derived from "NodeModel".
            // 
            NodeModel node = createdNode as NodeModel;
            if (node == null)
                return null;

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

            // Clear undo/redo stacks.
            CurrentWorkspace.ClearUndoRecorder();
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();

            // Reset workspace state
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CancelActiveState();

            dynSettings.Controller.ResetEngine();
            CurrentWorkspace.PreloadedTraceData = null;
        }

        /// <summary>
        /// Open a workspace from a path.
        /// </summary>
        /// <param name="xmlPath">The path to the workspace.</param>
        /// <returns></returns>
        public bool OpenWorkspace(string xmlPath)
        {
            DynamoLogger.Instance.Log("Opening home workspace " + xmlPath + "...");

            OnWorkspaceOpening(this, EventArgs.Empty);

            CleanWorkbench();
            MigrationManager.ResetIdentifierIndex();

            //clear the renderables
            //dynSettings.Controller.VisualizationManager.ClearRenderables();

            var sw = new Stopwatch();

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

                Version fileVersion = MigrationManager.VersionFromString(version);

                var dynamoModel = dynSettings.Controller.DynamoModel;
                var currentVersion = MigrationManager.VersionFromWorkspace(dynamoModel.HomeSpace);
                var decision = MigrationManager.ShouldMigrateFile(fileVersion, currentVersion);
                if (decision == MigrationManager.Decision.Abort)
                {
                    Utils.DisplayObsoleteFileMessage(xmlPath, fileVersion, currentVersion);
                    return false;
                }
                else if (decision == MigrationManager.Decision.Migrate)
                {
                    string backupPath = string.Empty;
                    bool isTesting = DynamoController.IsTestMode; // No backup during test.
                    if (!isTesting && MigrationManager.BackupOriginalFile(xmlPath, ref backupPath))
                    {
                        string message = string.Format(
                            "Original file '{0}' gets backed up at '{1}'",
                            Path.GetFileName(xmlPath), backupPath);

                        DynamoLogger.Instance.Log(message);
                    }

                    MigrationManager.Instance.ProcessWorkspaceMigrations(xmlDoc, fileVersion);
                    MigrationManager.Instance.ProcessNodesInWorkspace(xmlDoc, fileVersion);
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
                        typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                        System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);
                        if (type != null)
                            el = CreateNodeInstance(type, nickname, signature, guid);

                        if (el != null)
                        {
                            el.WorkSpace = CurrentWorkspace;
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

                    // If a custom node fails to load its definition, convert it into a dummy node.
                    var function = el as Dynamo.Nodes.Function;
                    if ((function != null) && (function.Definition == null))
                    {
                        var e = elNode as XmlElement;
                        dummyElement = MigrationManager.CreateMissingNode(
                            e, el.InPortData.Count, el.OutPortData.Count);
                    }

                    if (dummyElement != null) // If a dummy node placement is desired.
                    {
                        // The new type representing the dummy node.
                        typeName = dummyElement.GetAttribute("type");
                        System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);

                        el = CreateNodeInstance(type, nickname, string.Empty, guid);
                        el.WorkSpace = CurrentWorkspace;
                        el.Load(dummyElement);
                    }

                    CurrentWorkspace.Nodes.Add(el);

                    OnNodeAdded(el);

                    el.X = x;
                    el.Y = y;

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

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;

                    if (CurrentWorkspace == HomeSpace)
                        el.SaveResult = true;
                }

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading nodes.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                //OnRequestLayoutUpdate(this, EventArgs.Empty);

                //DynamoLogger.Instance.Log(string.Format("{0} ellapsed for updating layout.", sw.Elapsed - previousElapsed));
                //previousElapsed = sw.Elapsed;

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
                    PortType portType = ((PortType) Convert.ToInt16(portTypeAttrib.Value));

                    //find the elements to connect
                    NodeModel start = null;
                    NodeModel end = null;

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

                    if (newConnector != null)
                        CurrentWorkspace.Connectors.Add(newConnector);

                    OnConnectorAdded(newConnector);
                }

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading connectors.",
                    sw.Elapsed - previousElapsed));
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

                        // TODO(Ben): Shouldn't we be reading in the Guid 
                        // from file instead of generating a new one here?
                        Guid id = Guid.NewGuid();
                        var command = new DynCmd.CreateNoteCommand(id, text, x, y, false);
                        AddNoteInternal(command, CurrentWorkspace);
                    }
                }

                #endregion

                DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading notes.", sw.Elapsed - previousElapsed));

                foreach (NodeModel e in CurrentWorkspace.Nodes)
                    e.EnableReporting();

                // http://www.japf.fr/2009/10/measure-rendering-time-in-a-wpf-application/comment-page-1/#comment-2892
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        sw.Stop();
                        DynamoLogger.Instance.Log(string.Format("{0} ellapsed for loading workspace.", sw.Elapsed));
                    }));

                #endregion

                HomeSpace.FileName = xmlPath;

                // Allow live runner a chance to preload trace data from XML.
                var engine = dynSettings.Controller.EngineController;
                if (engine != null && (engine.LiveRunnerCore != null))
                {
                    var data = Utils.LoadTraceDataFromXmlDocument(xmlDoc);
                    CurrentWorkspace.PreloadedTraceData = data;
                }
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

            OnWorkspaceOpened(this, EventArgs.Empty);

            return true;
        }

        public CustomNodeDefinition NewCustomNodeWorkspace(   Guid id,
                                                            string name,
                                                            string category,
                                                            string description,
                                                            bool makeCurrentWorkspace,
                                                            double workspaceOffsetX = 0,
                                                            double workspaceOffsetY = 0)
        {

            var workSpace = new CustomNodeWorkspaceModel(
                name, category, description, workspaceOffsetX, workspaceOffsetY)
            {
                WatchChanges = true
            };

            Workspaces.Add(workSpace);

            workSpace.Nodes.ToList();
            workSpace.Connectors.ToList();

            var functionDefinition = new CustomNodeDefinition(id)
            {
                WorkspaceModel = workSpace
            };

            functionDefinition.SyncWithWorkspace(true, true);

            if (makeCurrentWorkspace)
            {
                CurrentWorkspace = workSpace;
            }

            return functionDefinition;
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
        /// After command framework is implemented, this method should now be only 
        /// called from a menu item (i.e. Ctrl + W). It should not be used as a way
        /// for any other code paths to create a note programmatically. For that we
        /// now have AddNoteInternal which takes in more configurable arguments.
        /// </summary>
        /// <param name="parameters">This is not used and should always be null,
        /// otherwise an ArgumentException will be thrown.</param>
        /// 
        public void AddNote(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            var command = new DynCmd.CreateNoteCommand(Guid.NewGuid(), null, 0, 0, true);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanAddNote(object parameters)
        {
            return true;
        }

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
            DynamoSelection.Instance.ClearSelection();

            var nodes = dynSettings.Controller.ClipBoard.OfType<NodeModel>();

            var connectors = dynSettings.Controller.ClipBoard.OfType<ConnectorModel>();

            foreach (NodeModel node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();
                nodeLookup.Add(node.GUID, newGuid);

                string nodeName = node.GetType().ToString();
                if (node is Function)
                    nodeName = ((node as Function).Definition.FunctionId).ToString();
#if USE_DSENGINE
                else if (node is DSFunction)
                    nodeName = ((node as DSFunction).Definition.MangledName);
                else if (node is DSVarArgFunction)
                    nodeName = ((node as DSVarArgFunction).Definition.MangledName);
#endif

                var xmlDoc = new XmlDocument();
                var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
                xmlDoc.AppendChild(dynEl);
                node.Save(xmlDoc, dynEl, SaveContext.Copy);

                createdModels.Add(CreateNode(newGuid, node.X, node.Y + 100, nodeName, dynEl));
            }

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

                DynCmd.CreateNoteCommand command = new DynCmd.CreateNoteCommand(
                    newGUID, note.Text, newX, newY, false);

                createdModels.Add(AddNoteInternal(command, null));

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

            if (args.Success)
            {
                NewCustomNodeWorkspace(Guid.NewGuid(), args.Name, args.Category, args.Description, true);
            }
        }

        internal bool CanShowNewFunctionDialogCommand(object parameter)
        {
            return true;
        }

        // Wrapper for use in unit test cases (to be removed?)
        public NodeModel CreateNode(double x, double y, string nodeName)
        {
            System.Guid id = Guid.NewGuid();
            return CreateNodeInternal(id, nodeName, x, y, false, false, null);
        }

        public NodeModel CreateNode(Guid id, double x, double y,
            string nodeName, XmlNode xmlNode)
        {
            return CreateNodeInternal(id, nodeName, x, y, false, false, xmlNode);
        }

        public NodeModel CreateNode(Guid nodeId, string nodeName,
            double x, double y, bool defaultPosition, bool transformCoordinates)
        {
            return CreateNodeInternal(nodeId, nodeName,
                x, y, defaultPosition, transformCoordinates, null);
        }

        /// <summary>
        /// Create a node with the given parameters. Since this method is called
        /// on an instance of DynamoModel, it also raises node added event to any
        /// event handlers, typically useful for real user scenario (listeners 
        /// may include package manager and other UI components).
        /// </summary>
        /// <param name="nodeId">The Guid to be used for the new node, it cannot
        /// be Guid.Empty since this method does not attempt to internally generate 
        /// a new Guid. An ArgumentException will be thrown if this argument is 
        /// Guid.Empty.</param>
        /// <param name="nodeName">The name of the node type to be created.</param>
        /// <param name="x">The x coordinates where the newly created node should 
        /// be placed. This value is ignored if useDefaultPos is true.</param>
        /// <param name="y">The y coordinates where the newly created node should 
        /// be placed. This value is ignored if useDefaultPos is true.</param>
        /// <param name="useDefaultPos">This parameter indicates if the node 
        /// should be created at the default position. If this parameter is true,
        /// the node is created at the center of view, and both x and y parameters
        /// are ignored. If this is false, the values for both x and y parameters 
        /// will be used as the initial position of the new node.</param>
        /// <param name="transformCoordinates">If this parameter is true, then the
        /// position of new node will be transformed from outerCanvas space into 
        /// zoomCanvas space.</param>
        /// <param name="xmlNode">This argument carries information that a node 
        /// may require for its creation. The new node loads itself from this 
        /// parameter if one is specified. This parameter is optional.</param>
        /// <returns>Returns the created NodeModel, or null if the operation has 
        /// failed.</returns>
        /// 
        private NodeModel CreateNodeInternal(
            Guid nodeId, string nodeName, double x, double y,
            bool useDefaultPos, bool transformCoordinates, XmlNode xmlNode)
        {
            if (nodeId == Guid.Empty)
                throw new ArgumentException("Node ID must be specified", "nodeId");

            NodeModel node = CreateNodeInstance(nodeName);
            if (node == null)
            {
                string format = "Failed to create node '{0}' (GUID: {1})";
                WriteToLog(string.Format(format, nodeName, nodeId));
                return null;
            }

            if (useDefaultPos == false) // Position was specified.
            {
                node.X = x;
                node.Y = y;
            }

            if ((node is Symbol || node is Output) && CurrentWorkspace is HomeWorkspaceModel)
            {
                string format = "Cannot place '{0}' in HomeWorkspace (GUID: {1})";
                WriteToLog(string.Format(format, nodeName, nodeId));
                return null;
            }

            CurrentWorkspace.Nodes.Add(node);
            node.WorkSpace = CurrentWorkspace;

            if (null != xmlNode)
                node.Load(xmlNode);

            // Override the guid so we can store for connection lookup
            node.GUID = nodeId;

            DynamoViewModel viewModel = dynSettings.Controller.DynamoViewModel;
            WorkspaceViewModel workspaceViewModel = viewModel.CurrentSpaceViewModel;

            ModelEventArgs args = null;
            if (!useDefaultPos)
                args = new ModelEventArgs(node, x, y, transformCoordinates);
            else
            {
                // The position of the new node has not been specified.
                args = new ModelEventArgs(node, transformCoordinates);
            }

            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;
            vm.CurrentSpaceViewModel.OnRequestNodeCentered(this, args);

            node.EnableInteraction();

            if (CurrentWorkspace == HomeSpace)
                node.SaveResult = true;

            OnNodeAdded(node);
            return node;
        }

        internal static NodeModel CreateNodeInstance(string name)
        {
            NodeModel result;
            
#if USE_DSENGINE
            FunctionDescriptor functionItem = (dynSettings.Controller.EngineController.GetFunctionDescriptor(name));
            if (functionItem != null)
            {
                if (functionItem.IsVarArg) 
                    return new DSVarArgFunction(functionItem);
                return new DSFunction(functionItem);
            }
#endif
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
                    DynamoLogger.Instance.Log("Failed to find CustomNodeDefinition.");
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
        ///     If successful, the CurrentWorkspace.FileName field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        internal void SaveAs(string path)
        {
            CurrentWorkspace.SaveAs(path);
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
        ///     Attempts to save an the current workspace. Assumes that workspace has already been saved.
        /// </summary>
        public void Save(object parameter)
        {
            if (!String.IsNullOrEmpty(CurrentWorkspace.FileName))
                SaveAs(CurrentWorkspace.FileName);
        }

        internal bool CanSave(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Called when a node is added to a workspace
        /// </summary>
        /// <param name="node"></param>
        public void OnNodeAdded(NodeModel node)
        {
            if (NodeAdded != null && node != null)
            {
                NodeAdded(node);
            }
        }

        /// <summary>
        /// Called when a node is deleted
        /// </summary>
        /// <param name="node"></param>
        public void OnNodeDeleted(NodeModel node)
        {
            WorkspaceViewModel wvm = dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel;

            if (wvm.CurrentState == WorkspaceViewModel.StateMachine.State.Connection)
            {
                if (node == wvm.ActiveConnector.ActiveStartPort.Owner)
                    wvm.CancelActiveState();
            }
            
            if (NodeDeleted != null)
            {
                NodeDeleted(node);
            }
        }

        /// <summary>
        /// Called when a connector is added.
        /// </summary>
        /// <param name="connector"></param>
        private void OnConnectorAdded(ConnectorModel connector)
        {
            if (ConnectorAdded != null)
            {
                ConnectorAdded(connector);
            }
        }

        /// <summary>
        /// Called when a connector is deleted.
        /// </summary>
        /// <param name="connector"></param>
        internal void OnConnectorDeleted(ConnectorModel connector)
        {
            if (ConnectorDeleted != null)
            {
                ConnectorDeleted(connector);
            }
        }

        /// <summary>
        /// Clear the workspace. Removes all nodes, notes, and connectors from the current workspace.
        /// </summary>
        /// <param name="parameter"></param>
        public void Clear(object parameter)
        {
            OnWorkspaceClearing(this, EventArgs.Empty);

            dynSettings.Controller.IsUILocked = true;

            CleanWorkbench();

            //don't save the file path
            CurrentWorkspace.FileName = "";
            CurrentWorkspace.HasUnsavedChanges = false;
            CurrentWorkspace.WorkspaceVersion = AssemblyHelper.GetDynamoVersion();

            //OnModelCleared();

            dynSettings.Controller.IsUILocked = false;

            OnWorkspaceCleared(this, EventArgs.Empty);
        }

        internal bool CanClear(object parameter)
        {
            return true;
        }

        /// <summary>
        /// After command framework is implemented, this method should now be only 
        /// called from a menu item (i.e. Delete key). It should not be used as a 
        /// way for any other code paths to create a note programmatically. For 
        /// that we now have DeleteModelInternal which takes in more configurable 
        /// arguments.
        /// </summary>
        /// <param name="parameters">This is not used and should always be null,
        /// otherwise an ArgumentException will be thrown.</param>
        /// 
        internal void Delete(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            var command = new DynCmd.DeleteModelCommand(Guid.Empty);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanDelete(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        internal void DeleteModelInternal(List<ModelBase> modelsToDelete)
        {
            if (null == this._cspace)
                return;

            OnDeletionStarted(this, EventArgs.Empty);

            this._cspace.RecordAndDeleteModels(modelsToDelete);

            var selection = DynamoSelection.Instance.Selection;
            foreach (ModelBase model in modelsToDelete)
            {
                selection.Remove(model); // Remove from selection set.
                if (model is NodeModel)
                    OnNodeDeleted(model as NodeModel);
                if (model is ConnectorModel)
                    OnConnectorDeleted(model as ConnectorModel);
            }

            OnDeletionComplete(this, EventArgs.Empty);
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

                var c = ConnectorModel.Make(start, end, startIndex, endIndex, PortType.INPUT);

                if (c != null)
                    CurrentWorkspace.Connectors.Add(c);

                OnConnectorAdded(c);

                return c;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e.Message);
                DynamoLogger.Instance.Log(e);
            }

            return null;
        }

        internal NoteModel AddNoteInternal(DynCmd.CreateNoteCommand command, WorkspaceModel workspace)
        {
            double x = 0.0;
            double y = 0.0;
            if (false == command.DefaultPosition)
            {
                x = command.X;
                y = command.Y;
            }

            NoteModel noteModel = new NoteModel(x, y);
            noteModel.GUID = command.NodeId;

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            if (command.DefaultPosition)
            {
                ModelEventArgs args = new ModelEventArgs(noteModel, true);
                DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;
                vm.CurrentSpaceViewModel.OnRequestNodeCentered(this, args);
            }

            noteModel.Text = "New Note";
            if (!string.IsNullOrEmpty(command.NoteText))
                noteModel.Text = command.NoteText;

            if (null == workspace)
                workspace = CurrentWorkspace;

            workspace.Notes.Add(noteModel);
            return noteModel;
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
        public ModelBase Model { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public bool PositionSpecified { get; private set; }
        public bool TransformCoordinates { get; private set; }

        public ModelEventArgs(ModelBase model)
            : this(model, false)
        {
        }

        public ModelEventArgs(ModelBase model, bool transformCoordinates)
        {
            Model = model;
            PositionSpecified = false;
            TransformCoordinates = transformCoordinates;
        }

        public ModelEventArgs(ModelBase model, double x, double y, bool transformCoordinates)
        {
            Model = model;
            X = x;
            Y = y;
            PositionSpecified = true;
            TransformCoordinates = transformCoordinates;
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
