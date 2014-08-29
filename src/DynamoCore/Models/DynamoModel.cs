using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

using DSNodeServices;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Services;
using Dynamo.UI;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.Selection;

using DynamoUnits;

using DynamoUtilities;

using Microsoft.Practices.Prism;

using Enum = System.Enum;
using String = System.String;
using Utils = Dynamo.Nodes.Utilities;

using Dynamo.ViewModels;
using Dynamo.DSEngine;

using Double = System.Double;

namespace Dynamo.Models
{
    public partial class DynamoModel : ModelBase
    {
        #region Events

        public event FunctionNamePromptRequestHandler RequestsFunctionNamePrompt;
        public void OnRequestsFunctionNamePrompt(Object sender, FunctionNamePromptEventArgs e)
        {
            if (RequestsFunctionNamePrompt != null)
            {
                RequestsFunctionNamePrompt(this, e);
            }
        }

        public event WorkspaceHandler WorkspaceSaved;
        internal void OnWorkspaceSaved(WorkspaceModel model)
        {
            if (WorkspaceSaved != null)
            {
                WorkspaceSaved(model);
            }
        }

        #endregion

        #region internal members

        private ObservableCollection<WorkspaceModel> workspaces = new ObservableCollection<WorkspaceModel>();
        private Dictionary<Guid, NodeModel> nodeMap = new Dictionary<Guid, NodeModel>();
        private bool runEnabled = true;
        #endregion

        #region Static properties

        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        public static bool IsTestMode { get; set; }

        /// <summary>
        /// Setting this flag enables creation of an XML in following format that records 
        /// node mapping information - which old node has been converted to which to new node(s) 
        /// </summary>
        public static bool EnableMigrationLogging { get; set; }

        #endregion

        #region public properties

        // core app
        public string Context { get; set; }
        public DynamoLoader Loader { get; private set; }
        public PackageManagerClient PackageManagerClient { get; private set; }
        public CustomNodeManager CustomNodeManager { get; private set; }
        public DynamoLogger Logger { get; private set; }
        public DynamoRunner Runner { get; protected set; }
        public SearchModel SearchModel { get; private set; }
        public DebugSettings DebugSettings { get; private set; }
        public EngineController EngineController { get; private set; }
        public PreferenceSettings PreferenceSettings { get; private set; }
        public bool ShutdownRequested { get; internal set; }

        // KILLDYNSETTINGS: wut am I!?!
        public string UnlockLoadPath { get; set; }

        private WorkspaceModel currentWorkspace;
        public WorkspaceModel CurrentWorkspace
        {
            get { return currentWorkspace; }
            internal set
            {
                if (currentWorkspace != value)
                {
                    if (currentWorkspace != null)
                        currentWorkspace.IsCurrentSpace = false;

                    currentWorkspace = value;

                    if (currentWorkspace != null)
                        currentWorkspace.IsCurrentSpace = true;

                    OnCurrentWorkspaceChanged(currentWorkspace);
                    RaisePropertyChanged("CurrentWorkspace");
                }
            }
        }

        public HomeWorkspaceModel HomeSpace { get; protected set; }

        private ObservableCollection<ModelBase> clipBoard = new ObservableCollection<ModelBase>();
        public ObservableCollection<ModelBase> ClipBoard
        {
            get { return clipBoard; }
            set { clipBoard = value; }
        }

        private readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();
        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        private readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();
        public Dictionary<string, TypeLoadData> BuiltInTypesByName
        {
            get { return builtinTypesByTypeName; }
        }

        public bool IsShowingConnectors
        {
            get { return this.PreferenceSettings.ShowConnector; }
            set
            {
                this.PreferenceSettings.ShowConnector = value;
            }
        }

        public ConnectorType ConnectorType
        {
            get { return this.PreferenceSettings.ConnectorType; }
            set
            {
                this.PreferenceSettings.ConnectorType = value;
            }
        }

        public static bool IsCrashing { get; set; }
        public bool DynamicRunEnabled { get; set; }

        /// <summary>
        ///     The collection of visible workspaces in Dynamo
        /// </summary>
        public ObservableCollection<WorkspaceModel> Workspaces
        {
            get { return workspaces; }
            set 
            { 
                workspaces = value;
            }
        }

