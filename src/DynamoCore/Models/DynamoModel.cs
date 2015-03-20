using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
﻿using DSCoreNodesUI;
using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Library;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UpdateManager;
using Dynamo.Utilities;

using DynamoServices;

using DynamoUnits;
using DynamoUtilities;
using Greg; // Dynamo package manager
using ProtoCore;
using Dynamo.Models.NodeLoaders;
using Dynamo.Search.SearchElements;
using ProtoCore.Exceptions;
using FunctionGroup = Dynamo.DSEngine.FunctionGroup;
using Symbol = Dynamo.Nodes.Symbol;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Models
{
    public interface IEngineControllerManager
    {
        EngineController EngineController { get; }
    }

    public partial class DynamoModel : INotifyPropertyChanged, IDisposable, IEngineControllerManager // : ModelBase
    {
        public static readonly int MAX_TESSELLATION_DIVISIONS_DEFAULT = 128;

        #region private members

        private readonly string geometryFactoryPath;
        private readonly PathManager pathManager;
        private WorkspaceModel currentWorkspace;

        #endregion

        #region events

        public delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);
        public event FunctionNamePromptRequestHandler RequestsFunctionNamePrompt;
        public void OnRequestsFunctionNamePrompt(Object sender, FunctionNamePromptEventArgs e)
        {
            if (RequestsFunctionNamePrompt != null)
                RequestsFunctionNamePrompt(this, e);
        }

        public event WorkspaceHandler WorkspaceSaved;
        internal void OnWorkspaceSaved(WorkspaceModel model)
        {
            if (WorkspaceSaved != null)
                WorkspaceSaved(model);
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
        public static bool IsTestMode
        {
            get { return isTestMode; }
            set
            {
                isTestMode = value;
                InstrumentationLogger.IsTestMode = value;
            }
        }

        private static bool isTestMode;

        /// <summary>
        ///     Specifies whether or not Dynamo is in a crash-state.
        /// </summary>
        public static bool IsCrashing { get; set; }

        /// <summary>
        /// Setting this flag enables creation of an XML in following format that records 
        /// node mapping information - which old node has been converted to which to new node(s) 
        /// </summary>
        public static bool EnableMigrationLogging { get; set; }

        #endregion

        #region public properties

        /// <summary>
        ///     DesignScript VM EngineController, used for this instance of Dynamo.
        /// </summary>
        public EngineController EngineController { get; set; }

        /// <summary>
        ///     Manages all loaded ZeroTouch libraries.
        /// </summary>
        public readonly LibraryServices LibraryServices;

        /// <summary>
        ///     Flag specifying whether a shutdown of Dynamo was requested.
        /// </summary>
        public bool ShutdownRequested { get; internal set; }

        /// <summary>
        ///     This version of Dynamo.
        /// </summary>
        public string Version
        {
            get { return UpdateManager.UpdateManager.Instance.ProductVersion.ToString(); }
        }

        /// <summary>
        ///     The path manager that configures path information required for 
        ///     Dynamo to function properly. See IPathManager interface for more 
        ///     details.
        /// </summary>
        public IPathManager PathManager { get { return pathManager; } }

        /// <summary>
        ///     The context that Dynamo is running under.
        /// </summary>
        public readonly string Context;

        /// <summary>
        ///     Manages all loaded NodeModel libraries.
        /// </summary>
        public readonly DynamoLoader Loader;

        /// <summary>
        ///     Manages loading of packages.
        /// </summary>
        public readonly PackageLoader PackageLoader;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        public readonly PackageManagerClient PackageManagerClient;

        /// <summary>
        ///     Custom Node Manager instance, manages all loaded custom nodes.
        /// </summary>
        public readonly CustomNodeManager CustomNodeManager;

        /// <summary>
        ///     The Dynamo Logger, receives and manages all log messages.
        /// </summary>
        public readonly DynamoLogger Logger;

        /// <summary>
        ///     The Dynamo Scheduler, handles scheduling of asynchronous tasks on different
        ///     threads.
        /// </summary>
        public DynamoScheduler Scheduler { get; private set; }

        /// <summary>
        ///     The maximum amount of tesselation divisions used for geometry visualization.
        /// </summary>
        public int MaxTesselationDivisions { get; set; }

        /// <summary>
        ///     The Dynamo Node Library, complete with Search.
        /// </summary>
        public readonly NodeSearchModel SearchModel;

        /// <summary>
        ///     The application version string for analytics reporting APIs
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
        ///     Debugging settings for this instance of Dynamo.
        /// </summary>
        public readonly DebugSettings DebugSettings;

        /// <summary>
        ///     Preference settings for this instance of Dynamo.
        /// </summary>
        public readonly PreferenceSettings PreferenceSettings;

        /// <summary>
        ///     Node Factory, used for creating and intantiating loaded Dynamo nodes.
        /// </summary>
        public readonly NodeFactory NodeFactory;

        /// <summary>
        ///     Migration Manager, upgrades old Dynamo file formats to the current version.
        /// </summary>
        public readonly MigrationManager MigrationManager;

        /// <summary>
        ///     The active workspace in Dynamo.
        /// </summary>
        public WorkspaceModel CurrentWorkspace
        {
            get { return currentWorkspace; }
            set
            {
                if (Equals(value, currentWorkspace)) return;
                var old = currentWorkspace;
                currentWorkspace = value;
                OnWorkspaceHidden(old);
                OnPropertyChanged("CurrentWorkspace");
            }
        }

        /// <summary>
        ///     The copy/paste clipboard.
        /// </summary>
        public ObservableCollection<ModelBase> ClipBoard { get; set; }

        /// <summary>
        ///     Specifies whether connectors are displayed in Dynamo.
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
        ///     Specifies how connectors are displayed in Dynamo.
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
        ///     The private collection of visible workspaces in Dynamo
        /// </summary>
        private readonly List<WorkspaceModel> _workspaces = new List<WorkspaceModel>();

        public IEnumerable<WorkspaceModel> Workspaces 
        {
            get { return _workspaces; } 
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
            Dispose();
            PreferenceSettings.SaveInternal(pathManager.PreferenceFilePath);

            OnCleanup();

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

        public interface IStartConfiguration
        {
            string Context { get; set; }
            string DynamoCorePath { get; set; }
            IPreferences Preferences { get; set; }
            IPathResolver PathResolver { get; set; }
            bool StartInTestMode { get; set; }
            IUpdateManager UpdateManager { get; set; }
            ISchedulerThread SchedulerThread { get; set; }
            string GeometryFactoryPath { get; set; }
            IAuthProvider AuthProvider { get; set; }
            string PackageManagerAddress { get; set; }
        }

        /// <summary>
        /// Initialization settings for DynamoModel.
        /// </summary>
        public struct DefaultStartConfiguration : IStartConfiguration
        {
            public string Context { get; set; }
            public string DynamoCorePath { get; set; }
            public IPreferences Preferences { get; set; }
            public IPathResolver PathResolver { get; set; }
            public bool StartInTestMode { get; set; }
            public IUpdateManager UpdateManager { get; set; }
            public ISchedulerThread SchedulerThread { get; set; }
            public string GeometryFactoryPath { get; set; }
            public IAuthProvider AuthProvider { get; set; }
            public string PackageManagerAddress { get; set; }
        }

        /// <summary>
        ///     Start DynamoModel with all default configuration options
        /// </summary>
        /// <returns></returns>
        public static DynamoModel Start()
        {
            return Start(new DefaultStartConfiguration());
        }

        /// <summary>
        /// Start DynamoModel with custom configuration.  Defaults will be assigned not provided.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static DynamoModel Start(IStartConfiguration configuration)
        {
            // where necessary, assign defaults
            if (string.IsNullOrEmpty(configuration.Context))
                configuration.Context = Core.Context.NONE;

            return new DynamoModel(configuration);
        }

        
        protected DynamoModel(IStartConfiguration config)
        {
            ClipBoard = new ObservableCollection<ModelBase>();
            MaxTesselationDivisions = MAX_TESSELLATION_DIVISIONS_DEFAULT;

            pathManager = new PathManager(new PathManagerParams
            {
                CorePath = config.DynamoCorePath,
                PathResolver = config.PathResolver
            });

            // Ensure we have all directories in place.
            pathManager.EnsureDirectoryExistence();

            Context = config.Context;
            IsTestMode = config.StartInTestMode;
            DebugSettings = new DebugSettings();
            Logger = new DynamoLogger(DebugSettings, pathManager.LogDirectory);

            MigrationManager = new MigrationManager(DisplayFutureFileMessage, DisplayObsoleteFileMessage);
            MigrationManager.MessageLogged += LogMessage;
            MigrationManager.MigrationTargets.Add(typeof(WorkspaceMigrations));

            var thread = config.SchedulerThread ?? new DynamoSchedulerThread();
            Scheduler = new DynamoScheduler(thread, IsTestMode);
            Scheduler.TaskStateChanged += OnAsyncTaskStateChanged;

            geometryFactoryPath = config.GeometryFactoryPath;

            IPreferences preferences = CreateOrLoadPreferences(config.Preferences);
            var settings = preferences as PreferenceSettings;
            if (settings != null)
            {
                PreferenceSettings = settings;
                PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            }

            InitializePreferences(preferences);
            InitializeInstrumentationLogger();

            if (this.PreferenceSettings.IsFirstRun)
            {
                DynamoMigratorBase migrator = null;
                try
                {
                    migrator = DynamoMigratorBase.MigrateBetweenDynamoVersions(pathManager, config.PathResolver);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }
                if (migrator != null)
                    this.PreferenceSettings = migrator.PreferenceSettings;
            }

            SearchModel = new NodeSearchModel();
            SearchModel.ItemProduced +=
                node => ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, true));

            NodeFactory = new NodeFactory();
            NodeFactory.MessageLogged += LogMessage;

            CustomNodeManager = new CustomNodeManager(NodeFactory, MigrationManager);
            InitializeCustomNodeManager();

            Loader = new DynamoLoader();
            Loader.MessageLogged += LogMessage;

            PackageLoader = new PackageLoader(pathManager.PackagesDirectory);
            PackageLoader.MessageLogged += LogMessage;

            DisposeLogic.IsShuttingDown = false;

            // Create a core which is used for parsing code and loading libraries
            var libraryCore =
                new ProtoCore.Core(new Options { RootCustomPropertyFilterPathName = string.Empty });

            libraryCore.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(libraryCore));
            libraryCore.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            LibraryServices = new LibraryServices(libraryCore, pathManager);
            LibraryServices.MessageLogged += LogMessage;
            LibraryServices.LibraryLoaded += LibraryLoaded;

            ResetEngineInternal();

            AddHomeWorkspace();

            if (!IsTestMode)
                UpdateManager.UpdateManager.CheckForProductUpdate();

            Logger.Log(
                string.Format("Dynamo -- Build {0}", Assembly.GetExecutingAssembly().GetName().Version));

            var url = config.PackageManagerAddress ??
                      AssemblyConfiguration.Instance.GetAppSetting("packageManagerAddress");

            PackageManagerClient = InitializePackageManager(config.AuthProvider, url,
                PackageLoader.RootPackagesDirectory, CustomNodeManager);

            Logger.Log("Dynamo will use the package manager server at : " + url);

            InitializeNodeLibrary(preferences);
        }

        /// <summary>
        /// LibraryLoaded event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryLoaded(object sender, LibraryServices.LibraryLoadedEventArgs e)
        {
            string newLibrary = e.LibraryPath;

            // Load all functions defined in that library.
            AddZeroTouchNodesToSearch(LibraryServices.GetFunctionGroups(newLibrary));
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

                        var ws = currentWorkspace as HomeWorkspaceModel;
                        if (ws != null && ws.RunSettings.RunType != RunType.Periodic)
                        {
                            Logger.Log(String.Format(Properties.Resources.EvaluationCompleted, executionTimeSpan));
                        }

                        ExecutionEvents.OnGraphPostExecution();
                    }
                    break;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            LibraryServices.Dispose();
            LibraryServices.LibraryManagementCore.__TempCoreHostForRefactoring.Cleanup();
            Logger.Dispose();

            EngineController.Dispose();
            EngineController = null;

            if (PreferenceSettings != null)
            {
                PreferenceSettings.PropertyChanged -= PreferenceSettings_PropertyChanged;
            }

            foreach (var ws in _workspaces)
            {
                ws.Dispose(); 
            }
        }

        /// <summary>
        ///     Validate the package manager url and initialize the PackageManagerClient object
        /// </summary>
        /// <param name="provider">A possibly null IAuthProvider</param>
        /// <param name="url">The end point for the package manager server</param>
        /// <param name="rootDirectory">The root directory for the package manager</param>
        /// <param name="customNodeManager">A valid CustomNodeManager object</param>
        /// <returns>Newly created object</returns>
        private static PackageManagerClient InitializePackageManager(IAuthProvider provider, string url, string rootDirectory,
            CustomNodeManager customNodeManager)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Incorrectly formatted URL provided for Package Manager address.", "url");
            }

            return new PackageManagerClient(
                new GregClient(provider, url),
                rootDirectory,
                customNodeManager);
        }

        private void InitializeCustomNodeManager()
        {
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
                        var workspacesToRemove = _workspaces.FindAll(w => w is CustomNodeWorkspaceModel
                            && (w as CustomNodeWorkspaceModel).CustomNodeId == id);
                        workspacesToRemove.ForEach(w => RemoveWorkspace(w));
                    }
                };
            };
            CustomNodeManager.DefinitionUpdated += RegisterCustomNodeDefinitionWithEngine;
        }

        private void InitializeIncludedNodes()
        {
            NodeFactory.AddLoader(new CustomNodeLoader(CustomNodeManager, IsTestMode));

            var dsFuncData = new TypeLoadData(typeof(DSFunction));
            var dsVarArgFuncData = new TypeLoadData(typeof(DSVarArgFunction));
            var cbnData = new TypeLoadData(typeof(CodeBlockNodeModel));
            var dummyData = new TypeLoadData(typeof(DummyNode));
            var symbolData = new TypeLoadData(typeof(Symbol));
            var outputData = new TypeLoadData(typeof(Output));

            var ztLoader = new ZeroTouchNodeLoader(LibraryServices);
            NodeFactory.AddLoader(dsFuncData.Type, ztLoader);
            NodeFactory.AddAlsoKnownAs(dsFuncData.Type, dsFuncData.AlsoKnownAs);
            NodeFactory.AddLoader(dsVarArgFuncData.Type, ztLoader);
            NodeFactory.AddAlsoKnownAs(dsVarArgFuncData.Type, dsVarArgFuncData.AlsoKnownAs);

            var cbnLoader = new CodeBlockNodeLoader(LibraryServices);
            NodeFactory.AddLoader(cbnData.Type, cbnLoader);
            NodeFactory.AddFactory(cbnData.Type, cbnLoader);
            NodeFactory.AddAlsoKnownAs(cbnData.Type, cbnData.AlsoKnownAs);

            NodeFactory.AddTypeFactoryAndLoader(dummyData.Type);
            NodeFactory.AddAlsoKnownAs(dummyData.Type, dummyData.AlsoKnownAs);

            NodeFactory.AddTypeFactoryAndLoader(symbolData.Type);
            NodeFactory.AddAlsoKnownAs(symbolData.Type, symbolData.AlsoKnownAs);
            NodeFactory.AddTypeFactoryAndLoader(outputData.Type);
            NodeFactory.AddAlsoKnownAs(outputData.Type, outputData.AlsoKnownAs);

            SearchModel.Add(new CodeBlockNodeSearchElement(cbnData, LibraryServices));

            var symbolSearchElement = new NodeModelSearchElement(symbolData)
            {
                IsVisibleInSearch = CurrentWorkspace is CustomNodeWorkspaceModel
            };
            var outputSearchElement = new NodeModelSearchElement(outputData)
            {
                IsVisibleInSearch = CurrentWorkspace is CustomNodeWorkspaceModel
            };

            WorkspaceHidden += _ =>
            {
                var isVisible = CurrentWorkspace is CustomNodeWorkspaceModel;
                symbolSearchElement.IsVisibleInSearch = isVisible;
                outputSearchElement.IsVisibleInSearch = isVisible;
            };

            SearchModel.Add(symbolSearchElement);
            SearchModel.Add(outputSearchElement);
        }

        private void InitializeNodeLibrary(IPreferences preferences)
        {
            // Initialize all nodes inside of this assembly.
            InitializeIncludedNodes();

            List<TypeLoadData> modelTypes;
            List<TypeLoadData> migrationTypes;
            Loader.LoadNodeModelsAndMigrations(pathManager.NodeDirectories,
                Context, out modelTypes, out migrationTypes);

            // Load NodeModels
            foreach (var type in modelTypes)
            {
                // Protect ourselves from exceptions thrown by malformed third party nodes.
                try
                {
                    NodeFactory.AddTypeFactoryAndLoader(type.Type);
                    NodeFactory.AddAlsoKnownAs(type.Type, type.AlsoKnownAs);
                    AddNodeTypeToSearch(type);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }

            // Load migrations
            foreach (var type in migrationTypes)
                MigrationManager.AddMigrationType(type);

            // Import Zero Touch libs
            var functionGroups = LibraryServices.GetAllFunctionGroups();
            if (!DynamoModel.IsTestMode)
                AddZeroTouchNodesToSearch(functionGroups);
#if DEBUG_LIBRARY
            DumpLibrarySnapshot(functionGroups);
#endif

            // Load Packages
            PackageLoader.DoCachedPackageUninstalls(preferences);

            PackageLoader.LoadPackagesIntoDynamo(new LoadPackageParams
            {
                Preferences = preferences,
                PathManager = pathManager,
                LibraryServices = LibraryServices,
                Loader = Loader,
                Context = Context,
                IsTestMode = IsTestMode,
                CustomNodeManager = CustomNodeManager
            });

            // Load local custom nodes
            CustomNodeManager.AddUninitializedCustomNodesInPath(pathManager.UserDefinitions, IsTestMode);
            CustomNodeManager.AddUninitializedCustomNodesInPath(pathManager.CommonDefinitions, IsTestMode);
        }

        private void InitializeInstrumentationLogger()
        {
            if (IsTestMode == false)
                InstrumentationLogger.Start(this);
        }

        private IPreferences CreateOrLoadPreferences(IPreferences preferences)
        {
            if (preferences != null) // If there is preference settings provided...
                return preferences;

            // Is order for test cases not to interfere with the regular preference 
            // settings xml file, a test case usually specify a temporary xml file 
            // path from where preference settings are to be loaded. If that value 
            // is not set, then fall back to the file path specified in PathManager.
            // 
            var xmlFilePath = PreferenceSettings.DynamoTestPath;
            if (string.IsNullOrEmpty(xmlFilePath))
                xmlFilePath = pathManager.PreferenceFilePath;

            if (File.Exists(xmlFilePath))
            {
                // If the specified xml file path exists, load it.
                return PreferenceSettings.Load(xmlFilePath);
            }

            // Otherwise make a default preference settings object.
            return new PreferenceSettings();
        }

        private static void InitializePreferences(IPreferences preferences)
        {
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
                case "NumberFormat":
                    BaseUnit.NumberFormat = PreferenceSettings.NumberFormat;
                    break;
            }
        }

        #endregion

        #region engine management

        /// <summary>
        ///     Registers (or re-registers) a Custom Node definition with the DesignScript VM,
        ///     so that instances of the custom node can be evaluated.
        /// </summary>
        /// <param name="definition"></param>
        private void RegisterCustomNodeDefinitionWithEngine(CustomNodeDefinition definition)
        {
            EngineController.GenerateGraphSyncDataForCustomNode(
                Workspaces.OfType<HomeWorkspaceModel>().SelectMany(ws => ws.Nodes),
                definition,
                DebugSettings.VerboseLogging);

            MarkAllDependenciesAsModified(definition);
        }

        /// <summary>
        /// Get all function instances or directly or indrectly dependo on the 
        /// specified function definition and mark them as modified so that 
        /// their values will be re-queryed.
        /// </summary>
        /// <param name="functionId"></param>
        /// <returns></returns>
        private void MarkAllDependenciesAsModified(CustomNodeDefinition def)
        {
            var homeWorkspace = Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            if (homeWorkspace == null)
                return;

            var dependencies = CustomNodeManager.GetAllDependenciesGuids(def);
            var funcNodes = homeWorkspace.Nodes.OfType<Function>();
            var dirtyNodes = funcNodes.Where(n => dependencies.Contains(n.Definition.FunctionId));
            homeWorkspace.MarkNodesAsModifiedAndRequestRun(dirtyNodes);
        }

        /// <summary>
        /// Call this method to reset the virtual machine, avoiding a race 
        /// condition by using a thread join inside the vm executive.
        /// TODO(Luke): Push this into a resync call with the engine controller
        /// </summary>
        /// <param name="markNodesAsDirty">Set this parameter to true to force 
        ///     reset of the execution substrait. Note that setting this parameter 
        ///     to true will have a negative performance impact.</param>
        public virtual void ResetEngine(bool markNodesAsDirty = false)
        {
            ResetEngineInternal();
            foreach (var workspaceModel in Workspaces.OfType<HomeWorkspaceModel>())
                workspaceModel.ResetEngine(EngineController, markNodesAsDirty);
        }

        protected void ResetEngineInternal()
        {
            if (EngineController != null)
            {
                EngineController.MessageLogged -= LogMessage;
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(
                LibraryServices,
                geometryFactoryPath,
                DebugSettings.VerboseLogging);
            EngineController.MessageLogged += LogMessage;

            foreach (var def in CustomNodeManager.LoadedDefinitions)
                RegisterCustomNodeDefinitionWithEngine(def);
        }

        /// <summary>
        ///     Forces an evaluation of the current workspace by resetting the DesignScript VM.
        /// </summary>
        public void ForceRun()
        {
            Logger.Log("Beginning engine reset");
            ResetEngine(true);
            Logger.Log("Reset complete");

            ((HomeWorkspaceModel)CurrentWorkspace).Run();
        }

        #endregion

        #region save/load

        /// <summary>
        ///     Opens a Dynamo workspace from a path to an Xml file on disk.
        /// </summary>
        /// <param name="xmlPath"></param>
        public void OpenFileFromPath(string xmlPath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            WorkspaceInfo workspaceInfo;
            if (WorkspaceInfo.FromXmlDocument(xmlDoc, xmlPath, IsTestMode, Logger, out workspaceInfo))
            {
                if (MigrationManager.ProcessWorkspace(workspaceInfo, xmlDoc, IsTestMode, NodeFactory))
                {
                    WorkspaceModel ws;
                    if (OpenFile(workspaceInfo, xmlDoc, out ws))
                    {
                        AddWorkspace(ws);
                        CurrentWorkspace = ws;
                        return;
                    }
                }
            }
            Logger.LogError("Could not open workspace at: " + xmlPath);
        }

        private bool OpenFile(WorkspaceInfo workspaceInfo, XmlDocument xmlDoc, out WorkspaceModel workspace)
        {
            CustomNodeManager.AddUninitializedCustomNodesInPath(
                Path.GetDirectoryName(workspaceInfo.FileName),
                IsTestMode);

            var result = workspaceInfo.IsCustomNodeWorkspace
                ? CustomNodeManager.OpenCustomNodeWorkspace(xmlDoc, workspaceInfo, IsTestMode, out workspace)
                : OpenHomeWorkspace(xmlDoc, workspaceInfo, out workspace);

            workspace.OnCurrentOffsetChanged(
                this,
                new PointEventArgs(new Point2D(workspaceInfo.X, workspaceInfo.Y)));

            return result;
        }

        private bool OpenHomeWorkspace(
            XmlDocument xmlDoc, WorkspaceInfo workspaceInfo, out WorkspaceModel workspace)
        {
            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, NodeFactory);

            var newWorkspace = new HomeWorkspaceModel(
                EngineController,
                Scheduler,
                NodeFactory,
                Utils.LoadTraceDataFromXmlDocument(xmlDoc),
                nodeGraph.Nodes,
                nodeGraph.Notes,
                workspaceInfo,
                DebugSettings.VerboseLogging, 
                IsTestMode
               );

            RegisterHomeWorkspace(newWorkspace);
           
            workspace = newWorkspace;

            return true;
        }

        private void RegisterHomeWorkspace(HomeWorkspaceModel newWorkspace)
        {
            newWorkspace.EvaluationCompleted += OnEvaluationCompleted;
            newWorkspace.Disposed += () =>
            {
                newWorkspace.EvaluationCompleted -= OnEvaluationCompleted;
            };
        }

        #endregion

        #region internal methods

        internal void PostUIActivation(object parameter)
        {
            Logger.Log(Properties.Resources.WelcomeMessage);
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
                var node = model as NodeModel;
                if (node != null)
                    node.Dispose();
            }

            OnDeletionComplete(this, EventArgs.Empty);
        }

        internal void DumpLibraryToXml(object parameter)
        {
            string fileName = String.Format("LibrarySnapshot_{0}.xml", DateTime.Now.ToString("yyyyMMddHmmss"));
            string fullFileName = Path.Combine(pathManager.LogDirectory, fileName);

            SearchModel.DumpLibraryToXml(fullFileName);

            Logger.Log(string.Format(Properties.Resources.LibraryIsDumped, fullFileName));
        }

        internal bool CanDumpLibraryToXml(object obj)
        {
            return true;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Add a new HomeWorkspace and set as current
        /// </summary>
        public void AddHomeWorkspace()
        {
            var defaultWorkspace = new HomeWorkspaceModel(
                EngineController,
                Scheduler,
                NodeFactory,
                DebugSettings.VerboseLogging,
                IsTestMode,string.Empty);

            RegisterHomeWorkspace(defaultWorkspace);
            AddWorkspace(defaultWorkspace);
            CurrentWorkspace = defaultWorkspace;
        }

        /// <summary>
        ///     Add a new, visible Custom Node workspace to Dynamo
        /// </summary>
        /// <param name="workspace"></param>
        public void AddCustomNodeWorkspace(CustomNodeWorkspaceModel workspace)
        {
            AddWorkspace(workspace);
        }

        /// <summary>
        ///     Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            if (_workspaces.Remove(workspace))
            {
                if (workspace is HomeWorkspaceModel)
                    workspace.Dispose();
                OnWorkspaceRemoved(workspace);
            }
        }

        /// <summary>
        ///     Opens an existing custom node workspace.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool OpenCustomNodeWorkspace(Guid guid)
        {
            CustomNodeWorkspaceModel customNodeWorkspace;
            if (CustomNodeManager.TryGetFunctionWorkspace(guid, IsTestMode, out customNodeWorkspace))
            {
                if (!Workspaces.OfType<CustomNodeWorkspaceModel>().Contains(customNodeWorkspace))
                    AddWorkspace(customNodeWorkspace);
                CurrentWorkspace = customNodeWorkspace;
            }
            return false;
        }

        /// <summary>
        ///     Adds a node to the current workspace.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="centered"></param>
        public void AddNodeToCurrentWorkspace(NodeModel node, bool centered)
        {
            CurrentWorkspace.AddNode(node, centered);

            //TODO(Steve): This should be moved to WorkspaceModel.AddNode when all workspaces have their own selection -- MAGN-5707
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(node);

            //TODO(Steve): Make sure we're not missing something with TransformCoordinates. -- MAGN-5708
        }

        /// <summary>
        /// Copy selected ISelectable objects to the clipboard.
        /// </summary>
        public void Copy()
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
            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.ClearSelection();

            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            var nodeLookup = new Dictionary<Guid, NodeModel>();

            //make a list of all newly created models so that their
            //creations can be recorded in the undo recorder.
            var createdModels = new List<ModelBase>();

            var nodes = ClipBoard.OfType<NodeModel>();
            var connectors = ClipBoard.OfType<ConnectorModel>();
            var notes = ClipBoard.OfType<NoteModel>();

            var minX = Double.MaxValue;
            var minY = Double.MaxValue;

            // Create the new NoteModel's
            var newNoteModels = new List<NoteModel>();
            foreach (var note in notes)
            {
                newNoteModels.Add(new NoteModel(note.X, note.Y, note.Text, Guid.NewGuid()));

                minX = Math.Min(note.X, minX);
                minY = Math.Min(note.Y, minY);
            }

            var xmlDoc = new XmlDocument();

            // Create the new NodeModel's
            var newNodeModels = new List<NodeModel>();
            foreach (var node in nodes)
            {
                NodeModel newNode;

                if (CurrentWorkspace is HomeWorkspaceModel && (node is Symbol || node is Output))
                {
                    var symbol = (node is Symbol
                        ? (node as Symbol).InputSymbol
                        : (node as Output).Symbol);
                    var code = (string.IsNullOrEmpty(symbol) ? "x" : symbol) + ";";
                    newNode = new CodeBlockNodeModel(code, node.X, node.Y, LibraryServices);
                }
                else
                {
                    var dynEl = node.Serialize(xmlDoc, SaveContext.Copy);
                    newNode = NodeFactory.CreateNodeFromXml(dynEl, SaveContext.Copy);
                }

                newNode.ArgumentLacing = node.ArgumentLacing;
                if (!string.IsNullOrEmpty(node.NickName))
                    newNode.NickName = node.NickName;

                nodeLookup.Add(node.GUID, newNode);

                newNodeModels.Add( newNode );

                minX = Math.Min(node.X, minX);
                minY = Math.Min(node.Y, minY);
            }

            // Move all of the notes and nodes such that they are aligned with
            // the top left of the workspace
            var workspaceX = -CurrentWorkspace.X / CurrentWorkspace.Zoom;
            var workspaceY = -CurrentWorkspace.Y / CurrentWorkspace.Zoom;

            // Provide a small offset when pasting so duplicate pastes aren't directly on top of each other
            CurrentWorkspace.IncrementPasteOffset();

            var shiftX = workspaceX - minX + CurrentWorkspace.CurrentPasteOffset;
            var shiftY = workspaceY - minY + CurrentWorkspace.CurrentPasteOffset;

            foreach (var model in newNodeModels.Concat<ModelBase>(newNoteModels))
            {
                model.X = model.X + shiftX;
                model.Y = model.Y + shiftY;
            }

            // Add the new NodeModel's to the Workspace
            foreach (var newNode in newNodeModels)
            {
                CurrentWorkspace.AddNode(newNode, false);
                createdModels.Add(newNode);
                AddToSelection(newNode);
            }

            // TODO: is this required?
            OnRequestLayoutUpdate(this, EventArgs.Empty);

            // Add the new NoteModel's to the Workspace
            foreach (var newNote in newNoteModels)
            {
                CurrentWorkspace.AddNote(newNote, false);
                createdModels.Add(newNote);
                AddToSelection(newNote);
            }

            NodeModel start;
            NodeModel end;
            var newConnectors =
                from c in connectors

                // If the guid is in nodeLookup, then we connect to the new pasted node. Otherwise we
                // re-connect to the original.
                let startNode =
                    nodeLookup.TryGetValue(c.Start.Owner.GUID, out start)
                        ? start
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.Start.Owner.GUID)
                let endNode =
                    nodeLookup.TryGetValue(c.End.Owner.GUID, out end)
                        ? end
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.End.Owner.GUID)

                // Don't make a connector if either end is null.
                where startNode != null && endNode != null
                select
                    ConnectorModel.Make(startNode, endNode, c.Start.Index, c.End.Index);

            createdModels.AddRange(newConnectors);

            // Record models that are created as part of the command.
            CurrentWorkspace.RecordCreatedModels(createdModels);
        }

        /// <summary>
        ///     Add an ISelectable object to the selection.
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
        ///     Clear the workspace. Removes all nodes, notes, and connectors from the current workspace.
        /// </summary>
        public void ClearCurrentWorkspace()
        {
            OnWorkspaceClearing(this, EventArgs.Empty);

            CurrentWorkspace.Clear();

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

        private void AddZeroTouchNodesToSearch(IEnumerable<FunctionGroup> functionGroups)
        {
            foreach (var funcGroup in functionGroups)
                AddZeroTouchNodeToSearch(funcGroup);
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

        /// <summary>
        ///     Adds a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        private void AddWorkspace(WorkspaceModel workspace)
        {
            if (workspace == null) return;
            
            Action savedHandler = () => OnWorkspaceSaved(workspace);
            workspace.WorkspaceSaved += savedHandler;
            workspace.MessageLogged += LogMessage;
            workspace.PropertyChanged += OnWorkspacePropertyChanged;
            workspace.Disposed += () =>
            {
                workspace.WorkspaceSaved -= savedHandler;
                workspace.MessageLogged -= LogMessage;
                workspace.PropertyChanged -= OnWorkspacePropertyChanged;
            };

            _workspaces.Add(workspace);
            OnWorkspaceAdded(workspace);
        }
        enum ButtonId
        {
            Ok = 43420,
            Cancel,
            DownloadLatest,
            Proceed,
            Submit
        }

        /// <summary>
        /// Call this method to display a message box when a file of an older 
        /// version cannot be opened by the current version of Dynamo.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="fileVersion">Version of the input file.</param>
        /// <param name="currVersion">Current version of the Dynamo.</param>
        private void DisplayObsoleteFileMessage(string fullFilePath, Version fileVersion, Version currVersion)
        {
            var fileVer = ((fileVersion != null) ? fileVersion.ToString() : "Unknown");
            var currVer = ((currVersion != null) ? currVersion.ToString() : "Unknown");

            InstrumentationLogger.LogPiiInfo(
                "ObsoleteFileMessage",
                fullFilePath + " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            string summary = Properties.Resources.FileCannotBeOpened;
            var description =
                string.Format(
                    Properties.Resources.ObsoleteFileDescription,
                    fullFilePath,
                    fileVersion,
                    currVersion);

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_obsolete_file.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Properties.Resources.ObsoleteFileTitle,
                summary,
                description);

            args.AddRightAlignedButton((int)ButtonId.Ok, Properties.Resources.OKButton);

            OnRequestTaskDialog(null, args);
        }

        /// <summary>
        /// Call this method to display an error message in an event when live 
        /// runner throws an exception that is not handled anywhere else. This 
        /// message instructs user to save their work and restart Dynamo.
        /// </summary>
        /// <param name="exception">The exception to display.</param>
        private TaskDialogEventArgs DisplayEngineFailureMessage(Exception exception)
        {
            StabilityTracking.GetInstance().NotifyCrash();
            InstrumentationLogger.LogAnonymousEvent("EngineFailure", "Stability");

            if (exception != null)
            {
                InstrumentationLogger.LogException(exception);
            }

            string summary = Properties.Resources.UnhandledExceptionSummary;

            string description = (exception is HeapCorruptionException)
                ? exception.Message
                : @Properties.Resources.ExceptionIsNotHeapCorruptionDescription;

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_crash.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Properties.Resources.UnhandledExceptionTitle,
                summary,
                description);

            args.AddRightAlignedButton((int)ButtonId.Submit, Properties.Resources.SubmitBugButton);
            args.AddRightAlignedButton((int)ButtonId.Ok, Properties.Resources.ArggOKButton);
            args.Exception = exception;

            OnRequestTaskDialog(null, args);
            if (args.ClickedButtonId == (int)ButtonId.Submit)
                OnRequestBugReport();

            return args;
        }

        /// <summary>
        /// Displays file open error dialog if the file is of a future version than the currently installed version
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="fileVersion"></param>
        /// <param name="currVersion"></param>
        /// <returns> true if the file must be opened and false otherwise </returns>
        private bool DisplayFutureFileMessage(string fullFilePath, Version fileVersion, Version currVersion)
        {
            var fileVer = ((fileVersion != null) ? fileVersion.ToString() : Properties.Resources.UnknownVersion);
            var currVer = ((currVersion != null) ? currVersion.ToString() : Properties.Resources.UnknownVersion);

            InstrumentationLogger.LogPiiInfo("FutureFileMessage", fullFilePath +
                " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            string summary = Properties.Resources.FutureFileSummary;
            var description = string.Format(Properties.Resources.FutureFileDescription, fullFilePath, fileVersion, currVersion);

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Properties.Resources.FutureFileTitle, summary, description) { ClickedButtonId = (int)ButtonId.Cancel };

            args.AddRightAlignedButton((int)ButtonId.Cancel, Properties.Resources.CancelButton);
            args.AddRightAlignedButton((int)ButtonId.DownloadLatest, Properties.Resources.DownloadLatestButton);
            args.AddRightAlignedButton((int)ButtonId.Proceed, Properties.Resources.ProceedButton);

            OnRequestTaskDialog(null, args);
            if (args.ClickedButtonId == (int)ButtonId.DownloadLatest)
            {
                // this should be an event on DynamoModel
                OnRequestDownloadDynamo();
                return false;
            }

            return args.ClickedButtonId == (int)ButtonId.Proceed;
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "RunEnabled")
                OnPropertyChanged("RunEnabled");
        }

        #endregion
    }
}
