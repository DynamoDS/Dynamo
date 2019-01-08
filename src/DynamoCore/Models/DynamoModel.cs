using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Events;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Migration;
using Dynamo.Properties;
using Dynamo.Scheduler;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Updates;
using Dynamo.Utilities;
using DynamoServices;
using DynamoUnits;
using Greg;
using ProtoCore;
using ProtoCore.Runtime;
using Compiler = ProtoAssociative.Compiler;
// Dynamo package manager
using DefaultUpdateManager = Dynamo.Updates.UpdateManager;
using FunctionGroup = Dynamo.Engine.FunctionGroup;
using Utils = Dynamo.Graph.Nodes.Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Dynamo.Models
{
    /// <summary>
    /// This class contains the extra Dynamo-specific preferences data
    /// </summary>
    public class DynamoPreferencesData
    {
        public double ScaleFactor { get; internal set; }
        public bool HasRunWithoutCrash { get; internal set; }
        public bool IsVisibleInDynamoLibrary { get; internal set; }
        public string Version { get; internal set; }
        public string RunType { get; internal set; }
        public string RunPeriod { get; internal set; }

        public DynamoPreferencesData(
          double scaleFactor,
          bool hasRunWithoutCrash,
          bool isVisibleInDynamoLibrary,
          string version,
          string runType,
          string runPeriod)
        {
            ScaleFactor = scaleFactor;
            HasRunWithoutCrash = hasRunWithoutCrash;
            IsVisibleInDynamoLibrary = isVisibleInDynamoLibrary;
            Version = version;
            RunType = runType;
            RunPeriod = runPeriod;
        }

        public static DynamoPreferencesData Default()
        {
            return new DynamoPreferencesData(
              1.0,
              true,
              true,
              AssemblyHelper.GetDynamoVersion().ToString(),
              Models.RunType.Automatic.ToString(),
              RunSettings.DefaultRunPeriod.ToString());
        }
    }

    /// <summary>
    /// This class creates an interface for Engine controller.
    /// </summary>
    public interface IEngineControllerManager
    {
        /// <summary>
        /// A controller to coordinate the interactions between some DesignScript
        /// sub components like library management, live runner and so on.
        /// </summary>
        EngineController EngineController { get; }
    }

    /// <summary>
    /// The core model of Dynamo.
    /// </summary>
    public partial class DynamoModel : IDynamoModel, IDisposable, IEngineControllerManager, ITraceReconciliationProcessor
    {
        #region private members

        private readonly string geometryFactoryPath;
        private readonly PathManager pathManager;
        private WorkspaceModel currentWorkspace;
        private Timer backupFilesTimer;
        private Dictionary<Guid, string> backupFilesDict = new Dictionary<Guid, string>();
        internal readonly Stopwatch stopwatch = Stopwatch.StartNew();
        #endregion

        #region events

        internal delegate void FunctionNamePromptRequestHandler(object sender, FunctionNamePromptEventArgs e);
        internal event FunctionNamePromptRequestHandler RequestsFunctionNamePrompt;
        internal void OnRequestsFunctionNamePrompt(Object sender, FunctionNamePromptEventArgs e)
        {
            if (RequestsFunctionNamePrompt != null)
                RequestsFunctionNamePrompt(this, e);
        }

        internal event Action<PresetsNamePromptEventArgs> RequestPresetsNamePrompt;
        internal void OnRequestPresetNamePrompt(PresetsNamePromptEventArgs e)
        {
            if (RequestPresetsNamePrompt != null)
                RequestPresetsNamePrompt(e);
        }

        /// <summary>
        /// Occurs when a workspace is saved to a file.
        /// </summary>
        public event WorkspaceHandler WorkspaceSaved;
        internal void OnWorkspaceSaved(WorkspaceModel workspace)
        {
            if (WorkspaceSaved != null)
            {
                WorkspaceSaved(workspace);
            }
        }

        /// <summary>
        /// Occurs when a workspace is scheduled to be saved to a backup file.
        /// </summary>
        public event Action<string, bool> RequestWorkspaceBackUpSave;
        internal void OnRequestWorkspaceBackUpSave(string path, bool isBackUp)
        {
            if (RequestWorkspaceBackUpSave != null)
            {
                RequestWorkspaceBackUpSave(path, isBackUp);
            }
        }

        /// <summary>
        /// Event that is fired during the opening of the workspace.
        ///
        /// Use the XmlDocument object provided to conduct additional
        /// workspace opening operations.
        /// </summary>
        public event Action<object> WorkspaceOpening;
        internal void OnWorkspaceOpening(object obj)
        {
            var handler = WorkspaceOpening;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// This event is raised right before the shutdown of DynamoModel started.
        /// When this event is raised, the shutdown is guaranteed to take place
        /// (i.e. user has had a chance to save the work and decided to proceed
        /// with shutting down Dynamo). Handlers of this event can still safely
        /// access the DynamoModel, the WorkspaceModel (along with its contents),
        /// and the DynamoScheduler.
        /// </summary>
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
        public event DynamoModelHandler ShutdownCompleted;

        private void OnShutdownCompleted()
        {
            if (ShutdownCompleted != null)
                ShutdownCompleted(this);
        }

         /// <summary>
         /// This event is raised when Dynamo is ready for user interaction.
         /// </summary>
         public event Action<ReadyParams> DynamoReady;
         private bool dynamoReady;

        #endregion

        #region static properties

        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        public static bool IsTestMode { get; set; }

        /// <summary>
        /// Flag to indicate that there is no UI on this process, and things
        /// like the update manager and the analytics collection should be
        /// disabled.
        /// </summary>
        public static bool IsHeadless { get; set; }

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
            get { return DefaultUpdateManager.GetProductVersion().ToString(); }
        }

        /// <summary>
        /// Current Version of the Host (i.e. DynamoRevit/DynamoStudio)
        /// </summary>
        public string HostVersion { get; set; }

        /// <summary>
        /// Name of the Host (i.e. DynamoRevit/DynamoStudio)
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// UpdateManager to handle automatic upgrade to higher version.
        /// </summary>
        public IUpdateManager UpdateManager { get; private set; }

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
        ///     Manages all extensions for Dynamo
        /// </summary>
        public IExtensionManager ExtensionManager { get { return extensionManager; } }

        private readonly ExtensionManager extensionManager;

        /// <summary>
        ///     Manages all loaded NodeModel libraries.
        /// </summary>
        public readonly NodeModelAssemblyLoader Loader;

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
                    + DefaultUpdateManager.GetProductVersion();
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

        /// <summary>
        ///     Returns collection of visible workspaces in Dynamo
        /// </summary>
        public IEnumerable<WorkspaceModel> Workspaces
        {
            get { return _workspaces; }
        }

        /// <summary>
        /// An object which implements the ITraceReconciliationProcessor interface,
        /// and is used for handlling the results of a trace reconciliation.
        /// </summary>
        public ITraceReconciliationProcessor TraceReconciliationProcessor { get; set; }

        /// <summary>
        /// Returns authentication manager object for oxygen authentication.
        /// </summary>
        public AuthenticationManager AuthenticationManager { get; set; }

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

            AnalyticsService.ShutDown();

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
            string DynamoHostPath { get; set; }
            IPreferences Preferences { get; set; }
            IPathResolver PathResolver { get; set; }
            bool StartInTestMode { get; set; }
            IUpdateManager UpdateManager { get; set; }
            ISchedulerThread SchedulerThread { get; set; }
            string GeometryFactoryPath { get; set; }
            IAuthProvider AuthProvider { get; set; }
            IEnumerable<IExtension> Extensions { get; set; }
            TaskProcessMode ProcessMode { get; set; }

            /// <summary>
            /// If true, the program does not have a UI.
            /// No update checks or analytics collection should be done.
            /// </summary>
            bool IsHeadless { get; set; }
        }

        /// <summary>
        /// Initialization settings for DynamoModel.
        /// </summary>
        public struct DefaultStartConfiguration : IStartConfiguration
        {
            public string Context { get; set; }
            public string DynamoCorePath { get; set; }
            public string DynamoHostPath { get; set; }
            public IPreferences Preferences { get; set; }
            public IPathResolver PathResolver { get; set; }
            public bool StartInTestMode { get; set; }
            public IUpdateManager UpdateManager { get; set; }
            public ISchedulerThread SchedulerThread { get; set; }
            public string GeometryFactoryPath { get; set; }
            public IAuthProvider AuthProvider { get; set; }
            public IEnumerable<IExtension> Extensions { get; set; }
            public TaskProcessMode ProcessMode { get; set; }
            public bool IsHeadless { get; set; }
        }

        /// <summary>
        ///     Start DynamoModel with all default configuration options
        /// </summary>
        /// <returns>The instance of <see cref="DynamoModel"/></returns>
        public static DynamoModel Start()
        {
            return Start(new DefaultStartConfiguration() { ProcessMode = TaskProcessMode.Asynchronous });
        }

        /// <summary>
        /// Start DynamoModel with custom configuration.  Defaults will be assigned not provided.
        /// </summary>
        /// <param name="configuration">Start configuration</param>
        /// <returns>The instance of <see cref="DynamoModel"/></returns>
        public static DynamoModel Start(IStartConfiguration configuration)
        {
            // where necessary, assign defaults
            if (string.IsNullOrEmpty(configuration.Context))
                configuration.Context = Configuration.Context.NONE;

            return new DynamoModel(configuration);
        }

        /// <summary>
        /// Default constructor for DynamoModel
        /// </summary>
        /// <param name="config">Start configuration</param>
        protected DynamoModel(IStartConfiguration config)
        {
            ClipBoard = new ObservableCollection<ModelBase>();

            pathManager = new PathManager(new PathManagerParams
            {
                CorePath = config.DynamoCorePath,
                HostPath = config.DynamoHostPath,
                PathResolver = config.PathResolver
            });

            // Ensure we have all directories in place.
            var exceptions = new List<Exception>();
            pathManager.EnsureDirectoryExistence(exceptions);

            Context = config.Context;
            IsTestMode = config.StartInTestMode;
            IsHeadless = config.IsHeadless;

            DebugSettings = new DebugSettings();
            Logger = new DynamoLogger(DebugSettings, pathManager.LogDirectory);

            foreach (var exception in exceptions)
            {
                Logger.Log(exception); // Log all exceptions.
            }

            MigrationManager = new MigrationManager(DisplayFutureFileMessage, DisplayObsoleteFileMessage);
            MigrationManager.MessageLogged += LogMessage;
            MigrationManager.MigrationTargets.Add(typeof(WorkspaceMigrations));

            var thread = config.SchedulerThread ?? new DynamoSchedulerThread();
            Scheduler = new DynamoScheduler(thread, config.ProcessMode);
            Scheduler.TaskStateChanged += OnAsyncTaskStateChanged;

            geometryFactoryPath = config.GeometryFactoryPath;

            IPreferences preferences = CreateOrLoadPreferences(config.Preferences);
            var settings = preferences as PreferenceSettings;
            if (settings != null)
            {
                PreferenceSettings = settings;
                PreferenceSettings.PropertyChanged += PreferenceSettings_PropertyChanged;
            }

            InitializeInstrumentationLogger();

            if (!IsTestMode && PreferenceSettings.IsFirstRun)
            {
                DynamoMigratorBase migrator = null;

                try
                {
                    var dynamoLookup = config.UpdateManager != null && config.UpdateManager.Configuration != null
                        ? config.UpdateManager.Configuration.DynamoLookUp : null;

                    migrator = DynamoMigratorBase.MigrateBetweenDynamoVersions(pathManager, dynamoLookup);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }

                if (migrator != null)
                {
                    var isFirstRun = PreferenceSettings.IsFirstRun;
                    PreferenceSettings = migrator.PreferenceSettings;

                    // Preserve the preference settings for IsFirstRun as this needs to be set
                    // only by UsageReportingManager
                    PreferenceSettings.IsFirstRun = isFirstRun;
                }
            }
            InitializePreferences(PreferenceSettings);

            // At this point, pathManager.PackageDirectories only has 1 element which is the directory
            // in AppData. If list of PackageFolders is empty, add the folder in AppData to the list since there
            // is no additional location specified. Otherwise, update pathManager.PackageDirectories to include
            // PackageFolders
            if (PreferenceSettings.CustomPackageFolders.Count == 0)
                PreferenceSettings.CustomPackageFolders = new List<string> { pathManager.UserDataDirectory };

            // Make sure that the default package folder is added in the list if custom packages folder.
            var userDataFolder = pathManager.GetUserDataFolder(); // Get the default user data path
            AddPackagePath(userDataFolder);

            // Check if the Python template file specified in the settings file exists & it's not empty
            // If not, check the default filepath for the Python template (userDataFolder\PythonTemplate.py).
            // If that doesn't exist either or is empty, Dynamo falls back to the hard-coded Python script template.
            // We log the behaviour to make it easy for users to troubleshoot this.
            if (string.IsNullOrEmpty(PreferenceSettings.PythonTemplateFilePath) || !File.Exists(PreferenceSettings.PythonTemplateFilePath))
            {
                if (string.IsNullOrEmpty(pathManager.PythonTemplateFilePath) || !File.Exists(pathManager.PythonTemplateFilePath))
                    Logger.Log(Resources.PythonTemplateInvalid);
                else
                {
                    PreferenceSettings.PythonTemplateFilePath = pathManager.PythonTemplateFilePath;
                    Logger.Log(Resources.PythonTemplateDefaultFile + " : " + pathManager.PythonTemplateFilePath);
                }
            }
            else Logger.Log(Resources.PythonTemplateUserFile + " : " + pathManager.PythonTemplateFilePath);

            pathManager.Preferences = PreferenceSettings;

            SearchModel = new NodeSearchModel(Logger);
            SearchModel.ItemProduced +=
                node => ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, true));

            NodeFactory = new NodeFactory();
            NodeFactory.MessageLogged += LogMessage;

            extensionManager = new ExtensionManager();
            extensionManager.MessageLogged += LogMessage;
            var extensions = config.Extensions ?? LoadExtensions();

            // when dynamo is ready, alert the loaded extensions
            DynamoReady += (readyParams) =>
            {
                this.dynamoReady = true;
                DynamoReadyExtensionHandler(readyParams, ExtensionManager.Extensions);
            };

            // when an extension is added if dynamo is ready, alert that extension (this alerts late
            // loaded extensions)
            ExtensionManager.ExtensionAdded += (extension) =>
            {
                if (this.dynamoReady)
                {
                    DynamoReadyExtensionHandler(new ReadyParams(this),
                    new List<IExtension>() { extension });
                };
            };

            Loader = new NodeModelAssemblyLoader();
            Loader.MessageLogged += LogMessage;

            // Create a core which is used for parsing code and loading libraries
            var libraryCore = new ProtoCore.Core(new Options());

            libraryCore.Compilers.Add(Language.Associative, new Compiler(libraryCore));
            libraryCore.Compilers.Add(Language.Imperative, new ProtoImperative.Compiler(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            LibraryServices = new LibraryServices(libraryCore, pathManager, PreferenceSettings);
            LibraryServices.MessageLogged += LogMessage;
            LibraryServices.LibraryLoaded += LibraryLoaded;

            CustomNodeManager = new CustomNodeManager(NodeFactory, MigrationManager, LibraryServices);
            InitializeCustomNodeManager();

            ResetEngineInternal();

            AddHomeWorkspace();

            AuthenticationManager = new AuthenticationManager(config.AuthProvider);

            UpdateManager = config.UpdateManager ?? new DefaultUpdateManager(null);

            var hostUpdateManager = config.UpdateManager;

            if (hostUpdateManager != null)
            {
                HostName = hostUpdateManager.HostName;
                HostVersion = hostUpdateManager.HostVersion == null ? null : hostUpdateManager.HostVersion.ToString();
            }
            else
            {
                HostName = string.Empty;
                HostVersion = null;
            }

            UpdateManager.Log += UpdateManager_Log;
            if (!IsTestMode && !IsHeadless)
            {
                DefaultUpdateManager.CheckForProductUpdate(UpdateManager);
            }

            Logger.Log(string.Format("Dynamo -- Build {0}",
                                        Assembly.GetExecutingAssembly().GetName().Version));

            InitializeNodeLibrary(PreferenceSettings);

            if (extensions.Any())
            {
                var startupParams = new StartupParams(config.AuthProvider,
                    pathManager, new ExtensionLibraryLoader(this), CustomNodeManager,
                    GetType().Assembly.GetName().Version, PreferenceSettings);

                foreach (var ext in extensions)
                {
                    var logSource = ext as ILogSource;
                    if (logSource != null)
                        logSource.MessageLogged += LogMessage;

                    try
                    {
                        ext.Startup(startupParams);
                        // if we are starting extension (A) which is a source of other extensions (like packageManager)
                        // then we can start the extension(s) (B) that it requested be loaded.
                        if(ext is IExtensionSource)
                        {
                           foreach(var loadedExtension in((ext as IExtensionSource).RequestedExtensions))
                            {
                                if(loadedExtension is IExtension)
                                {
                                    (loadedExtension as IExtension).Startup(startupParams);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                    }

                    ExtensionManager.Add(ext);

                }
            }

            LogWarningMessageEvents.LogWarningMessage += LogWarningMessage;

            StartBackupFilesTimer();

            TraceReconciliationProcessor = this;
            // This event should only be raised at the end of this method.
             DynamoReady(new ReadyParams(this));
        }

        private void DynamoReadyExtensionHandler(ReadyParams readyParams, IEnumerable<IExtension> extensions) {

            foreach (var ext in extensions)
            {
                try
                {
                    ext.Ready(readyParams);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds a new path to the list of custom package folders, but only if the path
        /// does not already exist in the list.
        /// </summary>
        /// <param name="path"> The path to add.</param>
        /// <param name="file"> The file to add when importing a library.</param>
        public bool AddPackagePath(string path, string file = "")
        {
            if (!Directory.Exists(path))
                return false;

            string fullFilename = path;
            if (file != "")
            {
                fullFilename = Path.Combine(path, file);
                if (!File.Exists(fullFilename))
                    return false;
            }

            if (PreferenceSettings.CustomPackageFolders.Contains(fullFilename))
                return false;

            PreferenceSettings.CustomPackageFolders.Add(fullFilename);

            return true;
        }

        private IEnumerable<IExtension> LoadExtensions()
        {
            var extensions = new List<IExtension>();
            foreach (var dir in pathManager.ExtensionsDirectories)
            {
                extensions.AddRange(ExtensionManager.ExtensionLoader.LoadDirectory(dir));
            }
            return extensions;
        }

        private void RemoveExtension(IExtension ext)
        {
            ExtensionManager.Remove(ext);

            var logSource = ext as ILogSource;
            if (logSource != null)
                logSource.MessageLogged -= LogMessage;
        }

        private void EngineController_TraceReconcliationComplete(TraceReconciliationEventArgs obj)
        {
            Debug.WriteLine("TRACE RECONCILIATION: {0} total serializables were orphaned.", obj.CallsiteToOrphanMap.SelectMany(kvp => kvp.Value).Count());

            // The orphans will come back here as a dictionary of lists of ISerializables jeyed by their callsite id.
            // This dictionary gets redistributed into a dictionary keyed by the workspace id.

            var workspaceOrphanMap = new Dictionary<Guid, List<ISerializable>>();

            foreach (var ws in Workspaces.OfType<HomeWorkspaceModel>())
            {
                // Get the orphaned serializables to this workspace
                var wsOrphans = ws.GetOrphanedSerializablesAndClearHistoricalTraceData().ToList();

                if (!wsOrphans.Any())
                    continue;

                if (!workspaceOrphanMap.ContainsKey(ws.Guid))
                {
                    workspaceOrphanMap.Add(ws.Guid, wsOrphans);
                }
                else
                {
                    workspaceOrphanMap[ws.Guid].AddRange(wsOrphans);
                }
            }

            foreach (var kvp in obj.CallsiteToOrphanMap)
            {
                if (!kvp.Value.Any()) continue;

                var nodeGuid = EngineController.LiveRunnerRuntimeCore.RuntimeData.CallSiteToNodeMap[kvp.Key];

                // TODO: MAGN-7314
                // Find the owning workspace for a node.
                var nodeSpace =
                    Workspaces.FirstOrDefault(
                        ws =>
                            ws.Nodes.FirstOrDefault(n => n.GUID == nodeGuid)
                                != null);

                if (nodeSpace == null) continue;

                // Add the node's orphaned serializables to the workspace
                // orphan map.
                if (workspaceOrphanMap.ContainsKey(nodeSpace.Guid))
                {
                    workspaceOrphanMap[nodeSpace.Guid].AddRange(kvp.Value);
                }
                else
                {
                    workspaceOrphanMap.Add(nodeSpace.Guid, kvp.Value);
                }
            }

            TraceReconciliationProcessor.PostTraceReconciliation(workspaceOrphanMap);
        }

        /// <summary>
        /// Deals with orphaned serializables.
        /// </summary>
        /// <param name="orphanedSerializables">Collection of orphaned serializables.</param>
        public virtual void PostTraceReconciliation(Dictionary<Guid, List<ISerializable>> orphanedSerializables)
        {
            // Override in derived classes to deal with orphaned serializables.
        }

        void UpdateManager_Log(LogEventArgs args)
        {
            Logger.Log(args.Message, args.Level);
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
            var updateTask = e.Task as UpdateGraphAsyncTask;
            switch (e.CurrentState)
            {
                case TaskStateChangedEventArgs.State.ExecutionStarting:
                    if (updateTask != null)
                        ExecutionEvents.OnGraphPreExecution(new ExecutionSession(updateTask, this, geometryFactoryPath));
                    break;

                case TaskStateChangedEventArgs.State.ExecutionCompleted:
                    if (updateTask != null)
                    {
                        // Record execution time for update graph task.
                        long start = e.Task.ExecutionStartTime.TickCount;
                        long end = e.Task.ExecutionEndTime.TickCount;
                        var executionTimeSpan = new TimeSpan(end - start);

                        if (Logging.Analytics.ReportingAnalytics)
                        {
                            var modifiedNodes = "";
                            if (updateTask.ModifiedNodes != null && updateTask.ModifiedNodes.Any())
                            {
                                modifiedNodes = updateTask.ModifiedNodes
                                    .Select(n => n.GetOriginalName())
                                    .Aggregate((x, y) => string.Format("{0}, {1}", x, y));
                            }

                            Dynamo.Logging.Analytics.TrackTimedEvent(
                                Categories.Performance,
                                e.Task.GetType().Name,
                                executionTimeSpan, modifiedNodes);
                        }

                        Debug.WriteLine(String.Format(Resources.EvaluationCompleted, executionTimeSpan));

                        ExecutionEvents.OnGraphPostExecution(new ExecutionSession(updateTask, this, geometryFactoryPath));
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
            EngineController.TraceReconcliationComplete -= EngineController_TraceReconcliationComplete;

            ExtensionManager.Dispose();
            extensionManager.MessageLogged -= LogMessage;

            LibraryServices.Dispose();
            LibraryServices.LibraryManagementCore.Cleanup();

            UpdateManager.Log -= UpdateManager_Log;
            Logger.Dispose();

            EngineController.Dispose();
            EngineController = null;

            if (backupFilesTimer != null)
            {
                backupFilesTimer.Dispose();
                backupFilesTimer = null;
                Logger.Log("Backup files timer is disposed");
            }

            if (PreferenceSettings != null)
            {
                PreferenceSettings.PropertyChanged -= PreferenceSettings_PropertyChanged;
            }

            LogWarningMessageEvents.LogWarningMessage -= LogWarningMessage;
            foreach (var ws in _workspaces)
            {
                ws.Dispose();
            }
        }

        private void InitializeCustomNodeManager()
        {
            CustomNodeManager.MessageLogged += LogMessage;

            var customNodeSearchRegistry = new HashSet<Guid>();
            CustomNodeManager.InfoUpdated += info =>
            {
                if (customNodeSearchRegistry.Contains(info.FunctionId)
                        || !info.IsVisibleInDynamoLibrary)
                    return;

                var elements = SearchModel.SearchEntries.OfType<CustomNodeSearchElement>().
                                Where(x =>
                                        {
                                            // Search for common paths and get rid of empty paths.
                                            // It can be empty just in case it's just created node.
                                            return String.Compare(x.Path, info.Path, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                !String.IsNullOrEmpty(x.Path);
                                        }).ToList();

                if (elements.Any())
                {
                    foreach (var element in elements)
                    {
                        element.SyncWithCustomNodeInfo(info);
                        SearchModel.Update(element);
                    }
                    return;
                }

                customNodeSearchRegistry.Add(info.FunctionId);
                var searchElement = new CustomNodeSearchElement(CustomNodeManager, info);
                SearchModel.Add(searchElement);
                Action<CustomNodeInfo> infoUpdatedHandler = null;
                infoUpdatedHandler = newInfo =>
                {
                    if (info.FunctionId == newInfo.FunctionId)
                    {
                        bool isCategoryChanged = searchElement.FullCategoryName != newInfo.Category;
                        searchElement.SyncWithCustomNodeInfo(newInfo);
                        SearchModel.Update(searchElement, isCategoryChanged);
                    }
                };
                CustomNodeManager.InfoUpdated += infoUpdatedHandler;
                CustomNodeManager.CustomNodeRemoved += id =>
                {
                    CustomNodeManager.InfoUpdated -= infoUpdatedHandler;
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
            CustomNodeManager.DefinitionUpdated += UpdateCustomNodeDefinition;
        }

        private void InitializeIncludedNodes()
        {
            var customNodeData = new TypeLoadData(typeof(Function));
            NodeFactory.AddLoader(new CustomNodeLoader(CustomNodeManager, IsTestMode));
            NodeFactory.AddAlsoKnownAs(customNodeData.Type, customNodeData.AlsoKnownAs);

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

            var inputLoader = new InputNodeLoader();
            NodeFactory.AddLoader(symbolData.Type, inputLoader);
            NodeFactory.AddFactory(symbolData.Type, inputLoader);
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
            if (!IsTestMode)
                AddZeroTouchNodesToSearch(functionGroups);
#if DEBUG_LIBRARY
            DumpLibrarySnapshot(functionGroups);
#endif

            // Load local custom nodes and locally imported libraries
            foreach (var path in pathManager.DefinitionDirectories)
            {
                // NOTE: extension will only be null if path is null
                string extension = null;
                try
                {
                    extension = Path.GetExtension(path);
                }
                catch (ArgumentException e)
                {
                    Logger.Log(e.Message);
                }
                if (extension == null)
                    continue;

                // If the path has a .dll or .ds extension it is an explicitly imported library
                if (extension == ".dll" || extension == ".ds")
                {
                    // If a library was explicitly loaded by using the "File | ImportLibrary..." command
                    // and for some reason the import fails we do not want to throw an exception
                    LibraryServices.ImportLibrary(path, true);
                    continue;
                }

                // Otherwise it is a custom node
                CustomNodeManager.AddUninitializedCustomNodesInPath(path, IsTestMode);
            }

            CustomNodeManager.AddUninitializedCustomNodesInPath(pathManager.CommonDefinitions, IsTestMode);
        }

        internal void LoadNodeLibrary(Assembly assem)
        {
            if (!NodeModelAssemblyLoader.ContainsNodeModelSubType(assem))
            {
                LibraryServices.ImportLibrary(assem.Location);
                return;
            }

            var nodes = new List<TypeLoadData>();
            Loader.LoadNodesFromAssembly(assem, Context, nodes, new List<TypeLoadData>());

            foreach (var type in nodes)
            {
                // Protect ourselves from exceptions thrown by malformed third party nodes.
                try
                {
                    NodeFactory.AddTypeFactoryAndLoader(type.Type);
                    NodeFactory.AddAlsoKnownAs(type.Type, type.AlsoKnownAs);
                    type.IsPackageMember = true;
                    AddNodeTypeToSearch(type);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }
        }

        private void InitializeInstrumentationLogger()
        {
            if (!IsTestMode && !IsHeadless)
            {
                AnalyticsService.Start(this);
            }
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

            var settings = preferences as PreferenceSettings;
            if (settings != null)
            {
                settings.InitializeNamespacesToExcludeFromLibrary();
            }
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

        /// <summary>
        /// This warning message is displayed on the node associated with the FFI dll
        /// </summary>
        /// <param name="args"></param>
        private void LogWarningMessage(LogWarningMessageEventArgs args)
        {
            Validity.Assert(EngineController.LiveRunnerRuntimeCore != null);
            EngineController.LiveRunnerRuntimeCore.RuntimeStatus.LogWarning(WarningID.Default, args.message);
        }

        #endregion

        #region engine management

        /// <summary>
        ///     Register custom node defintion and execute all custom node
        ///     instances.
        /// </summary>
        /// <param name="definition"></param>
        private void UpdateCustomNodeDefinition(CustomNodeDefinition definition)
        {
            RegisterCustomNodeDefinitionWithEngine(definition);
            MarkAllDependenciesAsModified(definition);
        }

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
        }

        /// <summary>
        /// Returns all function instances directly or indirectly depends on the
        /// specified function definition and mark them as modified so that
        /// their values will be re-queried.
        /// </summary>
        /// <param name="def"></param>
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
        /// </summary>
        /// <param name="markNodesAsDirty">Set this parameter to true to force
        /// reset of the execution substrate. Note that setting this parameter
        /// to true will have a negative performance impact.</param>
        public virtual void ResetEngine(bool markNodesAsDirty = false)
        {
            // TODO(Luke): Push this into a resync call with the engine controller
            //
            // Tracked in MAGN-5167.
            // As some async tasks use engine controller, for example
            // CompileCustomNodeAsyncTask and UpdateGraphAsyncTask, it is possible
            // that engine controller is reset *before* tasks get executed. For
            // example, opening custom node will schedule a CompileCustomNodeAsyncTask
            // firstly and then reset engine controller.
            //
            // We should make sure engine controller is reset after all tasks that
            // depend on it get executed, or those tasks are thrown away if safe to
            // do that.

            ResetEngineInternal();
            foreach (var workspaceModel in Workspaces.OfType<HomeWorkspaceModel>())
            {
                workspaceModel.ResetEngine(EngineController, markNodesAsDirty);
            }
        }

        protected void ResetEngineInternal()
        {
            if (EngineController != null)
            {
                EngineController.TraceReconcliationComplete -= EngineController_TraceReconcliationComplete;
                EngineController.MessageLogged -= LogMessage;
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(
                LibraryServices,
                geometryFactoryPath,
                DebugSettings.VerboseLogging);

            EngineController.MessageLogged += LogMessage;
            EngineController.TraceReconcliationComplete += EngineController_TraceReconcliationComplete;

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
        /// Opens a Dynamo workspace from a path to a file on disk.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="forceManualExecutionMode">Set this to true to discard
        /// execution mode specified in the file and set manual mode</param>
        public void OpenFileFromPath(string filePath, bool forceManualExecutionMode = false)
        {
            XmlDocument xmlDoc;
            Exception ex;
            if (DynamoUtilities.PathHelper.isValidXML(filePath, out xmlDoc, out ex))
            {
                OpenXmlFileFromPath(xmlDoc, filePath, forceManualExecutionMode);
                return;
            }
            else
            {
                // These kind of exceptions indicate that file is not accessible 
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    throw ex;
                }
                if (ex is System.Xml.XmlException)
                {
                    // XML opening failure can indicate that this file is corrupted XML or Json
                    string fileContents;

                    if (DynamoUtilities.PathHelper.isValidJson(filePath, out fileContents, out ex))
                    {
                        OpenJsonFileFromPath(fileContents, filePath, forceManualExecutionMode);
                        return;
                    }
                    else
                    {
                        // When Json opening also failed, either this file is corrupted or there
                        // are other kind of failures related to Json de-serialization
                        throw ex;
                    }
                }
            }
        }

        static private DynamoPreferencesData DynamoPreferencesDataFromJson(string json)
        {
            JsonReader reader = new JsonTextReader(new StringReader(json));
            var obj = JObject.Load(reader);
            var viewBlock = obj["View"];
            var dynamoBlock = viewBlock == null ? null : viewBlock["Dynamo"];
            if (dynamoBlock == null)
                return DynamoPreferencesData.Default();

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                Culture = CultureInfo.InvariantCulture
            };

            return JsonConvert.DeserializeObject<DynamoPreferencesData>(dynamoBlock.ToString(), settings);
        }

        /// <summary>
        /// Opens a Dynamo workspace from a path to an JSON file on disk.
        /// </summary>
        /// <param name="fileContents">Json file contents</param>
        /// <param name="filePath">Path to file</param>
        /// <param name="forceManualExecutionMode">Set this to true to discard
        /// execution mode specified in the file and set manual mode</param>
        /// <returns>True if workspace was opened successfully</returns>
        private bool OpenJsonFileFromPath(string fileContents, string filePath, bool forceManualExecutionMode)
        {
            try
            {
                DynamoPreferencesData dynamoPreferences = DynamoPreferencesDataFromJson(fileContents);
                if (dynamoPreferences != null)
                {
                    // TODO, QNTM-1101: Figure out JSON migration strategy
                    if (true) //MigrationManager.ProcessWorkspace(dynamoPreferences.Version, xmlDoc, IsTestMode, NodeFactory))
                    {
                        WorkspaceModel ws;
                        if (OpenJsonFile(filePath, fileContents, dynamoPreferences, forceManualExecutionMode, out ws))
                        {
                            OpenWorkspace(ws);
                            //Raise an event to deserialize the view parameters before
                            //setting the graph to run
                            OnComputeModelDeserialized();

                            SetPeriodicEvaluation(ws);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Opens a Dynamo workspace from a path to an Xml file on disk.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="forceManualExecutionMode">Set this to true to discard
        /// execution mode specified in the file and set manual mode</param>
        /// <returns>True if workspace was opened successfully</returns>
        private bool OpenXmlFileFromPath(XmlDocument xmlDoc, string filePath, bool forceManualExecutionMode)
        {
            try
            {
                //save the file before it is migrated to JSON.
                //if in test mode, don't save the file in backup
                if (!IsTestMode)
                {
                    if (!pathManager.BackupXMLFile(xmlDoc, filePath))
                    {
                        Logger.Log("File is not saved in the backup folder {0}: ", pathManager.BackupDirectory);
                    }
                }

                WorkspaceInfo workspaceInfo;
                if (WorkspaceInfo.FromXmlDocument(xmlDoc, filePath, IsTestMode, forceManualExecutionMode, Logger, out workspaceInfo))
                {
                    if (MigrationManager.ProcessWorkspace(workspaceInfo, xmlDoc, IsTestMode, NodeFactory))
                    {
                        WorkspaceModel ws;
                        if (OpenXmlFile(workspaceInfo, xmlDoc, out ws))
                        {
                            OpenWorkspace(ws);

                            // Set up workspace cameras here
                            OnWorkspaceOpening(xmlDoc);
                            SetPeriodicEvaluation(ws);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OpenWorkspace(WorkspaceModel ws)
        {
            // TODO: #4258
            // The logic to remove all other home workspaces from the model
            // was moved from the ViewModel. When #4258 is implemented, we will need to
            // remove this step.
            var currentHomeSpaces = Workspaces.OfType<HomeWorkspaceModel>().ToList();
            if (currentHomeSpaces.Any())
            {
                // If the workspace we're opening is a home workspace,
                // then remove all the other home workspaces. Otherwise,
                // Remove all but the first home workspace.
                var end = ws is HomeWorkspaceModel ? 0 : 1;

                for (var i = currentHomeSpaces.Count - 1; i >= end; i--)
                {
                    RemoveWorkspace(currentHomeSpaces[i]);
                }
            }

            AddWorkspace(ws);
            CurrentWorkspace = ws;
        }

        private void SetPeriodicEvaluation(WorkspaceModel ws)
        {
            // TODO: #4258
            // The following logic to start periodic evaluation will need to be moved
            // inside of the HomeWorkspaceModel's constructor.  It cannot be there today
            // as it causes an immediate crash due to the above ResetEngine call.
            var hws = ws as HomeWorkspaceModel;
            if (hws != null)
            {
                // TODO: #4258
                // Remove this ResetEngine call when multiple home workspaces is supported.
                // This call formerly lived in DynamoViewModel
                ResetEngine();

                if (hws.RunSettings.RunType == RunType.Periodic)
                {
                    hws.StartPeriodicEvaluation();
                }
            }
        }

        private bool OpenJsonFile(
          string filePath,
          string fileContents,
          DynamoPreferencesData dynamoPreferences,
          bool forceManualExecutionMode,
          out WorkspaceModel workspace)
        {
            CustomNodeManager.AddUninitializedCustomNodesInPath(
                Path.GetDirectoryName(filePath),
                IsTestMode);

            // TODO, QNTM-1108: WorkspaceModel.FromJson does not check a schema and so will not fail as long
            // as the fileContents are valid JSON, regardless of if all required data is present or not
            workspace = WorkspaceModel.FromJson(
                fileContents,
                LibraryServices,
                EngineController,
                Scheduler,
                NodeFactory,
                IsTestMode,
                false,
                CustomNodeManager);

            workspace.FileName = filePath;
            workspace.ScaleFactor = dynamoPreferences.ScaleFactor;

            // NOTE: This is to handle the case of opening a JSON file that does not have a version string
            //       This logic may not be correct, need to decide the importance of versioning early JSON files
            string versionString = dynamoPreferences.Version;
            if (versionString == null)
                versionString = AssemblyHelper.GetDynamoVersion().ToString();
            workspace.WorkspaceVersion = new System.Version(versionString);

            HomeWorkspaceModel homeWorkspace = workspace as HomeWorkspaceModel;
            if (homeWorkspace != null)
            {
                homeWorkspace.HasRunWithoutCrash = dynamoPreferences.HasRunWithoutCrash;

                RunType runType;
                if (!homeWorkspace.HasRunWithoutCrash || !Enum.TryParse(dynamoPreferences.RunType, false, out runType) || forceManualExecutionMode)
                    runType = RunType.Manual;
                int runPeriod;
                if (!Int32.TryParse(dynamoPreferences.RunPeriod, out runPeriod))
                    runPeriod = RunSettings.DefaultRunPeriod;
                homeWorkspace.RunSettings = new RunSettings(runType, runPeriod);

                RegisterHomeWorkspace(homeWorkspace);
            }

            CustomNodeWorkspaceModel customNodeWorkspace = workspace as CustomNodeWorkspaceModel;
            if (customNodeWorkspace != null)
                customNodeWorkspace.IsVisibleInDynamoLibrary = dynamoPreferences.IsVisibleInDynamoLibrary;

            return true;
        }

        private bool OpenXmlFile(WorkspaceInfo workspaceInfo, XmlDocument xmlDoc, out WorkspaceModel workspace)
        {
            CustomNodeManager.AddUninitializedCustomNodesInPath(
                Path.GetDirectoryName(workspaceInfo.FileName),
                IsTestMode);

            var result = workspaceInfo.IsCustomNodeWorkspace
                ? CustomNodeManager.OpenCustomNodeWorkspace(xmlDoc, workspaceInfo, IsTestMode, out workspace)
                : OpenXmlHomeWorkspace(xmlDoc, workspaceInfo, out workspace);

            workspace.OnCurrentOffsetChanged(
                this,
                new PointEventArgs(new Point2D(workspaceInfo.X, workspaceInfo.Y)));

            workspace.ScaleFactor = workspaceInfo.ScaleFactor;
            return result;
        }

        private bool OpenXmlHomeWorkspace(
            XmlDocument xmlDoc, WorkspaceInfo workspaceInfo, out WorkspaceModel workspace)
        {
            var nodeGraph = NodeGraph.LoadGraphFromXml(xmlDoc, NodeFactory);
            Guid deterministicId = GuidUtility.Create(GuidUtility.UrlNamespace, workspaceInfo.Name);
            var newWorkspace = new HomeWorkspaceModel(
                deterministicId,
                EngineController,
                Scheduler,
                NodeFactory,
                Utils.LoadTraceDataFromXmlDocument(xmlDoc),
                nodeGraph.Nodes,
                nodeGraph.Notes,
                nodeGraph.Annotations,
                nodeGraph.Presets,
                nodeGraph.ElementResolver,
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
            newWorkspace.RefreshCompleted += OnRefreshCompleted;

            newWorkspace.Disposed += () =>
            {
                newWorkspace.EvaluationCompleted -= OnEvaluationCompleted;
                newWorkspace.RefreshCompleted -= OnRefreshCompleted;
            };
        }

        #endregion

        #region backup/timer

        /// <summary>
        /// Backup all the files
        /// </summary>
        protected void SaveBackupFiles(object state)
        {
            OnRequestDispatcherBeginInvoke(() =>
            {
                // tempDict stores the list of backup files and their corresponding workspaces IDs
                // when the last auto-save operation happens. Now the IDs will be used to know
                // whether some workspaces have already been backed up. If so, those workspaces won't be
                // backed up again.
                var tempDict = new Dictionary<Guid, string>(backupFilesDict);
                backupFilesDict.Clear();
                PreferenceSettings.BackupFiles.Clear();
                foreach (var workspace in Workspaces)
                {
                    if (!workspace.HasUnsavedChanges)
                    {
                        if (workspace.Nodes.Any() &&
                            !workspace.Notes.Any())
                            continue;

                        if (tempDict.ContainsKey(workspace.Guid))
                        {
                            backupFilesDict.Add(workspace.Guid, tempDict[workspace.Guid]);
                            continue;
                        }
                    }

                    var savePath = pathManager.GetBackupFilePath(workspace);
                    OnRequestWorkspaceBackUpSave(savePath, true);
                    backupFilesDict[workspace.Guid] = savePath;
                    Logger.Log("Backup file is saved: " + savePath);
                }
                PreferenceSettings.BackupFiles.AddRange(backupFilesDict.Values);
            });
        }

        /// <summary>
        /// Start the timer to backup files periodically
        /// </summary>
        private void StartBackupFilesTimer()
        {
            // When running test cases, the dispatcher may be null which will cause the timer to
            // introduce a lot of threads. So the timer will not be started if test cases are running.
            if (IsTestMode)
                return;

            if (backupFilesTimer != null)
            {
                throw new Exception("The timer to backup files has already been started!");
            }

            backupFilesTimer = new Timer(SaveBackupFiles);
            backupFilesTimer.Change(PreferenceSettings.BackupInterval, PreferenceSettings.BackupInterval);
            Logger.Log(String.Format("Backup files timer is started with an interval of {0} milliseconds", PreferenceSettings.BackupInterval));
        }

        #endregion

        #region internal methods

        internal void PostUIActivation(object parameter)
        {
            Logger.Log(Resources.WelcomeMessage);
        }

        internal void DeleteModelInternal(List<ModelBase> modelsToDelete)
        {
            if (null == CurrentWorkspace)
                return;

            //Check for empty group
            var annotations = Workspaces.SelectMany(ws => ws.Annotations);
            foreach (var annotation in annotations)
            {
                //record the annotation before the models in it are deleted.
                foreach (var model in modelsToDelete)
                {
                    //If there is only one model, then deleting that model should delete the group. In that case, do not record
                    //the group for modification. Until we have one model in a group, group should be recorded for modification
                    //otherwise, undo operation cannot get the group back.
                    if (annotation.Nodes.Count() > 1 && annotation.Nodes.Where(x => x.GUID == model.GUID).Any())
                    {
                        CurrentWorkspace.RecordGroupModelBeforeUngroup(annotation);
                    }
                }

                if (annotation.Nodes.Any() && !annotation.Nodes.Except(modelsToDelete).Any())
                {
                    //Annotation Model has to be serialized first - before the nodes.
                    //so, store the Annotation model as first object. This will serialize the
                    //annotation before the nodes are deleted. So, when Undo is pressed,
                    //annotation model is deserialized correctly.
                    modelsToDelete.Insert(0, annotation);
                }
            }

            var cancelEventArgs = new CancelEventArgs();
            OnDeletionStarted(modelsToDelete, cancelEventArgs);
            if (cancelEventArgs.Cancel)
            {
                return;
            }

            CurrentWorkspace.RecordAndDeleteModels(modelsToDelete);

            var selection = DynamoSelection.Instance.Selection;
            foreach (ModelBase model in modelsToDelete)
            {
                selection.Remove(model); // Remove from selection set.
                model.Dispose();
            }

            OnDeletionComplete(this, EventArgs.Empty);
        }

        internal void UngroupModel(List<ModelBase> modelsToUngroup)
        {
            var emptyGroup = new List<ModelBase>();
            var annotations = Workspaces.SelectMany(ws => ws.Annotations);
            foreach (var model in modelsToUngroup)
            {
                foreach (var annotation in annotations)
                {
                    if (annotation.Nodes.Any(x => x.GUID == model.GUID))
                    {
                        var list = annotation.Nodes.ToList();

                        if (list.Count > 1)
                        {
                            CurrentWorkspace.RecordGroupModelBeforeUngroup(annotation);
                            if (list.Remove(model))
                            {
                                annotation.Nodes = list;
                                annotation.UpdateBoundaryFromSelection();
                            }
                        }
                        else
                        {
                            emptyGroup.Add(annotation);
                        }
                    }
                }
            }

            if (emptyGroup.Any())
            {
                DeleteModelInternal(emptyGroup);
            }
        }

        internal void AddToGroup(List<ModelBase> modelsToAdd)
        {
            var workspaceAnnotations = Workspaces.SelectMany(ws => ws.Annotations);
            var selectedGroup = workspaceAnnotations.FirstOrDefault(x => x.IsSelected);
            if (selectedGroup != null)
            {
                foreach (var model in modelsToAdd)
                {
                    CurrentWorkspace.RecordGroupModelBeforeUngroup(selectedGroup);
                    selectedGroup.AddToSelectedModels(model);
                }
            }

        }

        internal void DumpLibraryToXml(object parameter)
        {
            string fileName = String.Format("LibrarySnapshot_{0}.xml", DateTime.Now.ToString("yyyyMMddHmmss"));
            string fullFileName = Path.Combine(pathManager.LogDirectory, fileName);

            SearchModel.DumpLibraryToXml(fullFileName, PathManager.DynamoCoreDirectory);

            Logger.Log(string.Format(Resources.LibraryIsDumped, fullFileName));
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
        /// <api_stability>1</api_stability>
        public void AddHomeWorkspace()
        {
            var defaultWorkspace = new HomeWorkspaceModel(
                EngineController,
                Scheduler,
                NodeFactory,
                DebugSettings.VerboseLogging,
                IsTestMode, string.Empty);

            RegisterHomeWorkspace(defaultWorkspace);
            AddWorkspace(defaultWorkspace);
            CurrentWorkspace = defaultWorkspace;
        }

        /// <summary>
        ///     Add a new, visible Custom Node workspace to Dynamo
        /// </summary>
        /// <param name="workspace"><see cref="CustomNodeWorkspaceModel"/> to add</param>
        public void AddCustomNodeWorkspace(CustomNodeWorkspaceModel workspace)
        {
            AddWorkspace(workspace);
        }

        /// <summary>
        ///     Remove a workspace from the dynamo model.
        /// </summary>
        /// <param name="workspace">Workspace to remove</param>
        public void RemoveWorkspace(WorkspaceModel workspace)
        {
            OnWorkspaceRemoveStarted(workspace);
            if (_workspaces.Remove(workspace))
            {
                if (workspace is HomeWorkspaceModel)
                {
                    workspace.Dispose();
                }
                OnWorkspaceRemoved(workspace);
            }
        }

        /// <summary>
        ///     Opens an existing custom node workspace.
        /// </summary>
        /// <param name="guid">Identifier of the workspace to open</param>
        /// <returns>True if workspace was found and open</returns>
        public bool OpenCustomNodeWorkspace(Guid guid)
        {
            CustomNodeWorkspaceModel customNodeWorkspace;
            if (CustomNodeManager.TryGetFunctionWorkspace(guid, IsTestMode, out customNodeWorkspace))
            {
                if (!Workspaces.OfType<CustomNodeWorkspaceModel>().Contains(customNodeWorkspace))
                    AddWorkspace(customNodeWorkspace);

                CurrentWorkspace = customNodeWorkspace;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds a node to the current workspace.
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <param name="centered">Indicates if the node should be placed
        /// at the center of workspace.</param>
        /// <param name="addToSelection">Indicates if the newly added node
        /// should be selected</param>
        internal void AddNodeToCurrentWorkspace(NodeModel node, bool centered, bool addToSelection = true)
        {
            CurrentWorkspace.AddAndRegisterNode(node, centered);

            //TODO(Steve): This should be moved to WorkspaceModel.AddNode when all workspaces have their own selection -- MAGN-5707
            if (addToSelection)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(node);
            }

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
        ///     Paste ISelectable objects from the clipboard to the workspace
        /// so that the nodes appear in their original location with a slight offset
        /// </summary>
        public void Paste()
        {
            var locatableModels = ClipBoard.Where(model => model is NoteModel || model is NodeModel);
            var x = locatableModels.Min(m => m.X);
            var y = locatableModels.Min(m => m.Y);
            var targetPoint = new Point2D(x, y);

            Paste(targetPoint);
        }

        /// <summary>
        ///     Paste ISelectable objects from the clipboard to the workspace at specified point.
        /// </summary>
        /// <param name="targetPoint">Location where data will be pasted</param>
        /// <param name="useOffset">Indicates whether we will use current workspace offset or paste nodes
        /// directly in this point. </param>
        public void Paste(Point2D targetPoint, bool useOffset = true)
        {
            if (useOffset)
            {
                // Provide a small offset when pasting so duplicate pastes aren't directly on top of each other
                CurrentWorkspace.IncrementPasteOffset();
            }

            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.ClearSelection();

            //make a lookup table to store the guids of the
            //old models and the guids of their pasted versions
            var modelLookup = new Dictionary<Guid, ModelBase>();

            //make a list of all newly created models so that their
            //creations can be recorded in the undo recorder.
            var createdModels = new List<ModelBase>();

            var nodes = ClipBoard.OfType<NodeModel>();
            var connectors = ClipBoard.OfType<ConnectorModel>();
            var notes = ClipBoard.OfType<NoteModel>();
            var annotations = ClipBoard.OfType<AnnotationModel>();

            // Create the new NoteModel's
            var newNoteModels = new List<NoteModel>();
            foreach (var note in notes)
            {
                var noteModel = new NoteModel(note.X, note.Y, note.Text, Guid.NewGuid());
                //Store the old note as Key and newnote as value.
                modelLookup.Add(note.GUID, noteModel);
                newNoteModels.Add(noteModel);
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
                    newNode = new CodeBlockNodeModel(code, node.X, node.Y, LibraryServices, CurrentWorkspace.ElementResolver);
                }
                else
                {
                    var dynEl = node.Serialize(xmlDoc, SaveContext.Copy);
                    newNode = NodeFactory.CreateNodeFromXml(dynEl, SaveContext.Copy, CurrentWorkspace.ElementResolver);
                }

                var lacing = node.ArgumentLacing.ToString();
                newNode.UpdateValue(new UpdateValueParams("ArgumentLacing", lacing));
                if (!string.IsNullOrEmpty(node.Name) && !(node is Symbol) && !(node is Output))
                    newNode.Name = node.Name;

                newNode.Width = node.Width;
                newNode.Height = node.Height;

                modelLookup.Add(node.GUID, newNode);

                newNodeModels.Add(newNode);
            }

            var newItems = newNodeModels.Concat<ModelBase>(newNoteModels);

            var shiftX = targetPoint.X - newItems.Min(item => item.X);
            var shiftY = targetPoint.Y - newItems.Min(item => item.Y);
            var offset = useOffset ? CurrentWorkspace.CurrentPasteOffset : 0;

            foreach (var model in newItems)
            {
                model.X = model.X + shiftX + offset;
                model.Y = model.Y + shiftY + offset;
            }

            // Add the new NodeModel's to the Workspace
            foreach (var newNode in newNodeModels)
            {
                CurrentWorkspace.AddAndRegisterNode(newNode, false);
                createdModels.Add(newNode);
            }

            // TODO: is this required?
            OnRequestLayoutUpdate(this, EventArgs.Empty);

            // Add the new NoteModel's to the Workspace
            foreach (var newNote in newNoteModels)
            {
                CurrentWorkspace.AddNote(newNote, false);
                createdModels.Add(newNote);
            }

            ModelBase start;
            ModelBase end;
            var newConnectors =
                from c in connectors

                    // If the guid is in nodeLookup, then we connect to the new pasted node. Otherwise we
                    // re-connect to the original.
                let startNode =
                    modelLookup.TryGetValue(c.Start.Owner.GUID, out start)
                        ? start as NodeModel
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.Start.Owner.GUID)
                let endNode =
                    modelLookup.TryGetValue(c.End.Owner.GUID, out end)
                        ? end as NodeModel
                        : CurrentWorkspace.Nodes.FirstOrDefault(x => x.GUID == c.End.Owner.GUID)

                // Don't make a connector if either end is null.
                where startNode != null && endNode != null
                select
                    ConnectorModel.Make(startNode, endNode, c.Start.Index, c.End.Index);

            createdModels.AddRange(newConnectors);

            //Grouping depends on the selected node models.
            //so adding the group after nodes / notes are added to workspace.
            //select only those nodes that are part of a group.
            var newAnnotations = new List<AnnotationModel>();
            foreach (var annotation in annotations)
            {
                var annotationNodeModel = new List<NodeModel>();
                var annotationNoteModel = new List<NoteModel>();
                // some models can be deleted after copying them,
                // so they need to be in pasted annotation as well
                var modelsToRestore = annotation.DeletedModelBases.Intersect(ClipBoard);
                var modelsToAdd = annotation.Nodes.Concat(modelsToRestore);
                // checked condition here that supports pasting of multiple groups
                foreach (var models in modelsToAdd)
                {
                    ModelBase mbase;
                    modelLookup.TryGetValue(models.GUID, out mbase);
                    if (mbase is NodeModel)
                    {
                        annotationNodeModel.Add(mbase as NodeModel);
                    }
                    if (mbase is NoteModel)
                    {
                        annotationNoteModel.Add(mbase as NoteModel);
                    }
                }

                var annotationModel = new AnnotationModel(annotationNodeModel, annotationNoteModel)
                {
                    GUID = Guid.NewGuid(),
                    AnnotationText = annotation.AnnotationText,
                    Background = annotation.Background,
                    FontSize = annotation.FontSize
                };

                newAnnotations.Add(annotationModel);
            }

            // Add the new Annotation's to the Workspace
            foreach (var newAnnotation in newAnnotations)
            {
                CurrentWorkspace.AddAnnotation(newAnnotation);
                createdModels.Add(newAnnotation);
                AddToSelection(newAnnotation);
            }

            // adding an annotation overrides selection, so add nodes and notes after
            foreach (var item in newItems)
            {
                AddToSelection(item);
            }

            // Record models that are created as part of the command.
            CurrentWorkspace.RecordCreatedModels(createdModels);
        }

        /// <summary>
        ///     Add an ISelectable object to the selection.
        /// </summary>
        /// <param name="parameters">The object to add to the selection.</param>
        public void AddToSelection(object parameters)
        {
            var selectable = parameters as ISelectable;
            if (selectable != null)
            {
                DynamoSelection.Instance.Selection.AddUnique(selectable);
            }
        }

        /// <summary>
        ///     Clear the workspace. Removes all nodes, notes, and connectors from the current workspace.
        /// </summary>
        public void ClearCurrentWorkspace()
        {
            OnWorkspaceClearing();

            CurrentWorkspace.Clear();

            //don't save the file path
            CurrentWorkspace.FileName = "";
            CurrentWorkspace.HasUnsavedChanges = false;
            CurrentWorkspace.WorkspaceVersion = AssemblyHelper.GetDynamoVersion();

            OnWorkspaceCleared(CurrentWorkspace);
        }

        #endregion

        #region private methods

        private void LogMessage(ILogMessage obj)
        {
            Logger.Log(obj);
        }

#if DEBUG_LIBRARY
        private void DumpLibrarySnapshot(IEnumerable<Engine.FunctionGroup> functionGroups)
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

        /// <summary>
        /// This method updates the node search library to either hide or unhide nodes that belong
        /// to a specified assembly name and namespace. These nodes will be hidden from the node
        /// library sidebar and from the node search.
        /// </summary>
        /// <param name="hide">Set to true to hide, set to false to unhide.</param>
        /// <param name="library">The assembly name of the library.</param>
        /// <param name="namespc">The namespace of the nodes to be hidden.</param>
        internal void HideUnhideNamespace(bool hide, string library, string namespc)
        {
            var str = library + ':' + namespc;
            var namespaces = PreferenceSettings.NamespacesToExcludeFromLibrary;

            if (hide)
            {
                if (!namespaces.Contains(str))
                {
                    namespaces.Add(str);
                }
            }
            else // unhide
            {
                namespaces.Remove(str);
            }
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
            if (functionDescriptor.IsVisibleInLibrary)
            {
                SearchModel.Add(new ZeroTouchSearchElement(functionDescriptor));
            }
        }

        /// <summary>
        ///     Adds a workspace to the dynamo model.
        /// </summary>
        /// <param name="workspace"></param>
        public void AddWorkspace(WorkspaceModel workspace)
        {
            if (workspace == null) return;

            Action savedHandler = () => OnWorkspaceSaved(workspace);
            workspace.Saved += savedHandler;
            workspace.MessageLogged += LogMessage;
            workspace.PropertyChanged += OnWorkspacePropertyChanged;
            workspace.Disposed += () =>
            {
                workspace.Saved -= savedHandler;
                workspace.MessageLogged -= LogMessage;
                workspace.PropertyChanged -= OnWorkspacePropertyChanged;
            };

            _workspaces.Add(workspace);
            CheckForXMLDummyNodes(workspace);
            OnWorkspaceAdded(workspace);
        }

        private void CheckForXMLDummyNodes(WorkspaceModel workspace)
        {
            //if the graph that is opened contains xml dummynodes log a notification 
            if (workspace.containsXmlDummyNodes())
            {
                this.Logger.LogNotification("DynamoViewModel",
                  Resources.UnresolvedNodesWarningTitle,
                  Resources.UnresolvedNodesWarningShortMessage,
                  Resources.UnresolvedNodesWarningMessage);
                if (!IsTestMode)
                {
                    DisplayXmlDummyNodeWarning();
                }
                //raise a window as well so the user is clearly alerted to this state.
            }
        }
        private void DisplayXmlDummyNodeWarning()
        {
            var xmlDummyNodeCount = this.CurrentWorkspace.Nodes.OfType<DummyNode>().
                 Where(node => node.OriginalNodeContent is XmlElement).Count();

            Logging.Analytics.LogPiiInfo("XmlDummyNodeWarning",
                xmlDummyNodeCount.ToString());

            string summary = Resources.UnresolvedNodesWarningShortMessage;
            var description = Resources.UnresolvedNodesWarningMessage;
            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
            var args = new TaskDialogEventArgs(
               new Uri(imageUri, UriKind.Relative),
               Resources.UnresolvedNodesWarningTitle, summary, description);

            args.AddRightAlignedButton((int)ButtonId.Proceed, Resources.OKButton);
            OnRequestTaskDialog(null, args);
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

            Logging.Analytics.LogPiiInfo(
                "ObsoleteFileMessage",
                fullFilePath + " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            string summary = Resources.FileCannotBeOpened;
            var description =
                string.Format(
                    Resources.ObsoleteFileDescription,
                    fullFilePath,
                    fileVersion,
                    currVersion);

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_obsolete_file.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Resources.ObsoleteFileTitle,
                summary,
                description);

            args.AddRightAlignedButton((int)ButtonId.Ok, Resources.OKButton);

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
            Dynamo.Logging.Analytics.TrackEvent(Actions.EngineFailure, Categories.Stability);

            if (exception != null)
            {
                Dynamo.Logging.Analytics.TrackException(exception, false);
            }

            string summary = Resources.UnhandledExceptionSummary;

            string description = Resources.DisplayEngineFailureMessageDescription;

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_crash.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Resources.UnhandledExceptionTitle,
                summary,
                description);

            args.AddRightAlignedButton((int)ButtonId.Submit, Resources.SubmitBugButton);
            args.AddRightAlignedButton((int)ButtonId.Ok, Resources.ArggOKButton);
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
            var fileVer = ((fileVersion != null) ? fileVersion.ToString() : Resources.UnknownVersion);
            var currVer = ((currVersion != null) ? currVersion.ToString() : Resources.UnknownVersion);

            Logging.Analytics.LogPiiInfo("FutureFileMessage", fullFilePath +
                " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            string summary = Resources.FutureFileSummary;
            var description = string.Format(Resources.FutureFileDescription, fullFilePath, fileVersion, currVersion);

            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                Resources.FutureFileTitle, summary, description)
            { ClickedButtonId = (int)ButtonId.Cancel };

            args.AddRightAlignedButton((int)ButtonId.Cancel, Resources.CancelButton);
            args.AddRightAlignedButton((int)ButtonId.DownloadLatest, Resources.DownloadLatestButton);
            args.AddRightAlignedButton((int)ButtonId.Proceed, Resources.ProceedButton);

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

            if (args.PropertyName == "EnablePresetOptions")
                OnPropertyChanged("EnablePresetOptions");
        }

        #endregion
    }
}