        /// <summary>
        /// Returns a shallow copy of the collection of Nodes in the model.
        /// </summary>
        public List<NodeModel> Nodes
        {
            get { return CurrentWorkspace.Nodes.ToList(); }
        }

        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                runEnabled = value;
                RaisePropertyChanged("RunEnabled");
            }
        }
        public bool RunInDebug { get; set; }

        /// <summary>
        /// All nodes in all workspaces. 
        /// </summary>
        public IEnumerable<NodeModel> AllNodes
        {
            get
            {
                return Workspaces.Aggregate((IEnumerable<NodeModel>)new List<NodeModel>(), (a, x) => a.Concat(x.Nodes))
                    .Concat(CustomNodeManager.GetLoadedDefinitions().Aggregate(
                        (IEnumerable<NodeModel>)new List<NodeModel>(),
                        (a, x) => a.Concat(x.WorkspaceModel.Nodes)
                        )
                    );
            }
        }

        public ObservableDictionary<string, Guid> CustomNodes
        {
            get { return this.CustomNodeManager.GetAllNodeNames(); }
        }

        /// <summary>
        /// A map of all nodes in the model in the home workspace
        /// keyed by their GUID.
        /// </summary>
        public Dictionary<Guid, NodeModel> NodeMap
        {
            get { return nodeMap; }
            set { nodeMap = value; }
        }

        #endregion

        public struct StartConfiguration
        {
            public string Context { get; set; }
            public string DynamoCorePath { get; set; }
            public IPreferences Preferences { get; set; }
            public bool StartInTestMode { get; set; }
            public DynamoRunner Runner { get; set; }
        }

        /// <summary>
        /// Start DynamoModel with all default configuration options
        /// </summary>
        /// <returns></returns>
        public static DynamoModel Start()
        {
            return Start(new StartConfiguration());
        }

        /// <summary>
        /// Start DynamoModel with custom configuration.  Defaults will be assigned not provided.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static DynamoModel Start(StartConfiguration configuration)
        {
            // where necessary, assign defaults
            if (string.IsNullOrEmpty(configuration.Context))
                configuration.Context = Core.Context.NONE;
            if (string.IsNullOrEmpty(configuration.DynamoCorePath))
            {
                var asmLocation = Assembly.GetExecutingAssembly().Location;
                configuration.DynamoCorePath = Path.GetDirectoryName(asmLocation);
            }

            if (configuration.Preferences == null)
                configuration.Preferences = new PreferenceSettings();
            if (configuration.Runner == null)
                configuration.Runner = new DynamoRunner();

            return new DynamoModel(configuration);
        }

        protected DynamoModel(StartConfiguration configuration)
        {
            string context = configuration.Context;
            IPreferences preferences = configuration.Preferences;
            string corePath = configuration.DynamoCorePath;
            DynamoRunner runner = configuration.Runner;
            bool isTestMode = configuration.StartInTestMode;

            DynamoPathManager.Instance.InitializeCore(corePath);
            UsageReportingManager.Instance.InitializeCore(this);

            Runner = runner;
            Context = context;
            IsTestMode = isTestMode;
            Logger = new DynamoLogger(this, DynamoPathManager.Instance.Logs);
            DebugSettings = new DebugSettings();

            if (preferences is PreferenceSettings)
            {
                this.PreferenceSettings = preferences as PreferenceSettings;
                PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            }

            InitializePreferences(preferences);
            InitializeInstrumentationLogger();

            UpdateManager.UpdateManager.Instance.CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation)));

            SearchModel = new SearchModel(this);

            InitializeCurrentWorkspace();

            this.CustomNodeManager = new CustomNodeManager(this, DynamoPathManager.Instance.UserDefinitions);
            this.Loader = new DynamoLoader(this);

            this.Loader.PackageLoader.DoCachedPackageUninstalls();
            this.Loader.PackageLoader.LoadPackages();

            DisposeLogic.IsShuttingDown = false;

            this.EngineController = new EngineController(this, DynamoPathManager.Instance.GeometryFactory);
            this.CustomNodeManager.RecompileAllNodes(EngineController);

            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            this.Reset();

            Logger.Log(String.Format(
                "Dynamo -- Build {0}",
                Assembly.GetExecutingAssembly().GetName().Version));

            this.Loader.ClearCachedAssemblies();
            this.Loader.LoadNodeModels();

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));

            PackageManagerClient = new PackageManagerClient(this);
        }

        private void InitializeInstrumentationLogger()
        {
            InstrumentationLogger.Start(this);
        }

        private void InitializeCurrentWorkspace()
        {
            this.AddHomeWorkspace();
            this.CurrentWorkspace = this.HomeSpace;
            this.CurrentWorkspace.X = 0;
            this.CurrentWorkspace.Y = 0;
        }

        private static void InitializePreferences(IPreferences preferences)
        {
            BaseUnit.LengthUnit = preferences.LengthUnit;
            BaseUnit.AreaUnit = preferences.AreaUnit;
            BaseUnit.VolumeUnit = preferences.VolumeUnit;
            BaseUnit.NumberFormat = preferences.NumberFormat;
        }

        #region internal methods

        public string Version
        {
            get { return UpdateManager.UpdateManager.Instance.ProductVersion.ToString(); }
        }

        public virtual void ShutDown(bool shutDownHost, EventArgs args = null)
        {
            ShutdownRequested = true;

            CleanWorkbench();

            EngineController.Dispose();
            EngineController = null;

            PreferenceSettings.Save();

            OnCleanup(args);

            Logger.Dispose();
        }
        
        /// <summary>
        /// Force reset of the execution substrait. Executing this will have a negative performance impact
        /// </summary>
        public void Reset()
        {
            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            //TODO(Luke): Push this into a resync call with the engine controller
            ResetEngine();

            foreach (var node in Nodes)
            {
                node.RequiresRecalc = true;
            }
        }

        public virtual void ResetEngine()
        {
            if (EngineController != null)
            {
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(this, DynamoPathManager.Instance.GeometryFactory);
            CustomNodeManager.RecompileAllNodes(EngineController);
        }

        public void RunExpression()
        {
            Runner.RunExpression(this.HomeSpace);
        }

        internal void RunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync(this.EngineController);
            else
                RunExpression();
        }

        internal void ForceRunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync(this.EngineController);
            else
            {
                Logger.Log("Beginning engine reset");
                Reset();
                Logger.Log("Reset complete");

                RunExpression();
            }
        }

        /// <summary>
        /// Responds to property update notifications on the preferences,
        /// and synchronizes with the Units Manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreferenceSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LengthUnit":
                    BaseUnit.LengthUnit = PreferenceSettings.LengthUnit;
                    break;
                case "AreaUnit":
                    BaseUnit.AreaUnit = PreferenceSettings.AreaUnit;
                    break;
                case "VolumeUnit":
                    BaseUnit.VolumeUnit = PreferenceSettings.VolumeUnit;
                    break;
                case "NumberFormat":
                    BaseUnit.NumberFormat = PreferenceSettings.NumberFormat;
                    break;
            }
        }

        private void RemoveNodeFromMap(NodeModel n)
        {
            if (n.Workspace != HomeSpace)
            {
                return;
            }

            if (NodeMap.ContainsKey(n.GUID))
            {
                NodeMap.Remove(n.GUID);
            }
        }

        private void AddNodeToMap(NodeModel n)
        {
            if (n.Workspace != HomeSpace)
            {
                return;
            }

            if (!NodeMap.ContainsKey(n.GUID))
            {
                NodeMap.Add(n.GUID, n);
            }
            else
            {
                throw new Exception("Duplicate node GUID in map!");
            }
        }

        internal void OpenInternal(string xmlPath)
        {
            if (!OpenDefinition(xmlPath))
            {
                Logger.Log("Workbench could not be opened.");

                if (Logger != null)
                {
                    WriteToLog("Workbench could not be opened.");
                    WriteToLog(xmlPath);
                }
            }
        }

        internal void PostUIActivation(object parameter)
        {
            Loader.LoadCustomNodes();

            this.SearchModel.RemoveEmptyCategories();
            this.SearchModel.SortCategoryChildren();

            Logger.Log("Welcome to Dynamo!");
        }

        internal bool CanDoPostUIActivation(object parameter)
        {
            return true;
        }

        internal void OpenCustomNodeAndFocus(WorkspaceHeader workspaceHeader)
        {
            // load custom node
            var manager = CustomNodeManager;
            var info = manager.AddFileToPath(workspaceHeader.FileName);
            var funcDef = manager.GetFunctionDefinition(info.Guid);
            if (funcDef == null) // Fail to load custom function.
                return;

            if (funcDef.IsProxy && info != null)
            {
                funcDef = manager.ReloadFunctionDefintion(info.Guid);
                if (funcDef == null)
                {
                    return;
                }
            }

            funcDef.AddToSearch(this.SearchModel);

            var ws = funcDef.WorkspaceModel;
            ws.Zoom = workspaceHeader.Zoom;
            ws.HasUnsavedChanges = false;

            if (!this.Workspaces.Contains(ws))
            {
                this.Workspaces.Add(ws);
            }

            var vm = this.Workspaces.First(x => x == ws);
            vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(workspaceHeader.X, workspaceHeader.Y)));

            this.CurrentWorkspace = ws;
        }

        internal bool OpenDefinition(string xmlPath)
        {
            var workspaceInfo = WorkspaceHeader.FromPath(this, xmlPath);

            if (workspaceInfo == null)
            {
                return false;
            }

            if (workspaceInfo.IsCustomNodeWorkspace())
            {
                OpenCustomNodeAndFocus(workspaceInfo);
                return true;
            }

            if (CurrentWorkspace != HomeSpace)
                ViewHomeWorkspace();

            // add custom nodes in dyn directory to path
            var dirName = Path.GetDirectoryName(xmlPath);
            CustomNodeManager.AddDirectoryToSearchPath(dirName);
            CustomNodeManager.UpdateSearchPath();

            return OpenWorkspace(xmlPath);
        }

        internal void CleanWorkbench()
        {
            Logger.Log("Clearing workflow...");

            //Copy locally
            List<NodeModel> elements = Nodes.ToList();

            foreach (NodeModel el in elements)
            {
                el.DisableReporting();
                el.Destroy();
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

                RemoveNodeFromMap(el);
            }

            CurrentWorkspace.Connectors.Clear();
            CurrentWorkspace.Nodes.Clear();
            CurrentWorkspace.Notes.Clear();

            CurrentWorkspace.ClearUndoRecorder();

            this.ResetEngine();
            CurrentWorkspace.PreloadedTraceData = null;
        }

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewHomeWorkspace()
        {
            CurrentWorkspace = HomeSpace;
        }

        internal void DeleteModelInternal(List<ModelBase> modelsToDelete)
        {
            if (null == this.currentWorkspace)
                return;

            OnDeletionStarted(this, EventArgs.Empty);

            this.currentWorkspace.RecordAndDeleteModels(modelsToDelete);

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

        internal bool CanGoHome(object parameter)
        {
            return CurrentWorkspace != HomeSpace;
        }

        #endregion

        #region public methods

        public void HideWorkspace(WorkspaceModel workspace)
        {
            this.CurrentWorkspace = workspaces[0];  // go home
            workspaces.Remove(workspace);
            OnWorkspaceHidden(workspace);
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        private void AddHomeWorkspace()
        {
            var workspace = new HomeWorkspaceModel(this)
            {
                WatchChanges = true
            };
            HomeSpace = workspace;
            workspaces.Insert(0, workspace); // to front
        }

        /// <summary>
        /// Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            workspaces.Remove(workspace);
        }

        /// <summary>
        /// Open a workspace from a path.
        /// </summary>
        /// <param name="xmlPath">The path to the workspace.</param>
        /// <returns></returns>
        public bool OpenWorkspace(string xmlPath)
        {
            Logger.Log("Opening home workspace " + xmlPath + "...");

            CleanWorkbench();
            MigrationManager.ResetIdentifierIndex();

            var sw = new Stopwatch();

            try
            {
                #region read xml file

                sw.Start();

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                TimeSpan previousElapsed = sw.Elapsed;
                Logger.Log(String.Format("{0} elapsed for loading xml.", sw.Elapsed));

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
                            cx = Double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            cy = Double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("zoom"))
                        {
                            zoom = Double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Version"))
                        {
                            version = att.Value;
                        }
                    }
                }

                Version fileVersion = MigrationManager.VersionFromString(version);
                var currentVersion = MigrationManager.VersionFromWorkspace(this.HomeSpace);

                if (fileVersion > currentVersion)
                {
                    bool resume = Utils.DisplayFutureFileMessage(this, xmlPath, fileVersion, currentVersion);
                    if (!resume)
                        return false;                    
                }

                var decision = MigrationManager.ShouldMigrateFile(fileVersion, currentVersion);
                if (decision == MigrationManager.Decision.Abort)
                {
                    Utils.DisplayObsoleteFileMessage(this, xmlPath, fileVersion, currentVersion);
                    return false;
                }
                else if (decision == MigrationManager.Decision.Migrate)
                {
                    string backupPath = String.Empty;
                    if (!IsTestMode && MigrationManager.BackupOriginalFile(xmlPath, ref backupPath))
                    {
                        string message = String.Format(
                            "Original file '{0}' gets backed up at '{1}'",
                            Path.GetFileName(xmlPath), backupPath);

                        Logger.Log(message);
                    }

                    //Hardcode the file version to 0.6.0.0. The file whose version is 0.7.0.x
                    //needs to be forced to be migrated. The version number needs to be changed from
                    //0.7.0.x to 0.6.0.0.
                    if (fileVersion == new Version(0, 7, 0, 0))
                        fileVersion = new Version(0, 6, 0, 0);

                    MigrationManager.Instance.ProcessWorkspaceMigrations(this, xmlDoc, fileVersion);
                    MigrationManager.Instance.ProcessNodesInWorkspace(this, xmlDoc, fileVersion);
                }

                //set the zoom and offsets and trigger events
                //to get the view to position iteself
                CurrentWorkspace.X = cx;
                CurrentWorkspace.Y = cy;
                CurrentWorkspace.Zoom = zoom;

                var vm = this.Workspaces.First(x => x == CurrentWorkspace);
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

                    double x = Double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                    double y = Double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

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
                        typeName = Utils.PreprocessTypeName(typeName);
                        Type type = Utils.ResolveType(this, typeName);
                        if (type != null)
                            el = CurrentWorkspace.NodeFactory.CreateNodeInstance(type, nickname, signature, guid);

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

                    // If a custom node fails to load its definition, convert it into a dummy node.
                    var function = el as Function;
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
                        var type = Utils.ResolveType(this, typeName);

                        el = CurrentWorkspace.NodeFactory.CreateNodeInstance(type, nickname, String.Empty, guid);
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

                    // This is to fix MAGN-3648. Method reference in CBN that gets 
                    // loaded before method definition causes a CBN to be left in 
                    // a warning state. This is to clear such warnings and set the 
                    // node to "Dead" state (correct value of which will be set 
                    // later on with a call to "EnableReporting" below). Please 
                    // refer to the defect for details and other possible fixes.
                    // 
                    if (el.State == ElementState.Warning && (el is CodeBlockNodeModel))
                        el.State = ElementState.Dead; // Condition to fix MAGN-3648

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;

                    if (CurrentWorkspace == HomeSpace)
                        el.SaveResult = true;
                }

                Logger.Log(String.Format("{0} ellapsed for loading nodes.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                //OnRequestLayoutUpdate(this, EventArgs.Empty);

                //Logger.Log(string.Format("{0} ellapsed for updating layout.", sw.Elapsed - previousElapsed));
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

                    var newConnector = currentWorkspace.AddConnection( start, end,
                        startIndex, endIndex, portType);

                    OnConnectorAdded(newConnector);
                }

                Logger.Log(String.Format("{0} ellapsed for loading connectors.",
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
                        double x = Double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                        double y = Double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                        // TODO(Ben): Shouldn't we be reading in the Guid 
                        // from file instead of generating a new one here?
                        CurrentWorkspace.AddNote(false, x, y, text, Guid.NewGuid());
                    }
                }

                #endregion

                Logger.Log(String.Format("{0} ellapsed for loading notes.", sw.Elapsed - previousElapsed));

                foreach (NodeModel e in CurrentWorkspace.Nodes)
                    e.EnableReporting();

                // http://www.japf.fr/2009/10/measure-rendering-time-in-a-wpf-application/comment-page-1/#comment-2892
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        sw.Stop();
                        Logger.Log(String.Format("{0} ellapsed for loading workspace.", sw.Elapsed));
                    }));

                #endregion

                HomeSpace.FileName = xmlPath;

                // Allow live runner a chance to preload trace data from XML.
                var engine = this.EngineController;
                if (engine != null && (engine.LiveRunnerCore != null))
                {
                    var data = Utils.LoadTraceDataFromXmlDocument(xmlDoc);
                    CurrentWorkspace.PreloadedTraceData = data;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("There was an error opening the workbench.");
                Logger.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                OnWorkspaceCleared(this, EventArgs.Empty);
                return false;
            }

            CurrentWorkspace.HasUnsavedChanges = false;
            OnWorkspaceCleared(this, EventArgs.Empty);

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

            var workSpace = new CustomNodeWorkspaceModel(this,
                name, category, description, workspaceOffsetX, workspaceOffsetY)
            {
                WatchChanges = true
            };

            Workspaces.Add(workSpace);

            var functionDefinition = new CustomNodeDefinition(id)
            {
                WorkspaceModel = workSpace
            };

            functionDefinition.SyncWithWorkspace(this, true, true);

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
            Logger.Log(logText);
        }

        /// <summary>
        /// Copy selected ISelectable objects to the clipboard.
        /// </summary>
        /// <param name="parameters"></param>
        public void Copy(object parameters)
        {
            this.ClipBoard.Clear();

            foreach (ISelectable sel in DynamoSelection.Instance.Selection)
            {
                //MVVM : selection and clipboard now hold view model objects
                //UIElement el = sel as UIElement;
                ModelBase el = sel as ModelBase;
                if (el != null)
                {
                    if (!this.ClipBoard.Contains(el))
                    {
                        this.ClipBoard.Add(el);

                        //dynNodeView n = el as dynNodeView;
                        NodeModel n = el as NodeModel;
                        if (n != null)
                        {
                            var connectors = n.InPorts.ToList().SelectMany(x => x.Connectors)
                                .Concat(n.OutPorts.ToList().SelectMany(x => x.Connectors))
                                .Where(x => x.End != null &&
                                    x.End.Owner.IsSelected &&
                                    !this.ClipBoard.Contains(x));

                            this.ClipBoard.AddRange(connectors);
                        }
                    }
                }
            }
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

            var nodes = this.ClipBoard.OfType<NodeModel>();

            var connectors = this.ClipBoard.OfType<ConnectorModel>();

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
                    nodeName = ((node as DSFunction).Controller.MangledName);
                else if (node is DSVarArgFunction)
                    nodeName = ((node as DSVarArgFunction).Controller.MangledName);
#endif

                var xmlDoc = new XmlDocument();
                var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
                xmlDoc.AppendChild(dynEl);
                node.Save(xmlDoc, dynEl, SaveContext.Copy);

                var newNode = CurrentWorkspace.AddNode(
                    newGuid,
                    nodeName,
                    node.X,
                    node.Y + 100,
                    false,
                    false,
                    dynEl);
                createdModels.Add(newNode);

                newNode.ArgumentLacing = node.ArgumentLacing;
                if (!string.IsNullOrEmpty(node.NickName))
                {
                    newNode.NickName = node.NickName;
                }
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
                if (startNode.Workspace != CurrentWorkspace)
                {
                    continue;
                }

                createdModels.Add(CurrentWorkspace.AddConnection(startNode, endNode, c.Start.Index, c.End.Index));
            }

            //process the queue again to create the connectors
            //DynamoCommands.ProcessCommandQueue();

            var notes = this.ClipBoard.OfType<NoteModel>();

            foreach (NoteModel note in notes)
            {
                var newGUID = Guid.NewGuid();

                var sameSpace = CurrentWorkspace.Notes.Any(x => x.GUID == note.GUID);
                var newX = sameSpace ? note.X + 20 : note.X;
                var newY = sameSpace ? note.Y + 20 : note.Y;

                createdModels.Add(CurrentWorkspace.AddNote(false, newX, newY, note.Text, newGUID));

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

        /// <summary>
        /// Clear the workspace. Removes all nodes, notes, and connectors from the current workspace.
        /// </summary>
        /// <param name="parameter"></param>
        public void Clear(object parameter)
        {
            OnWorkspaceClearing(this, EventArgs.Empty);

            CleanWorkbench();

            //don't save the file path
            CurrentWorkspace.FileName = "";
            CurrentWorkspace.HasUnsavedChanges = false;
            CurrentWorkspace.WorkspaceVersion = AssemblyHelper.GetDynamoVersion();

            OnWorkspaceCleared(this, EventArgs.Empty);
        }

        /// <summary>
        /// View the home workspace.
        /// </summary>
        /// <param name="parameter"></param>
        public void Home(object parameter)
        {
            ViewHomeWorkspace();
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
}
