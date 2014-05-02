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
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Services;
using Dynamo.UI;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUnits;
using Microsoft.Practices.Prism.ViewModel;
using NUnit.Framework;
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
    public static partial class Context
    {
        public const string NONE = "None";
        public const string REVIT_2013 = "Revit 2013";
        public const string REVIT_2014 = "Revit 2014";
        public const string VASARI_2013 = "Vasari 2013";
        public const string VASARI_2014 = "Vasari 2014";
    }

    public delegate void ImageSaveEventHandler(object sender, ImageSaveEventArgs e);

    public class DynamoController : NotificationObject
    {
        private static bool testing;

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
        public InfoBubbleViewModel InfoBubbleViewModel { get; internal set; }
        public DynamoModel DynamoModel { get; set; }
        public Dispatcher UIDispatcher { get; set; }
        public IUpdateManager UpdateManager { get; set; }
        public IWatchHandler WatchHandler { get; set; }
        public IPreferences PreferenceSettings { get; set; }
        public IVisualizationManager VisualizationManager { get; set; }

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

        private bool isShowPreViewByDefault;
        public bool IsShowPreviewByDefault
        {
            get { return isShowPreViewByDefault;}
            set { isShowPreViewByDefault = value; RaisePropertyChanged("IsShowPreviewByDefault"); }
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

        #endregion

        #region Constructor and Initialization

        public static DynamoController MakeSandbox(string commandFilePath = null)
        {
            DynamoController controller;
            var logger = new DynamoLogger();
            dynSettings.DynamoLogger = logger;

            var updateManager = new UpdateManager.UpdateManager(logger);

            // If a command file path is not specified or if it is invalid, then fallback.
            if (string.IsNullOrEmpty(commandFilePath) || (File.Exists(commandFilePath) == false))
            {
                
                controller = new DynamoController("None", updateManager,
                    new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load());

                controller.DynamoViewModel = new DynamoViewModel(controller, null);
            }
            else
            {
                controller = new DynamoController("None", updateManager,
                 new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load());

                controller.DynamoViewModel = new DynamoViewModel(controller, commandFilePath);
            }

            controller.VisualizationManager = new VisualizationManager();
            return controller;
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(string context, IUpdateManager updateManager,
            IWatchHandler watchHandler, IPreferences preferences)
        {
            IsCrashing = false;

            dynSettings.Controller = this;

            Context = context;

            //Start heartbeat reporting
            InstrumentationLogger.Start();

            PreferenceSettings = preferences;
            ((PreferenceSettings) PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;

            SIUnit.LengthUnit = PreferenceSettings.LengthUnit;
            SIUnit.AreaUnit = PreferenceSettings.AreaUnit;
            SIUnit.VolumeUnit = PreferenceSettings.VolumeUnit;
            SIUnit.NumberFormat = PreferenceSettings.NumberFormat;

            UpdateManager = updateManager;
            UpdateManager.UpdateDownloaded += updateManager_UpdateDownloaded;
            UpdateManager.ShutdownRequested += updateManager_ShutdownRequested;
            UpdateManager.CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation),dynSettings.DynamoLogger, UpdateManager.UpdateDataAvailable));

            WatchHandler = watchHandler;

            //create the model
            DynamoModel = new DynamoModel ();
            DynamoModel.AddHomeWorkspace();
            DynamoModel.CurrentWorkspace = DynamoModel.HomeSpace;
            DynamoModel.CurrentWorkspace.X = 0;
            DynamoModel.CurrentWorkspace.Y = 0;

            // custom node loader
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            CustomNodeManager = new CustomNodeManager(pluginsPath);
            
            SearchViewModel = new SearchViewModel();

            dynSettings.PackageLoader = new PackageLoader();

            dynSettings.PackageLoader.DoCachedPackageUninstalls();
            dynSettings.PackageLoader.LoadPackages();

            DisposeLogic.IsShuttingDown = false;

            EngineController = new EngineController(this);

            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            //TODO(Luke): Push this into a resync call with the engine controller
            ResetEngine();

            dynSettings.DynamoLogger.Log(String.Format(
                "Dynamo -- Build {0}",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.ClearCachedAssemblies();
            DynamoLoader.LoadNodeModels();
            
            InfoBubbleViewModel = new InfoBubbleViewModel();

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));

            evaluationWorker.DoWork += RunThread;
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

        void updateManager_UpdateDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            UpdateManager.QuitAndInstallUpdate();
        }

        void updateManager_ShutdownRequested(object sender, EventArgs e)
        {
            UIDispatcher.Invoke((Action) delegate
            {
                ShutDown(true);
                UpdateManager.HostApplicationBeginQuit(this, e);
            });
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

        #region Running
        
        private readonly BackgroundWorker evaluationWorker = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };

        public bool Running { get; protected set; }

        public void RunExpression(int? executionInterval = null)
        {
            //dynSettings.DynamoLogger.LogWarning("Running expression", WarningLevel.Mild);

            //If we're already running, do nothing.
            if (Running)
                return;

            // If there is preloaded trace data, send that along to the current
            // LiveRunner instance. Here we make sure it is done exactly once 
            // by resetting WorkspaceModel.PreloadedTraceData property after it 
            // is obtained.
            // 
            var traceData = DynamoViewModel.Model.HomeSpace.PreloadedTraceData;
            DynamoViewModel.Model.HomeSpace.PreloadedTraceData = null; // Reset.
            EngineController.LiveRunnerCore.SetTraceDataForNodes(traceData);

            EngineController.GenerateGraphSyncData(DynamoViewModel.Model.HomeSpace.Nodes);
            if (!EngineController.HasPendingGraphSyncData)
                return;

            //We are now considered running
            Running = true;

            if (!testing)
            {
                //Setup background worker
                DynamoViewModel.RunEnabled = false;

                //Let's start
                evaluationWorker.RunWorkerAsync(executionInterval);
            }
            else
            {
                //for testing, we do not want to run asynchronously, as it will finish the 
                //test before the evaluation (and the run) is complete
                RunThread(evaluationWorker, new DoWorkEventArgs(executionInterval));
            }
        }

        private void RunThread(object s, DoWorkEventArgs args)
        {
            var bw = s as BackgroundWorker;

            do
            {
                Evaluate();

                if (args == null || args.Argument == null)
                    break;

                var sleep = (int)args.Argument;
                Thread.Sleep(sleep);
            } 
            while (bw != null && !bw.CancellationPending);

            OnRunCompleted(this, false);

            Running = false;
            DynamoViewModel.RunEnabled = true;
        }

        protected virtual void Evaluate()
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                Eval();
            }
            catch (Exception ex)
            {
                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    dynSettings.DynamoLogger.Log(ex);
                }

                OnRunCancelled(true);

                if (IsTestMode)
                    Assert.Fail(ex.Message + ":" + ex.StackTrace);
            }
            finally
            {
                sw.Stop();

                dynSettings.DynamoLogger.Log(string.Format("Evaluation completed in {0}", sw.Elapsed));
            }

            OnEvaluationCompleted(this, EventArgs.Empty);
        }

        private void Eval()
        {
            //Print some stuff if we're in debug mode
            if (DynamoViewModel.RunInDebug)
            {
            }

            // We have caught all possible exceptions in UpdateGraph call, I am 
            // not certain if this try-catch block is still meaningful or not.
            try
            {
                Exception fatalException = null;
                bool updated = EngineController.UpdateGraph(ref fatalException);

                // If there's a fatal exception, show it to the user, unless of course 
                // if we're running in a unit-test, in which case there's no user. I'd 
                // like not to display the dialog and hold up the continuous integration.
                // 
                if (IsTestMode == false && (fatalException != null))
                {
                    Action showFailureMessage = () => Nodes.Utilities.DisplayEngineFailureMessage(fatalException);

                    // The "Run" method is guaranteed to be called on a background 
                    // thread (for Revit's case, it is the idle thread). Here we 
                    // schedule the message to show up when the UI gets around and 
                    // handle it.
                    // 
                    if (UIDispatcher != null)
                        UIDispatcher.BeginInvoke(showFailureMessage);
                }

                // Currently just use inefficient way to refresh preview values. 
                // After we switch to async call, only those nodes that are really 
                // updated in this execution session will be required to update 
                // preview value.
                if (updated)
                {
                    var nodes = DynamoViewModel.Model.HomeSpace.Nodes;
                    foreach (NodeModel node in nodes)
                        node.IsUpdated = true;
                }
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                dynSettings.DynamoLogger.Log(ex);

                OnRunCancelled(true);

                //If we are testing, we need to throw an exception here
                //which will, in turn, throw an Assert.Fail in the 
                //Evaluation thread.
                if (IsTestMode)
                    throw new Exception(ex.Message);
            }
        }
        
        protected virtual void OnRunCancelled(bool error)
        {
            //dynSettings.Controller.DynamoLogger.Log("Run cancelled. Error: " + error);
        }

        /// <summary>
        /// Called when evaluation completes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnEvaluationCompleted(object sender, EventArgs e)
        {
            if (EvaluationCompleted != null)
                EvaluationCompleted(sender, e);
        }

    #endregion

        public virtual void ResetEngine()
        {
            if (EngineController != null)
            {
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(this);
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

        internal void RunExprCmd(object parameters)
        {
            bool displayErrors = Convert.ToBoolean(parameters);
            var command = new DynamoViewModel.RunCancelCommand(displayErrors, false);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanRunExprCmd(object parameters)
        {
            return (dynSettings.Controller != null);
        }

        internal void RunCancelInternal(bool displayErrors, bool cancelRun)
        {
            if (cancelRun)
                evaluationWorker.CancelAsync();
            else
                RunExpression();
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
    
    public class CrashPromptArgs : EventArgs
    {
        [Flags]
        public enum DisplayOptions
        {
            IsDefaultTextOverridden = 0x00000001,
            HasDetails = 0x00000002,
            HasFilePath = 0x00000004
        }

        public DisplayOptions Options { get; private set; }
        public string Details { get; private set; }
        public string OverridingText { get; private set; }
        public string FilePath { get; private set; }

        // Default Crash Prompt
        public CrashPromptArgs(string details, string overridingText = null, string filePath = null)
        {
            if (details != null)
            {
                Details = details;
                Options |= DisplayOptions.HasDetails;
            }

            if (overridingText != null)
            {
                OverridingText = overridingText;
                Options |= DisplayOptions.IsDefaultTextOverridden;
            }

            if (filePath != null)
            {
                FilePath = filePath;
                Options |= DisplayOptions.HasFilePath;
            }
        }

        public bool IsDefaultTextOverridden()
        {
            return Options.HasFlag(DisplayOptions.IsDefaultTextOverridden);
        }

        public bool HasDetails()
        {
            return Options.HasFlag(DisplayOptions.HasDetails);
        }

        public bool IsFilePath()
        {
            return Options.HasFlag(DisplayOptions.HasFilePath);
        }
    }

}
