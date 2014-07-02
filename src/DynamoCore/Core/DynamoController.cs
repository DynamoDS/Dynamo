using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using DSNodeServices;
using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Services;
using Dynamo.TestInfrastructure;
using Dynamo.UI;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUnits;
using DynamoUtilities;
using Microsoft.Practices.Prism.ViewModel;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Dynamo.UI.Prompts;

namespace Dynamo
{
    /// <summary>
    /// Context values are required during controller instantiation to flag
    /// what application Dynamo is running within. Use NONE for the sandbox and
    /// other applications where context-sensitive loading are not required.
    /// </summary>
    public static class Context
    {
        public const string NONE = "None";
        public const string REVIT_2013 = "Revit 2013";
        public const string REVIT_2014 = "Revit 2014";
        public const string REVIT_2015 = "Revit 2015";
        public const string VASARI_2013 = "Vasari 2013";
        public const string VASARI_2014 = "Vasari 2014";
    }

    public delegate void ImageSaveEventHandler(object sender, ImageSaveEventArgs e);

    public class DynamoController : NotificationObject
    {
        private static bool testing;
        /// <summary>
        /// DynamoRunner handles execution and dispatch of the graph at the UI layer
        /// </summary>
        public DynamoRunner Runner { get; set; }

        #region properties

        public bool IsCrashing { get; set; }

        private bool uiLocked = true;
        public bool IsUILocked
        {
            get { return uiLocked; }
            set
            {
                uiLocked = value;
                RaisePropertyChanged("IsUILocked");
            }
        }

        private readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        private readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        public CustomNodeManager CustomNodeManager { get; internal set; }
        public SearchViewModel SearchViewModel { get; internal set; }
        public DynamoViewModel DynamoViewModel { get; set; }
        public DynamoModel DynamoModel { get; set; }
        public Dispatcher UIDispatcher { get; set; }
        public IUpdateManager UpdateManager { get; set; }
        public IWatchHandler WatchHandler { get; set; }
        public IPreferences PreferenceSettings { get; set; }
        public IVisualizationManager VisualizationManager { get; set; }
        public DebugSettings DebugSettings { get; set; }

        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        public static bool IsTestMode 
        {
            get { return testing; }
            set { testing = value; }
        }

        ObservableCollection<ModelBase> clipBoard = new ObservableCollection<ModelBase>();

        public ObservableCollection<ModelBase> ClipBoard
        {
            get { return clipBoard; }
            set { clipBoard = value; }
        }

        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        public Dictionary<string, TypeLoadData> BuiltInTypesByName
        {
            get { return builtinTypesByTypeName; }
        }

        public string Context { get; set; }

        public bool IsShowingConnectors
        {
            get { return PreferenceSettings.ShowConnector; }
            set
            {
                PreferenceSettings.ShowConnector = value;
            }
        }

        public ConnectorType ConnectorType
        {
            get { return PreferenceSettings.ConnectorType; }
            set
            {
                PreferenceSettings.ConnectorType = value;
            }
        }

        public EngineController EngineController { get; protected set; }


        #endregion

        #region events

        /// <summary>
        /// An event triggered when a single graph evaluation completes.
        /// </summary>
        public event EventHandler EvaluationCompleted;

        /// <summary>
        /// An event which requests that a node be selected
        /// </summary>
        public event NodeEventHandler RequestNodeSelect;
        public virtual void OnRequestSelect(object sender, ModelEventArgs e)
        {
            if (RequestNodeSelect != null)
                RequestNodeSelect(sender, e);
        }

        public delegate void RunCompletedHandler(object controller, bool success);
        public event RunCompletedHandler RunCompleted;
        public virtual void OnRunCompleted(object sender, bool success)
        {
            if (RunCompleted != null)
                RunCompleted(sender, success);
        }

        public event EventHandler RequestsRedraw;
        public virtual void OnRequestsRedraw(object sender, EventArgs e)
        {
            if (RequestsRedraw != null)
                RequestsRedraw(sender, e);
        }

