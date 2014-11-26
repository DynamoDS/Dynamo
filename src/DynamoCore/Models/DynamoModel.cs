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
using Double = System.Double;
using Enum = System.Enum;
using String = System.String;
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
        /// TODO
        /// </summary>
        public DynamoRunner Runner { get; protected set; }

        private DynamoScheduler scheduler;
        public DynamoScheduler Scheduler { get { return scheduler; } }
        public int MaxTesselationDivisions { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public NodeSearchModel SearchModel { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public DebugSettings DebugSettings { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
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
        public bool RunEnabled { get; set; }
        //{
        //    get { return runEnabled; }
        //    set
        //    {
        //        runEnabled = value;
        //        //RaisePropertyChanged("RunEnabled");
        //    }
        //}
        
        /// <summary>
        ///     All nodes in all workspaces. 
        /// </summary>
        //TODO(Steve): Probably should get rid of this...
        [Obsolete("This should be done manually, but ideally not at all.", true)]
        public IEnumerable<NodeModel> AllNodes
        {
            get
            {
                return Workspaces
                    .Aggregate(
                        (IEnumerable<NodeModel>)new List<NodeModel>(),
                        (a, x) => a.Concat(x.Nodes))
                    .Concat(
                        CustomNodeManager.GetLoadedDefinitions().Aggregate(
                            (IEnumerable<NodeModel>)new List<NodeModel>(),
                            (a, x) => a.Concat(x.Workspace.Nodes)
                            ));
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        //TODO(Steve): Investigate if this is necessary
        public ObservableDictionary<string, Guid> CustomNodes
        {
            get { return CustomNodeManager.GetAllNodeNames(); }
        }

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

#if ENABLE_DYNAMO_SCHEDULER
            if (scheduler != null)
            {
                scheduler.Shutdown();
                scheduler.TaskStateChanged -= OnAsyncTaskStateChanged;
                scheduler = null;
            }
#endif
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
            public DynamoRunner Runner { get; set; }
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
            if (configuration.Runner == null)
                configuration.Runner = new DynamoRunner();

            return new DynamoModel(configuration);
        }

        protected DynamoModel(StartConfiguration configuration)
        {
            this.MaxTesselationDivisions = 128;
            string context = configuration.Context;
            IPreferences preferences = configuration.Preferences;
            string corePath = configuration.DynamoCorePath;
            DynamoRunner runner = configuration.Runner;
            bool isTestMode = configuration.StartInTestMode;

            DynamoPathManager.Instance.InitializeCore(corePath);

            Context = context;
            IsTestMode = isTestMode;
            DebugSettings = new DebugSettings();
            Logger = new DynamoLogger(DebugSettings, DynamoPathManager.Instance.Logs);

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));

#if ENABLE_DYNAMO_SCHEDULER
            var thread = configuration.SchedulerThread ?? new DynamoSchedulerThread();
            scheduler = new DynamoScheduler(thread);
            scheduler.TaskStateChanged += OnAsyncTaskStateChanged;
#endif

            Runner = runner;
            Runner.RunStarted += Runner_RunStarted;
            Runner.RunCompleted += Runner_RunCompeleted;
            Runner.EvaluationCompleted += Runner_EvaluationCompleted;
            Runner.ExceptionOccurred += Runner_ExceptionOccurred;
            Runner.MessageLogged += LogMessage;

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

            CustomNodeManager = new CustomNodeManager(DynamoPathManager.Instance.UserDefinitions);
            CustomNodeManager.MessageLogged += LogMessage;
            
            Loader = new DynamoLoader();
            Loader.MessageLogged += LogMessage;

            PackageLoader = new PackageLoader();
            PackageLoader.MessageLogged += LogMessage;

            DisposeLogic.IsShuttingDown = false;

            EngineController = new EngineController(this, DynamoPathManager.Instance.GeometryFactory);

            EngineController.LibraryServices.MessageLogged += LogMessage;

            NodeFactory = new NodeFactory();
            NodeFactory.MessageLogged += LogMessage;
            //TODO(Steve): Ensure this is safe when EngineController is re-created
            NodeFactory.AddLoader(new ZeroTouchNodeLoader(EngineController.LibraryServices));
            NodeFactory.AddLoader(new CustomNodeLoader(CustomNodeManager, IsTestMode));

            UpdateManager.UpdateManager.CheckForProductUpdate();

            // Reset virtual machine to avoid a race condition by causing a 
            // thread join inside the vm exec. Since DynamoModel is being called 
            // on the main/idle thread, it is safe to call ResetEngineInternal 
            // directly (we cannot call virtual method ResetEngine here).
            // 
            ResetEngineInternal();

            //TODO(Steve): Update location where nodes are marked dirty
            foreach (var n in CurrentWorkspace.Nodes)
            {
                n.RequiresRecalc = true;
                n.ForceReExecuteOfNode = true;
            }

            Logger.Log(
                string.Format("Dynamo -- Build {0}", Assembly.GetExecutingAssembly().GetName().Version));

            PackageManagerClient = new PackageManagerClient(PackageLoader.RootPackagesDirectory, CustomNodeManager);

            InitializeNodeLibrary(preferences);
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
            foreach (var funcGroup in EngineController.GetFunctionGroups())
                AddZeroTouchNodeToSearch(funcGroup);

            // Load Packages
            PackageLoader.DoCachedPackageUninstalls(preferences);
            //TODO(Steve): This will need refactoring
            PackageLoader.LoadPackagesIntoDynamo(preferences, TODO);

            // Load local custom nodes
            foreach (var custNodeDef in CustomNodeManager.ScanSearchPaths())
                AddCustomNodeToSearch(custNodeDef);
        }

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

        private void AddCustomNodeToSearch(CustomNodeInfo info)
        {
            SearchModel.Add(new CustomNodeSearchElement(CustomNodeManager, info));
        }

        public void Dispose()
        {
            NodeFactory.MessageLogged -= LogMessage;
            Runner.RunStarted -= Runner_RunStarted;
            Runner.RunCompleted -= Runner_RunCompeleted;
            Runner.EvaluationCompleted -= Runner_EvaluationCompleted;
            Runner.ExceptionOccurred -= Runner_ExceptionOccurred;
            Runner.MessageLogged -= LogMessage;
            CustomNodeManager.MessageLogged -= LogMessage;
            Loader.MessageLogged -= LogMessage;
            PackageLoader.MessageLogged -= LogMessage;
            EngineController.LibraryServices.MessageLogged -= LogMessage;

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
            var defaultWorkspace = new HomeWorkspaceModel();
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

        #region evaluation

        /// <summary>
        /// Force reset of the execution substrait. Executing this will have a negative performance impact
        /// </summary>
        public void Reset()
        {
            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            //TODO(Luke): Push this into a resync call with the engine controller
            ResetEngine();

            foreach (var node in CurrentWorkspace.Nodes)
            {
                //TODO(Steve): Update place where we're tracking modifications, no need to call each individual node.
                node.RequiresRecalc = true; 
            }
        }

        /// <summary>
        /// Call this method to reset the virtual machine, avoiding a race 
        /// condition by using a thread join inside the vm executive.
        /// TODO(Luke): Push this into a resync call with the engine controller
        /// </summary>
        /// <param name="markNodesAsDirty">Set this parameter to true to force 
        /// reset of the execution substrait. Note that setting this parameter 
        /// to true will have a negative performance impact.</param>
        /// 
        public virtual void ResetEngine(bool markNodesAsDirty = false)
        {
            ResetEngineInternal();
            if (markNodesAsDirty)
                Nodes.ForEach(n => n.RequiresRecalc = true);
        }

        protected void ResetEngineInternal()
        {
            if (EngineController != null)
            {
                EngineController.Dispose();
                EngineController = null;
            }

            var geomFactory = DynamoPathManager.Instance.GeometryFactory;
            EngineController = new EngineController(this, geomFactory);
            CustomNodeManager.RecompileAllNodes(EngineController);
        }

        /// <summary>
        /// This method is typically called from the main application thread (as 
        /// a result of user actions such as button click or node UI changes) to
        /// schedule an update of the graph. This call may or may not represent 
        /// an actual update. In the event that the user action does not result 
        /// in actual graph update (e.g. moving of node on UI), the update task 
        /// will not be scheduled for execution.
        /// </summary>
        [Obsolete("Running is now handled on HomeWorkspaceModel", true)]
        public void RunExpression()
        {
            var traceData = HomeSpace.PreloadedTraceData;
            if ((traceData != null) && traceData.Any())
            {
                // If we do have preloaded trace data, set it here first.
                var setTraceDataTask = new SetTraceDataAsyncTask(scheduler);
                if (setTraceDataTask.Initialize(EngineController, HomeSpace))
                    scheduler.ScheduleForExecution(setTraceDataTask);
            }

            // If one or more custom node have been updated, make sure they
            // are compiled first before the home workspace gets evaluated.
            // 
            EngineController.ProcessPendingCustomNodeSyncData(scheduler);

            var task = new UpdateGraphAsyncTask(scheduler);
            if (task.Initialize(EngineController, HomeSpace))
            {
                task.Completed += OnUpdateGraphCompleted;
                RunEnabled = false; // Disable 'Run' button.
                scheduler.ScheduleForExecution(task);
            }
            else
            {
                // Notify handlers that evaluation did not take place.
                var e = new EvaluationCompletedEventArgs(false);
                OnEvaluationCompleted(this, e);
            }
        }

        /// <summary>
        /// This callback method is invoked in the context of ISchedulerThread 
        /// when UpdateGraphAsyncTask is completed.
        /// </summary>
        /// <param name="task">The original UpdateGraphAsyncTask instance.</param>
        private void OnUpdateGraphCompleted(AsyncTask task)
        {
            var updateTask = task as UpdateGraphAsyncTask;
            var messages = new Dictionary<Guid, string>();

            // Runtime warnings take precedence over build warnings.
            foreach (var warning in updateTask.RuntimeWarnings)
            {
                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            foreach (var warning in updateTask.BuildWarnings)
            {
                // If there is already runtime warnings for 
                // this node, then ignore the build warnings.
                if (messages.ContainsKey(warning.Key))
                    continue;

                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            var workspace = updateTask.TargetedWorkspace;
            foreach (var message in messages)
            {
                var guid = message.Key;
                var node = workspace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node == null)
                    continue;

                node.Warning(message.Value); // Update node warning message.
            }

            // This method is guaranteed to be called in the context of 
            // ISchedulerThread (for Revit's case, it is the idle thread).
            // Dispatch the failure message display for execution on UI thread.
            // 
            if (task.Exception != null && (DynamoModel.IsTestMode == false))
            {
                Action showFailureMessage = () =>
                    Utils.DisplayEngineFailureMessage(this, task.Exception);

                OnRequestDispatcherBeginInvoke(showFailureMessage);
            }

            // Refresh values of nodes that took part in update.
            foreach (var modifiedNode in updateTask.ModifiedNodes)
            {
                modifiedNode.RequestValueUpdateAsync();
            }

            // Notify listeners (optional) of completion.
            RunEnabled = true; // Re-enable 'Run' button.

            // Notify handlers that evaluation took place.
            var e = new EvaluationCompletedEventArgs(true);
            OnEvaluationCompleted(this, e);
        }

        /// <summary>
        /// This event handler is invoked when DynamoScheduler changes the state 
        /// of an AsyncTask object. See TaskStateChangedEventArgs.State for more 
        /// details of these state changes.
        /// </summary>
        /// <param name="sender">The scheduler which raised the event.</param>
        /// <param name="e">Task state changed event argument.</param>
        /// 
        private void OnAsyncTaskStateChanged(
            DynamoScheduler sender,
            TaskStateChangedEventArgs e)
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

                        InstrumentationLogger.LogAnonymousTimedEvent("Perf",
                            e.Task.GetType().Name, executionTimeSpan);

                        Logger.Log("Evaluation completed in " + executionTimeSpan);
                        ExecutionEvents.OnGraphPostExecution();
                    }
                    break;
            }
        }

        [Obsolete("Running is now handled on HomeWorksapceModel", true)]
        internal void RunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync(EngineController);
            else
                RunExpression();
        }

        [Obsolete("Running is now handled on HomeWorksapceModel", true)]
        internal void ForceRunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync(EngineController);
            else
            {
                Logger.Log("Beginning engine reset");
                Reset();
                Logger.Log("Reset complete");

                RunExpression();
            }
        }

        private void Runner_RunStarted()
        {
            RunEnabled = false;
        }

        private void Runner_RunCompeleted(bool success, bool cancelled)
        {
            RunEnabled = true;
            if (cancelled)
                Reset();
            OnRunCompleted(this, success);
        }

        private void Runner_EvaluationCompleted()
        {
            OnEvaluationCompleted(this, EventArgs.Empty);
        }

        private void Runner_ExceptionOccurred(Exception exception, bool fatal)
        {
            // If there's a fatal exception, show it to the user, unless of course 
            // if we're running in a unit-test, in which case there's no user. I'd 
            // like not to display the dialog and hold up the continuous integration.
            if (!IsTestMode)
            {
                if (fatal)
                {
                    OnRequestDispatcherBeginInvoke(
                        () => Utils.DisplayEngineFailureMessage(this, exception));
                }
            }
            else
            {
                //If we are testing, we need to throw an exception here
                //which will, in turn, throw an Assert.Fail in the 
                //Evaluation thread.
                throw new Exception(exception.Message);
            }
        }

        #endregion

        #region save/load

        public void OpenFileFromPath(string xmlPath)
        {
            WorkspaceModel ws;
            if (OpenFile(xmlPath, out ws))
                AddWorkspace(ws);
        }

        private bool OpenFile(string xmlPath, out WorkspaceModel model)
        {
            WorkspaceHeader workspaceInfo;
            if (!WorkspaceHeader.FromPath(xmlPath, IsTestMode, Logger, out workspaceInfo))
            {
                model = null;
                return false;
            }

            if (workspaceInfo.IsCustomNodeWorkspace())
            {
                model = 
            }
        }

        [Obsolete("Use OpenFileFromPath()", true)]
        internal void OpenInternal(string xmlPath)
        {
            if (!OpenDefinition(xmlPath))
            {
                WriteToLog("Workbench could not be opened.");
                WriteToLog(xmlPath);
            }
        }

        //TODO(Steve): This belongs in CustomNodeManager
        internal void OpenCustomNodeAndFocus(WorkspaceHeader workspaceHeader)
        {
            // load custom node
            var manager = CustomNodeManager;
            var info = manager.AddFileToPath(workspaceHeader.FileName);

            //TODO(Steve): This is where the custom node workspace is actually "loaded"
            var funcDef = manager.GetFunctionDefinition(info.Guid, TODO); 

            if (funcDef == null) // Fail to load custom function.
                return;

            //TODO(Steve): Current logic -
            //  File with custom node instance is loaded
            //  No custom node definition for instance can be found
            //  CustomNodeManager stores a proxy definition
            //  Here, we check if the new custom node definition is the
            //  missing one that is currently a proxy.

            // New logic:
            //  Load a custom node instance, check for definition
            //      Either sync w/ def OR make proxy
            //      No need to store definition as a proxy
            //  Register event on custom node instance that checks for 
            //  loaded definitions
            //      If loaded def's guid == custom node instance's, resync
            //      This works for proxy nodes AND regular ones

            if (funcDef.IsProxy)
            {
                funcDef = manager.ReloadFunctionDefintion(info.Guid, TODO);
                if (funcDef == null)
                {
                    return;
                }
            }

            funcDef.AddToSearch(SearchModel);

            //TODO(Steve): Handle at load time?
            var ws = funcDef.Workspace;
            ws.Zoom = workspaceHeader.Zoom;
            ws.HasUnsavedChanges = false;

            if (!Workspaces.Contains(ws))
            {
                Workspaces.Add(ws);
            }

            var vm = Workspaces.First(x => x == ws);

            //TODO(Steve): Do this on VM layer?
            vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(workspaceHeader.X, workspaceHeader.Y)));

            CurrentWorkspace = ws;
        }

        [Obsolete("Use AddWorkspace(WorkspaceModel)", true)]
        internal bool OpenDefinition(string xmlPath)
        {
            WorkspaceHeader workspaceInfo; 
            if (WorkspaceHeader.FromPath(xmlPath, IsTestMode, Logger, out workspaceInfo))
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
            //TODO(Steve): It only searches for dyfs in the same directory if we're opening a dyn
            var dirName = Path.GetDirectoryName(xmlPath);
            CustomNodeManager.AddDirectoryToSearchPath(dirName);
            CustomNodeManager.ScanSearchPaths();

            return OpenWorkspace(xmlPath);
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

        #endregion

        #region public methods

        public void RemoveCustomNode(Guid guid)
        {
            CustomNodeManager.RemoveFromDynamo(guid);
            SearchModel.RemoveNodeAndEmptyParentCategory(guid);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public void UpdateCustomNode(
            Guid id, string name = null, string category = null, string description = null)
        {
            CustomNodeManager.Refactor(id, name, category, description);
            SearchModel.Refactor();
        }

        [Obsolete("Remove concept of hiding workspaces alltogether", true)]
        public void HideWorkspace(WorkspaceModel workspace)
        {
            CurrentWorkspace = Workspaces[0];  // go home
            Workspaces.Remove(workspace);
            OnWorkspaceHidden(workspace);
        }

        /// <summary>
        /// Add a workspace to the dynamo model.
        /// </summary>
        [Obsolete("Use AddWorkspace(WorkspaceModel)", true)]
        private void AddHomeWorkspace()
        {
            var workspace = new HomeWorkspaceModel
            {
                WatchChanges = true
            };

            HomeSpace = workspace;
            Workspaces.Insert(0, workspace); // to front
        }

        /// <summary>
        ///     Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            if (Workspaces.Remove(workspace))
            {
                workspace.Dispose();
            }
        }

        /// <summary>
        ///     Adds a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddWorkspace(WorkspaceModel workspace)
        {
            Action modifiedHandler = () => OnWorkspaceSaved(workspace);
            workspace.Modified += modifiedHandler;
            
            //workspace.NodeAdded += OnNodeAdded;
            workspace.ConnectorAdded += OnConnectorAdded;
            workspace.MessageLogged += LogMessage;

            workspace.Disposed += () =>
            {
                workspace.Modified -= modifiedHandler;
                //workspace.NodeAdded -= OnNodeAdded;
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
            CurrentWorkspace.AddNode(node);
            OnNodeAdded(node);
            
            //TODO(Steve): This should be moved to WorkspaceModel.AddNode, when all workspaces have their own selection.
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(node);
        }

        /// <summary>
        ///     Open a workspace from a path.
        /// </summary>
        /// <param name="xmlPath">The path to the workspace.</param>
        [Obsolete("Use AddWorkspace(WorkspaceModel)", true)]
        public bool OpenWorkspace(string xmlPath)
        {
            Logger.Log("Opening home workspace " + xmlPath + "...");

            CleanWorkbench();

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

                foreach (
                    XmlAttribute att in
                        from XmlNode node in workspaceNodes
                        from XmlAttribute att in node.Attributes
                        select att)
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

                Version fileVersion = MigrationManager.VersionFromString(version);
                var currentVersion = MigrationManager.VersionFromWorkspace(HomeSpace);

                if (fileVersion > currentVersion)
                {
                    bool resume = Utils.DisplayFutureFileMessage(this, xmlPath, fileVersion, currentVersion);
                    if (!resume)
                        return false;                    
                }

                var decision = MigrationManager.ShouldMigrateFile(fileVersion, currentVersion);
                switch (decision)
                {
                    case MigrationManager.Decision.Abort:
                        Utils.DisplayObsoleteFileMessage(this, xmlPath, fileVersion, currentVersion);
                        return false;
                    case MigrationManager.Decision.Migrate:
                        string backupPath = String.Empty;
                        if (!IsTestMode
                            && MigrationManager.BackupOriginalFile(xmlPath, ref backupPath))
                        {
                            string message = String.Format(
                                "Original file '{0}' gets backed up at '{1}'",
                                Path.GetFileName(xmlPath),
                                backupPath);

                            Logger.Log(message);
                        }

                        //Hardcode the file version to 0.6.0.0. The file whose version is 0.7.0.x
                        //needs to be forced to be migrated. The version number needs to be changed from
                        //0.7.0.x to 0.6.0.0.
                        if (fileVersion == new Version(0, 7, 0, 0))
                            fileVersion = new Version(0, 6, 0, 0);

                        MigrationManager.Instance.ProcessWorkspaceMigrations(
                            currentVersion,
                            xmlDoc,
                            fileVersion)
                        MigrationManager.Instance.ProcessNodesInWorkspace(this, xmlDoc, fileVersion);
                        break;
                }

                //set the zoom and offsets and trigger events
                //to get the view to position iteself
                CurrentWorkspace.X = cx;
                CurrentWorkspace.Y = cy;
                CurrentWorkspace.Zoom = zoom;

                var vm = Workspaces.First(x => x == CurrentWorkspace);
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
                        isVisible = isVisAttrib.Value == "true";

                    bool isUpstreamVisible = true;
                    if (isUpstreamVisAttrib != null)
                        isUpstreamVisible = isUpstreamVisAttrib.Value == "true";

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
                            el = NodeFactory.CreateNodeInstance(type, nickname, signature, guid, Logger);

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

                        el = NodeFactory.CreateNodeInstance(type, nickname, String.Empty, guid, Logger);
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
                            LacingStrategy lacing;
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
                    var portType = ((PortType) Convert.ToInt16(portTypeAttrib.Value));

                    //find the elements to connect
                    NodeModel start = null;
                    NodeModel end = null;

                    foreach (NodeModel e in CurrentWorkspace.Nodes)
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

                    var newConnector = CurrentWorkspace.AddConnection(
                        start,
                        end,
                        startIndex,
                        endIndex,
                        Logger,
                        portType);

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

                // We don't want to put this action into Dispatcher's queue 
                // in test mode because it would never get a chance to execute.
                // As Dispatcher is a static object, DynamoModel instance will 
                // be referenced by Dispatcher until nunit finishes all test 
                // cases. 
                if (!IsTestMode)
                {
                    // http://www.japf.fr/2009/10/measure-rendering-time-in-a-wpf-application/comment-page-1/#comment-2892
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            sw.Stop();
                            Logger.Log(String.Format("{0} ellapsed for loading workspace.", sw.Elapsed));
                        }));
                }

                #endregion

                HomeSpace.FileName = xmlPath;

                // Allow live runner a chance to preload trace data from XML.
                var engine = EngineController;
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
                return false;
            }

            CurrentWorkspace.HasUnsavedChanges = false;

            return true;
        }

        [Obsolete("Use AddCustomNodeWorkspace(CustomNodeWorkspaceModel) instead", true)]
        public CustomNodeDefinition NewCustomNodeWorkspace(
            Guid id, string name, string category, string description, bool makeCurrentWorkspace,
            double workspaceOffsetX = 0, double workspaceOffsetY = 0)
        {
            var workSpace = new CustomNodeWorkspaceModel(name,
                category,
                description,
                workspaceOffsetX,
                workspaceOffsetY) { WatchChanges = true };

            Workspaces.Add(workSpace);

            var functionDefinition = new CustomNodeDefinition(id) { Workspace = workSpace };

            functionDefinition.SyncWithWorkspace(this, true, true);

            if (makeCurrentWorkspace)
            {
                CurrentWorkspace = workSpace;
            }

            return functionDefinition;
        }

        /// <summary>
        ///     Write a message to the log.
        /// </summary>
        /// <param name="parameters">The message.</param>
        [Obsolete("Call Logger directly.", true)]
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
        public void Copy(object parameters) // TODO(Steve): Route to CurrentWorkspace
        {
            ClipBoard.Clear();

            foreach (
                var el in
                    DynamoSelection.Instance.Selection.OfType<ModelBase>()
                        .Where(el => !ClipBoard.Contains(el)))
            {
                ClipBoard.Add(el);

                //dynNodeView n = el as dynNodeView;
                var n = el as NodeModel;
                if (n == null)
                    continue;
                var connectors = n.InPorts.ToList().SelectMany(x => x.Connectors)
                    .Concat(n.OutPorts.ToList().SelectMany(x => x.Connectors))
                    .Where(x => x.End != null && x.End.Owner.IsSelected && !ClipBoard.Contains(x));

                ClipBoard.AddRange(connectors);
            }
        }

        /// <summary>
        ///     Paste ISelectable objects from the clipboard to the workspace.
        /// </summary>
        public void Paste() //TODO(Steve): Route to CurrentWorkspace
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

            var nodes = ClipBoard.OfType<NodeModel>();

            var connectors = ClipBoard.OfType<ConnectorModel>();

            foreach (NodeModel node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();
                nodeLookup.Add(node.GUID, newGuid);

                string nodeName = node.GetType().ToString();

                if (node is Function)
                    nodeName = ((node as Function).Definition.FunctionId).ToString();
                else if (node is DSFunction)
                    nodeName = ((node as DSFunction).Controller.MangledName);
                else if (node is DSVarArgFunction)
                    nodeName = ((node as DSVarArgFunction).Controller.MangledName);
                
                var xmlDoc = new XmlDocument();

                NodeModel newNode;

                if (CurrentWorkspace is HomeWorkspaceModel && (node is Symbol || node is Output))
                {
                    var symbol = (node is Symbol
                        ? (node as Symbol).InputSymbol
                        : (node as Output).Symbol);
                    var code = (string.IsNullOrEmpty(symbol) ? "x" : symbol) + ";";
                    newNode = new CodeBlockNodeModel(CurrentWorkspace, code);

                    CurrentWorkspace.AddNode(newNode, newGuid, node.X, node.Y + 100, false, false);
                }
                else
                {
                    var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
                    xmlDoc.AppendChild(dynEl);
                    node.Save(xmlDoc, dynEl, SaveContext.Copy);

                    newNode = CurrentWorkspace.AddNode(
                        newGuid,
                        nodeName,
                        node.X,
                        node.Y + 100,
                        false,
                        false,
                        dynEl);
                }

                createdModels.Add(newNode);

                newNode.ArgumentLacing = node.ArgumentLacing;
                if (!string.IsNullOrEmpty(node.NickName))
                {
                    newNode.NickName = node.NickName;
                }
            }

            OnRequestLayoutUpdate(this, EventArgs.Empty);

            Guid start;
            Guid end;
            createdModels.AddRange(
                from c in connectors
                let startGuid =
                    nodeLookup.TryGetValue(c.Start.Owner.GUID, out start)
                        ? start
                        : c.Start.Owner.GUID
                let endGuid =
                    nodeLookup.TryGetValue(c.End.Owner.GUID, out end)
                        ? end
                        : c.End.Owner.GUID
                let startNode = CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == startGuid)
                let endNode = CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == endGuid)
                where startNode != null && endNode != null
                where startNode.Workspace == CurrentWorkspace
                select
                    CurrentWorkspace.AddConnection(startNode, endNode, c.Start.Index, c.End.Index, TODO));

            //process the queue again to create the connectors
            //DynamoCommands.ProcessCommandQueue();

            var notes = ClipBoard.OfType<NoteModel>();

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

        /// <summary>
        /// View the home workspace.
        /// </summary>
        /// <param name="parameter"></param>
        [Obsolete("This makes no sense with multiple home workspaces.", true)]
        public void Home(object parameter)
        {
            ViewHomeWorkspace();
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
