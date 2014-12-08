using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DSNodeServices;
using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using DynamoUnits;
using DynamoUtilities;
using ProtoCore;
using Executive = ProtoAssociative.Executive;
using FunctionGroup = Dynamo.DSEngine.FunctionGroup;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Models
{
    public partial class DynamoModel : IDisposable // : ModelBase
    {
        #region private members
        #endregion

        #region events

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

        /// <summary>
        /// This event is raised right before the shutdown of DynamoModel started.
        /// When this event is raised, the shutdown is guaranteed to take place
        /// (i.e. user has had a chance to save the work and decided to proceed 
        /// with shutting down Dynamo). Handlers of this event can still safely 
        /// access the DynamoModel, the WorkspaceModel (along with its contents), 
        /// and the DynamoScheduler.
        /// </summary>
        /// 
        public event DynamoModelHandler ShutdownStarted;

        private void OnShutdownStarted()
        {
            if (ShutdownStarted != null)
                ShutdownStarted(this);
        }

        /// <summary>
        /// This event is raised after DynamoModel has been shut down. At this 
        /// point the DynamoModel is no longer valid and access to it should be 
        /// avoided.
        /// </summary>
        /// 
        public event DynamoModelHandler ShutdownCompleted;

        private void OnShutdownCompleted()
        {
            if (ShutdownCompleted != null)
                ShutdownCompleted(this);
        }

        #endregion
        
        #region static properties

        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        protected static bool IsTestMode { get; set; }

        /// <summary>
        /// Setting this flag enables creation of an XML in following format that records 
        /// node mapping information - which old node has been converted to which to new node(s) 
        /// </summary>
        public static bool EnableMigrationLogging { get; set; }

        #endregion

        #region public properties
        //TODO(Steve): Attempt to make the majority of these readonly fields
        
        public readonly LibraryServices LibraryServices;

        /// <summary>
        /// TODO
        /// </summary>
        public bool ShutdownRequested { get; internal set; }

        /// <summary>
        /// TODO
        /// </summary>
        public string Version
        {
            get { return UpdateManager.UpdateManager.Instance.ProductVersion.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        //TODO(Steve): This is still really crappy. We may be able to make it static, at the very least.
        public string Context { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public DynamoLoader Loader { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public PackageLoader PackageLoader { get; private set; }
        
        /// <summary>
        /// TODO
        /// </summary>
        public PackageManagerClient PackageManagerClient { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public CustomNodeManager CustomNodeManager { get; private set; }
        
        /// <summary>
        /// TODO
        /// </summary>
        public DynamoLogger Logger { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DynamoScheduler Scheduler { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int MaxTesselationDivisions { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public NodeSearchModel SearchModel { get; private set; }

        /// <summary>
        /// The application version string for analytics reporting APIs
        /// </summary>
        internal virtual string AppVersion
        {
            get
            {
                return Process.GetCurrentProcess().ProcessName + "-"
                    + UpdateManager.UpdateManager.Instance.ProductVersion;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public DebugSettings DebugSettings { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        [Obsolete("EngineController now kept on HomeWorkspace", true)]
        public EngineController EngineController { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public PreferenceSettings PreferenceSettings { get; private set; }
        
        /// <summary>
        /// TODO
        /// </summary>
        public NodeFactory NodeFactory { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public WorkspaceModel CurrentWorkspace { get; internal set;
            //get { return currentWorkspace; }
            //internal set
            //{
            //    if (currentWorkspace != value)
            //    {
            //        if (currentWorkspace != null)
            //            currentWorkspace.IsCurrentSpace = false;

            //        currentWorkspace = value;

            //        if (currentWorkspace != null)
            //            currentWorkspace.IsCurrentSpace = true;

            //        OnCurrentWorkspaceChanged(currentWorkspace);
            //        RaisePropertyChanged("CurrentWorkspace");
            //    }
            //}
        }
        //private WorkspaceModel currentWorkspace;

        [Obsolete("This makes no sense with multiple home workspaces.", true)]
        public HomeWorkspaceModel HomeSpace { get; protected set; }

        /// <summary>
        /// TODO
        /// </summary>
        public ObservableCollection<ModelBase> ClipBoard { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsShowingConnectors
        {
            get { return PreferenceSettings.ShowConnector; }
            set
            {
                PreferenceSettings.ShowConnector = value;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ConnectorType ConnectorType
        {
            get { return PreferenceSettings.ConnectorType; }
            set
            {
                PreferenceSettings.ConnectorType = value;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        //TODO(Steve): Determine if this is necessary, we should avoid static fields
        public static bool IsCrashing { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        [Obsolete("Running now handled on HomeWorkspaceModel", true)]
        public bool DynamicRunEnabled { get; set; }

        /// <summary>
        ///     The collection of visible workspaces in Dynamo
        /// </summary>
        public ObservableCollection<WorkspaceModel> Workspaces { get; set; }

        /// <summary>
        /// Returns a shallow copy of the collection of Nodes in the model.
        /// </summary>
        [Obsolete("Access CurrentWorkspace.Nodes directly", true)]
        public List<NodeModel> Nodes
        {
            get { return CurrentWorkspace.Nodes.ToList(); }
        }

        /// <summary>
        /// TODO
        /// </summary>
        [Obsolete("Running now handled on HomeWorkspaceModel", true)]
        public bool RunEnabled { get; set; }
        
        #endregion

        #region initialization and disposal

        /// <summary>
        /// External components call this method to shutdown DynamoModel. This 
        /// method marks 'ShutdownRequested' property to 'true'. This method is 
        /// used rather than a public virtual method to ensure that the value of
        /// ShutdownRequested is set to true.
        /// </summary>
        /// <param name="shutdownHost">Set this parameter to true to shutdown 
        /// the host application.</param>
        /// 
        public void ShutDown(bool shutdownHost)
        {
            if (ShutdownRequested)
            {
                const string message = "'DynamoModel.ShutDown' called twice";
                throw new InvalidOperationException(message);
            }

            ShutdownRequested = true;

            OnShutdownStarted(); // Notify possible event handlers.

            PreShutdownCore(shutdownHost);
            ShutDownCore(shutdownHost);
            PostShutdownCore(shutdownHost);

            OnShutdownCompleted(); // Notify possible event handlers.
        }

        protected virtual void PreShutdownCore(bool shutdownHost)
        {
        }

        protected virtual void ShutDownCore(bool shutdownHost)
        {
            CleanWorkbench();

            EngineController.Dispose();
            EngineController = null;

            PreferenceSettings.Save();

            OnCleanup();

            Logger.Dispose();

            DynamoSelection.DestroyInstance();

            InstrumentationLogger.End();

            if (Scheduler != null)
            {
                Scheduler.Shutdown();
                Scheduler.TaskStateChanged -= OnAsyncTaskStateChanged;
                Scheduler = null;
            }
        }

        protected virtual void PostShutdownCore(bool shutdownHost)
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        public struct StartConfiguration
        {
            public string Context { get; set; }
            public string DynamoCorePath { get; set; }
            public IPreferences Preferences { get; set; }
            public bool StartInTestMode { get; set; }
            public IUpdateManager UpdateManager { get; set; }
            public ISchedulerThread SchedulerThread { get; set; }
        }

        /// <summary>
        ///     Start DynamoModel with all default configuration options
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

            return new DynamoModel(configuration);
        }

        protected DynamoModel(StartConfiguration configuration)
        {
            MaxTesselationDivisions = 128;
            string context = configuration.Context;
            IPreferences preferences = configuration.Preferences;
            string corePath = configuration.DynamoCorePath;
            bool isTestMode = configuration.StartInTestMode;

            DynamoPathManager.Instance.InitializeCore(corePath);

            Context = context;
            IsTestMode = isTestMode;
            DebugSettings = new DebugSettings();
            Logger = new DynamoLogger(DebugSettings, DynamoPathManager.Instance.Logs);

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));

            var thread = configuration.SchedulerThread ?? new DynamoSchedulerThread();
            Scheduler = new DynamoScheduler(thread);
            Scheduler.TaskStateChanged += OnAsyncTaskStateChanged;
            
            //TODO(Steve): This is ugly
            if (preferences is PreferenceSettings)
            {
                PreferenceSettings = preferences as PreferenceSettings;
                PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            }

            InitializePreferences(preferences);
            InitializeInstrumentationLogger();

            //TODO(Steve): Need a way to hide input/output nodes in home workspaces...
            SearchModel = new NodeSearchModel();
            SearchModel.ItemProduced += AddNodeToCurrentWorkspace;

            InitializeCurrentWorkspace();

            NodeFactory = new NodeFactory();
            NodeFactory.MessageLogged += LogMessage;

            CustomNodeManager = new CustomNodeManager(NodeFactory);
            CustomNodeManager.MessageLogged += LogMessage;

            var customNodeSearchRegistry = new HashSet<Guid>();
            CustomNodeManager.InfoUpdated += info =>
            {
                if (customNodeSearchRegistry.Contains(info.FunctionId))
                    return;

                customNodeSearchRegistry.Add(info.FunctionId);
                var searchElement = new CustomNodeSearchElement(CustomNodeManager, info);
                SearchModel.Add(searchElement);
                CustomNodeManager.InfoUpdated += newInfo =>
                {
                    if (info.FunctionId == newInfo.FunctionId)
                        searchElement.SyncWithCustomNodeInfo(newInfo);
                };
                CustomNodeManager.CustomNodeRemoved += id =>
                {
                    if (info.FunctionId == id)
                    {
                        customNodeSearchRegistry.Remove(info.FunctionId);
                        SearchModel.Remove(searchElement);
                    }
                };
            };
            
            Loader = new DynamoLoader();
            Loader.MessageLogged += LogMessage;

            PackageLoader = new PackageLoader();
            PackageLoader.MessageLogged += LogMessage;

            DisposeLogic.IsShuttingDown = false;
            
            // Create a core which is used for parsing code and loading libraries
            var libraryCore = new ProtoCore.Core(new Options
            {
                RootCustomPropertyFilterPathName = string.Empty
            });

            libraryCore.Executives.Add(Language.kAssociative, new Executive(libraryCore));
            libraryCore.Executives.Add(Language.kImperative, new ProtoImperative.Executive(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            LibraryServices = new LibraryServices(libraryCore);
            LibraryServices.MessageLogged += LogMessage;
            
            //TODO(Steve): Ensure this is safe when EngineController is re-created
            NodeFactory.AddLoader(new ZeroTouchNodeLoader(LibraryServices));
            NodeFactory.AddLoader(new CustomNodeLoader(CustomNodeManager, IsTestMode));

            UpdateManager.UpdateManager.CheckForProductUpdate();

            Logger.Log(
                string.Format("Dynamo -- Build {0}", Assembly.GetExecutingAssembly().GetName().Version));

            PackageManagerClient = new PackageManagerClient(PackageLoader.RootPackagesDirectory, CustomNodeManager);

            InitializeNodeLibrary(preferences);
        }

        /// <summary>
        /// This event handler is invoked when DynamoScheduler changes the state 
        /// of an AsyncTask object. See TaskStateChangedEventArgs.State for more 
        /// details of these state changes.
        /// </summary>
        /// <param name="sender">The scheduler which raised the event.</param>
        /// <param name="e">Task state changed event argument.</param>
        /// 
        private void OnAsyncTaskStateChanged(DynamoScheduler sender, TaskStateChangedEventArgs e)
        {
            switch (e.CurrentState)
            {
                case TaskStateChangedEventArgs.State.ExecutionStarting:
                    if (e.Task is UpdateGraphAsyncTask)
                        ExecutionEvents.OnGraphPreExecution();
                    break;

                case TaskStateChangedEventArgs.State.ExecutionCompleted:
                    if (e.Task is UpdateGraphAsyncTask)
                    {
                        // Record execution time for update graph task.
                        long start = e.Task.ExecutionStartTime.TickCount;
                        long end = e.Task.ExecutionEndTime.TickCount;
                        var executionTimeSpan = new TimeSpan(end - start);

                        InstrumentationLogger.LogAnonymousTimedEvent(
                            "Perf",
                            e.Task.GetType().Name,
                            executionTimeSpan);

                        Logger.Log("Evaluation completed in " + executionTimeSpan);
                        ExecutionEvents.OnGraphPostExecution();
                    }
                    break;
            }
        }

        private void InitializeNodeLibrary(IPreferences preferences)
        {
            //CustomNodeManager.RecompileAllNodes(EngineController);
            //Loader.ClearCachedAssemblies();

            // Load NodeModels
            foreach (var type in Loader.LoadNodeModels(Context))
            {
                NodeFactory.AddLoader(type.Type, type.AlsoKnownAs);
                AddNodeTypeToSearch(type);
            }

            // Import Zero Touch libs
            var functionGroups = LibraryServices.GetAllFunctionGroups();
            foreach (var funcGroup in functionGroups)
                AddZeroTouchNodeToSearch(funcGroup);
#if DEBUG_LIBRARY
            DumpLibrarySnapshot(functionGroups);
#endif

            // Load Packages
            PackageLoader.DoCachedPackageUninstalls(preferences);
            //TODO(Steve): This will need refactoring
            PackageLoader.LoadPackagesIntoDynamo(preferences, LibraryServices, Loader);

            // Load local custom nodes
            CustomNodeManager.AddUninitializedCustomNodesInPath(DynamoPathManager.Instance.UserDefinitions, IsTestMode);
            CustomNodeManager.AddUninitializedCustomNodesInPath(DynamoPathManager.Instance.CommonDefinitions, IsTestMode);
        }

#if DEBUG_LIBRARY
        private void DumpLibrarySnapshot(IEnumerable<DSEngine.FunctionGroup> functionGroups)
        {
            if (null == functionGroups)
                return;

            var descriptions =
                functionGroups.Select(functionGroup => functionGroup.Functions.ToList())
                    .Where(functions => functions.Any())
                    .SelectMany(
                        functions => 
                            (from function in functions
                             where function.IsVisibleInLibrary
                             let displayString = function.UserFriendlyName
                             where !displayString.Contains("GetType")
                             select string.IsNullOrEmpty(function.Namespace)
                                ? ""
                                : function.Namespace + "." + function.Signature + "\n"));
            
            var sb = string.Join("\n", descriptions);

            Logger.Log(sb, LogLevel.File);
        }
#endif

        private void AddNodeTypeToSearch(TypeLoadData typeLoadData)
        {
            if (!typeLoadData.IsDSCompatible || typeLoadData.IsDeprecated || typeLoadData.IsHidden
                || typeLoadData.IsMetaNode)
            {
                return;
            }

            SearchModel.Add(new NodeModelSearchElement(typeLoadData));
        }

        private void AddZeroTouchNodeToSearch(FunctionGroup funcGroup)
        {
            foreach (var functionDescriptor in funcGroup.Functions)
            {
                AddZeroTouchNodeToSearch(functionDescriptor);
            }
        }

        private void AddZeroTouchNodeToSearch(FunctionDescriptor functionDescriptor)
        {
            if (functionDescriptor.IsVisibleInLibrary && !functionDescriptor.DisplayName.Contains("GetType"))
            {
                SearchModel.Add(new ZeroTouchSearchElement(functionDescriptor));
            }
        }
        
        public void Dispose()
        {
            NodeFactory.MessageLogged -= LogMessage;
            CustomNodeManager.MessageLogged -= LogMessage;
            Loader.MessageLogged -= LogMessage;
            PackageLoader.MessageLogged -= LogMessage;
            LibraryServices.MessageLogged -= LogMessage;

            SearchModel.ItemProduced -= AddNodeToCurrentWorkspace;

            if (PreferenceSettings != null)
            {
                PreferenceSettings.PropertyChanged -= PreferenceSettings_PropertyChanged;
            }
        }

        //SEPARATECORE: causes mono crash
        private void InitializeInstrumentationLogger()
        {
            if (IsTestMode == false)
                InstrumentationLogger.Start(this);
        }

        private void InitializeCurrentWorkspace()
        {
            var defaultWorkspace = new HomeWorkspaceModel(LibraryServices, Scheduler, DebugSettings.VerboseLogging);
            Workspaces.Add(defaultWorkspace);
            CurrentWorkspace = defaultWorkspace;
            //AddHomeWorkspace();
            //CurrentWorkspace = HomeSpace;
            //CurrentWorkspace.X = 0;
            //CurrentWorkspace.Y = 0;
        }

        private static void InitializePreferences(IPreferences preferences)
        {
            BaseUnit.LengthUnit = preferences.LengthUnit;
            BaseUnit.AreaUnit = preferences.AreaUnit;
            BaseUnit.VolumeUnit = preferences.VolumeUnit;
            BaseUnit.NumberFormat = preferences.NumberFormat;
        }

        /// <summary>
        /// Responds to property update notifications on the preferences,
        /// and synchronizes with the Units Manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //TODO(Steve): See if we can't just do this in PreferenceSettings by making the properties directly access BaseUnit
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

        #endregion

        #region save/load

        public void OpenFileFromPath(string xmlPath)
        {
            WorkspaceModel ws;
            if (OpenFile(xmlPath, out ws))
            {
                AddWorkspace(ws);
                CurrentWorkspace = ws;
            }
        }

        private bool OpenFile(string xmlPath, out WorkspaceModel model)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            WorkspaceHeader workspaceInfo;
            if (!WorkspaceHeader.FromXmlDocument(xmlDoc, xmlPath, IsTestMode, Logger, out workspaceInfo))
            {
                model = null;
                return false;
            }

            CustomNodeManager.AddUninitializedCustomNodesInPath(Path.GetDirectoryName(xmlPath), IsTestMode);

            return workspaceInfo.IsCustomNodeWorkspace
                ? CustomNodeManager.OpenCustomNodeWorkspace(xmlDoc, workspaceInfo, IsTestMode, out model)
                : OpenHomeWorkspace(xmlDoc, workspaceInfo, out model);
        }

        private bool OpenHomeWorkspace(
            XmlDocument xmlDoc, WorkspaceHeader workspaceInfo, out WorkspaceModel workspace)
        {
            // TODO(Steve): Refactor to remove dialogs. Results of various dialogs should be passed to this method.
            #region Migration

            Version fileVersion = MigrationManager.VersionFromString(workspaceInfo.Version);

            var currentVersion = AssemblyHelper.GetDynamoVersion(includeRevisionNumber: false);

            if (fileVersion > currentVersion)
            {
                bool resume = Utils.DisplayFutureFileMessage(
                    this,
                    workspaceInfo.FileName,
                    fileVersion,
                    currentVersion);

                if (!resume)
                {
                    workspace = null;
                    return false;
                }
            }

            var decision = MigrationManager.ProcessWorkspace(
                xmlDoc,
                fileVersion,
                currentVersion,
                workspaceInfo.FileName,
                IsTestMode,
                Logger);

            if (decision == MigrationManager.Decision.Abort)
            {
                Utils.DisplayObsoleteFileMessage(this, workspaceInfo.FileName, fileVersion, currentVersion);

                workspace = null;
                return false;
            }

            #endregion

            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, NodeFactory);

            var newWorkspace = new HomeWorkspaceModel(
                LibraryServices,
                Scheduler,
                Utils.LoadTraceDataFromXmlDocument(xmlDoc),
                nodeGraph.Nodes,
                nodeGraph.Connectors,
                nodeGraph.Notes,
                workspaceInfo.X,
                workspaceInfo.Y,
                DebugSettings.VerboseLogging);

            RegisterHomeWorkspace(newWorkspace);
            
            workspace = newWorkspace;
            return true;
        }

        private void RegisterHomeWorkspace(HomeWorkspaceModel newWorkspace)
        {
            foreach (var def in CustomNodeManager.LoadedDefinitions)
                newWorkspace.RegisterCustomNodeDefinitionWithEngine(def);

            CustomNodeManager.DefinitionUpdated += newWorkspace.RegisterCustomNodeDefinitionWithEngine;
            newWorkspace.Disposed +=
                () =>
                    CustomNodeManager.DefinitionUpdated -= newWorkspace.RegisterCustomNodeDefinitionWithEngine;
        }

        #endregion

        #region internal methods

        //TODO(Steve): We shouldn't have to handle post-activation stuff in the model, this stuff can probably go somewhere else.
        internal void PostUIActivation(object parameter)
        {
            Logger.Log("Welcome to Dynamo!");
        }

        [Obsolete("This is always true", true)]
        internal bool CanDoPostUIActivation(object parameter)
        {
            return true;
        }

        [Obsolete("Use CurrentWorkspace.Clear() instead", true)]
        internal void CleanWorkbench()
        {
            Logger.Log("Clearing workflow...");
            
            foreach (NodeModel el in CurrentWorkspace.Nodes)
            {
                el.DisableReporting();
                el.Dispose();

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

            CurrentWorkspace.ClearUndoRecorder();
            CurrentWorkspace.ResetWorkspace();

            // Don't bother resetting the engine during shutdown (especially 
            // since ResetEngine destroys and recreates a new EngineController).
            if (!ShutdownRequested)
                ResetEngine();

            CurrentWorkspace.PreloadedTraceData = null;
        }

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        [Obsolete("This makes no sense with multiple home workspaces.", true)]
        internal void ViewHomeWorkspace()
        {
            CurrentWorkspace = HomeSpace;
        }

        //TODO(Steve): Move to WorkspaceModel
        internal void DeleteModelInternal(List<ModelBase> modelsToDelete)
        {
            if (null == CurrentWorkspace)
                return;

            OnDeletionStarted(this, EventArgs.Empty);

            CurrentWorkspace.RecordAndDeleteModels(modelsToDelete);

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

        [Obsolete("This makes no sense with multiple home workspaces.", true)]
        internal bool CanGoHome(object parameter)
        {
            return CurrentWorkspace != HomeSpace;
        }

        internal void DumpLibraryToXml(object parameter)
        {
            string directory = DynamoPathManager.Instance.Logs;
            string fileName = String.Format("LibrarySnapshot_{0}.xml", DateTime.Now.ToString("yyyyMMddHmmss"));
            string fullFileName = Path.Combine(directory, fileName);

            this.SearchModel.DumpLibraryToXml(fullFileName);

            Logger.Log(string.Format("Library is dumped to \"{0}\".", fullFileName));
        }

        internal bool CanDumpLibraryToXml(object obj)
        {
            return true;
        }

        #endregion

        #region public methods
        
        /// <summary>
        ///     Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            if (Workspaces.Remove(workspace))
                workspace.Dispose();
        }

        /// <summary>
        ///     Adds a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        private void AddWorkspace(WorkspaceModel workspace)
        {
            Action savedHandler = () => OnWorkspaceSaved(workspace);
            workspace.WorkspaceSaved += savedHandler;
            
            workspace.NodeAdded += OnNodeAdded;
            workspace.NodeDeleted += OnNodeDeleted;
            workspace.ConnectorAdded += OnConnectorAdded;
            workspace.MessageLogged += LogMessage;

            workspace.Disposed += () =>
            {
                workspace.WorkspaceSaved -= savedHandler;
                workspace.NodeAdded -= OnNodeAdded;
                workspace.NodeDeleted -= OnNodeDeleted;
                workspace.ConnectorAdded -= OnConnectorAdded;
                workspace.MessageLogged -= LogMessage;
            };

            Workspaces.Add(workspace);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="node"></param>
        public void AddNodeToCurrentWorkspace(NodeModel node)
        {
            CurrentWorkspace.AddNode(node, centered: true);
            OnNodeAdded(node);
            
            //TODO(Steve): This should be moved to WorkspaceModel.AddNode when all workspaces have their own selection.
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(node);
        }
        
        /// <summary>
        /// Copy selected ISelectable objects to the clipboard.
        /// </summary>
        /// <param name="parameters"></param>
        public void Copy(object parameters)
        {
            ClipBoard.Clear();

            foreach (
                var el in
                    DynamoSelection.Instance.Selection.OfType<ModelBase>()
                        .Where(el => !ClipBoard.Contains(el)))
            {
                ClipBoard.Add(el);

                if (!(el is NodeModel))
                    continue;

                var node = el as NodeModel;
                var connectors =
                    node.InPorts.Concat(node.OutPorts).SelectMany(port => port.Connectors)
                        .Where(
                            connector =>
                                connector.End != null && connector.End.Owner.IsSelected
                                    && !ClipBoard.Contains(connector));

                ClipBoard.AddRange(connectors);
            }
        }

        /// <summary>
        ///     Paste ISelectable objects from the clipboard to the workspace.
        /// </summary>
        public void Paste()
        {
            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            var nodeLookup = new Dictionary<Guid, NodeModel>();

            //make a list of all newly created models so that their
            //creations can be recorded in the undo recorder.
            var createdModels = new List<ModelBase>();

            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.ClearSelection();

            var nodes = ClipBoard.OfType<NodeModel>();

            var connectors = ClipBoard.OfType<ConnectorModel>();

            var xmlDoc = new XmlDocument();

            foreach (var node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();

                NodeModel newNode;

                if (CurrentWorkspace is HomeWorkspaceModel && (node is Symbol || node is Output))
                {
                    var symbol = (node is Symbol
                        ? (node as Symbol).InputSymbol
                        : (node as Output).Symbol);
                    var code = (string.IsNullOrEmpty(symbol) ? "x" : symbol) + ";";
                    newNode = new CodeBlockNodeModel(code, LibraryServices.LibraryManagementCore)
                    {
                        X = node.X,
                        Y = node.Y + 100
                    };
                }
                else
                {
                    var dynEl = node.Serialize(xmlDoc, SaveContext.Copy);
                    newNode = NodeFactory.CreateNodeFromXml(dynEl, SaveContext.Copy);
                }

                newNode.ArgumentLacing = node.ArgumentLacing;
                if (!string.IsNullOrEmpty(node.NickName))
                    newNode.NickName = node.NickName;

                CurrentWorkspace.AddNode(newNode, false);
                createdModels.Add(newNode);
                nodeLookup.Add(node.GUID, newNode);
            }

            OnRequestLayoutUpdate(this, EventArgs.Empty);

            NodeModel start;
            NodeModel end;
            var newConnectors =
                from c in connectors
                let startNode =
                    nodeLookup.TryGetValue(c.Start.Owner.GUID, out start)
                        ? start
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.Start.Owner.GUID)
                let endNode =
                    nodeLookup.TryGetValue(c.End.Owner.GUID, out end)
                        ? end
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.End.Owner.GUID)
                where startNode != null && endNode != null
                select
                    ConnectorModel.Make(startNode, endNode, c.Start.Index, c.End.Index);

            foreach (var connector in newConnectors)
            {
                CurrentWorkspace.AddConnection(connector);
                createdModels.Add(connector);
            }

            //process the queue again to create the connectors
            //DynamoCommands.ProcessCommandQueue();

            var notes = ClipBoard.OfType<NoteModel>();

            var newNotes = from note in notes
                           let newGUID = Guid.NewGuid()
                           let sameSpace =
                               CurrentWorkspace.Notes.Any(x => x.GUID == note.GUID)
                           let newX = sameSpace ? note.X + 20 : note.X
                           let newY = sameSpace ? note.Y + 20 : note.Y
                           select new NoteModel(newX, newY, note.Text, newGUID);

            foreach (var newNote in newNotes)
            {
                CurrentWorkspace.AddNote(newNote, false);
                createdModels.Add(newNote);
                AddToSelection(newNote);
            }

            foreach (var de in nodeLookup.Values)
                AddToSelection(de);

            // Record models that are created as part of the command.
            CurrentWorkspace.RecordCreatedModels(createdModels);
        }

        /// <summary>
        /// Add an ISelectable object to the selection.
        /// </summary>
        /// <param name="parameters">The object to add to the selection.</param>
        [Obsolete("Each workspace handles their own selection.", true)]
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
        
        #endregion

        #region private methods

        private void LogMessage(ILogMessage obj)
        {
            Logger.Log(obj);
        }

        #endregion
    }
}