        // TODO(Ben): Obsolete CrashPrompt and make use of GenericTaskDialog.
        public delegate void CrashPromptHandler(object sender, CrashPromptArgs e);
        public event CrashPromptHandler RequestsCrashPrompt;
        public void OnRequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, args);
        }

        internal delegate void TaskDialogHandler(object sender, TaskDialogEventArgs e);
        internal event TaskDialogHandler RequestTaskDialog;
        internal void OnRequestTaskDialog(object sender, TaskDialogEventArgs args)
        {
            if (RequestTaskDialog != null)
                RequestTaskDialog(sender, args);
        }

        /// <summary>
        /// Called when evaluation completes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnEvaluationCompleted(object sender, EventArgs e)
        {
            if (EvaluationCompleted != null)
                EvaluationCompleted(sender, e);
        }


        #endregion

        #region Constructor and Initialization

        public static DynamoController MakeSandbox(string commandFilePath = null)
        {
            var corePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DynamoPathManager.Instance.InitializeCore(corePath);

            DynamoController controller;
            var logger = new DynamoLogger(DynamoPathManager.Instance.Logs);
            dynSettings.DynamoLogger = logger;

            var updateManager = new UpdateManager.UpdateManager(logger);

            // If a command file path is not specified or if it is invalid, then fallback.
            if (string.IsNullOrEmpty(commandFilePath) || (File.Exists(commandFilePath) == false))
            {
                controller = new DynamoController("None", updateManager,
                    new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load(), corePath);

                controller.DynamoViewModel = new DynamoViewModel(controller, null);
            }
            else
            {
                controller = new DynamoController("None", updateManager,
                 new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load(), corePath);

                controller.DynamoViewModel = new DynamoViewModel(controller, commandFilePath);
            }

            controller.VisualizationManager = new VisualizationManager();
            return controller;
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

            foreach (var node in this.DynamoModel.Nodes)
            {
                node.RequiresRecalc = true;
            }

            //DynamoLoader.ClearCachedAssemblies();
            //DynamoLoader.LoadNodeModels();
            
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(string context, IUpdateManager updateManager,
            IWatchHandler watchHandler, IPreferences preferences, string corePath)
        {
            DebugSettings = new DebugSettings();

            IsCrashing = false;

            dynSettings.Controller = this;

            Context = context;

            
            PreferenceSettings = preferences;
            ((PreferenceSettings) PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;

            SIUnit.LengthUnit = PreferenceSettings.LengthUnit;
            SIUnit.AreaUnit = PreferenceSettings.AreaUnit;
            SIUnit.VolumeUnit = PreferenceSettings.VolumeUnit;
            SIUnit.NumberFormat = PreferenceSettings.NumberFormat;

            UpdateManager = updateManager;
            UpdateManager.CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation),dynSettings.DynamoLogger, UpdateManager.UpdateDataAvailable));

            WatchHandler = watchHandler;

            //Start heartbeat reporting
            //This needs to be done after the update manager has been initialised
            //so that the version number can be reported
            InstrumentationLogger.Start();


            //create the model
            DynamoModel = new DynamoModel ();
            DynamoModel.AddHomeWorkspace();
            DynamoModel.CurrentWorkspace = DynamoModel.HomeSpace;
            DynamoModel.CurrentWorkspace.X = 0;
            DynamoModel.CurrentWorkspace.Y = 0;

            // custom node loader
            CustomNodeManager = new CustomNodeManager(DynamoPathManager.Instance.UserDefinitions);

            SearchViewModel = new SearchViewModel();

            dynSettings.PackageLoader = new PackageLoader();

            dynSettings.PackageLoader.DoCachedPackageUninstalls();
            dynSettings.PackageLoader.LoadPackages();

            DisposeLogic.IsShuttingDown = false;

            EngineController = new EngineController(this);
            CustomNodeManager.RecompileAllNodes(EngineController);

            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            //TODO(Luke): Push this into a resync call with the engine controller
            ResetEngine();

            dynSettings.DynamoLogger.Log(String.Format(
                "Dynamo -- Build {0}",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.ClearCachedAssemblies();
            DynamoLoader.LoadNodeModels();

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));

            Runner = new DynamoRunner();
        }

        /// <summary>
        /// Responds to property update notifications on the preferences,
        /// and synchronizes with the Units Manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PreferenceSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        public virtual void
            ShutDown(bool shutDownHost, EventArgs args = null)
        {
            EngineController.Dispose();
            EngineController = null;

            PreferenceSettings.Save();

            dynSettings.Controller.DynamoModel.OnCleanup(args);
            dynSettings.Controller = null;

            ((DynamoLogger)dynSettings.DynamoLogger).Dispose();
        }



        public virtual void ResetEngine()
        {
            if (EngineController != null)
            {
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(this);
            CustomNodeManager.RecompileAllNodes(EngineController);
        }

        public void RequestRedraw()
        {
            OnRequestsRedraw(this, EventArgs.Empty);
        }

        public void RequestClearDrawables()
        {
            //VisualizationManager.ClearRenderables();
        }

        public void CancelRunCmd(object parameter)
        {
            var command = new DynamoViewModel.RunCancelCommand(false, true);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanCancelRunCmd(object parameter)
        {
            return true;
        }

        public void RunExpression(object parameters) // For unit test cases.
        {
            RunExpression();
        }

        public void RunExpression()
        {
            Runner.RunExpression();
        }

        internal void RunExprCmd(object parameters)
        {
            bool displayErrors = Convert.ToBoolean(parameters);
            var command = new DynamoViewModel.RunCancelCommand(displayErrors, false);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal void ForceRunExprCmd(object parameters)
        {
            bool displayErrors = Convert.ToBoolean(parameters);
            var command = new DynamoViewModel.ForceRunCancelCommand(displayErrors, false);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal void MutateTestCmd(object parameters)
        {
            var command = new DynamoViewModel.MutateTestCommand();
            DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanRunExprCmd(object parameters)
        {
            return (dynSettings.Controller != null);
        }

        
        internal void RunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync();
            else
                RunExpression();
        }

        internal void ForceRunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                Runner.CancelAsync();
            else
            {
                dynSettings.DynamoLogger.Log(
"Beginning engine reset");

                
                Reset();


                dynSettings.DynamoLogger.Log(
"Reset complete");

                RunExpression();
            }
        }

        public void DisplayFunction(object parameters)
        {
            CustomNodeManager.GetFunctionDefinition((Guid)parameters);
        }

        internal bool CanDisplayFunction(object parameters)
        {
            return dynSettings.CustomNodes.Any(x => x.Value == (Guid)parameters);
        }

        public void ReportABug(object parameter)
        {
            Process.Start(Configurations.GitHubBugReportingLink);
        }

        internal void DownloadDynamo()
        {
            Process.Start(Configurations.DynamoDownloadLink);
        }

        internal bool CanReportABug(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Clear the UI log.
        /// </summary>
        public void ClearLog(object parameter)
        {
            dynSettings.DynamoLogger.ClearLog();
        }

        internal bool CanClearLog(object parameter)
        {
            return true;
        }
    }
    
}
