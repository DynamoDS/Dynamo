using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using DSNodeServices;
using Dynamo.DSEngine;
using Dynamo.FSchemeInterop;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI;
using Dynamo.Units;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.Practices.Prism.ViewModel;
using NUnit.Framework;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

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

    public class DynamoController:NotificationObject
    {
        private static bool testing = false;
        

        #region properties

        private bool _isCrashing = false;
        public bool IsCrashing
        {
            get { return _isCrashing; }
            set { _isCrashing = value; }
        }

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

        
        protected VisualizationManager visualizationManager;

        public CustomNodeManager CustomNodeManager { get; internal set; }
        public SearchViewModel SearchViewModel { get; internal set; }
        public DynamoViewModel DynamoViewModel { get; internal set; }
        public InfoBubbleViewModel InfoBubbleViewModel { get; internal set; }
        public DynamoModel DynamoModel { get; set; }
        public Dispatcher UIDispatcher { get; set; }
        public IUpdateManager UpdateManager { get; set; }
        public IWatchHandler WatchHandler { get; set; }
        public IPreferences PreferenceSettings { get; set; }

        public virtual VisualizationManager VisualizationManager
        {
            get { return visualizationManager ?? (visualizationManager = new VisualizationManager(this)); }
        }

        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        public static bool IsTestMode 
        {
            get { return DynamoController.testing; }
            set { DynamoController.testing = value; }
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

        private string context;
        public string Context
        {
            get { return context; }
            set { context = value; }
        }

        private bool _isShowingConnectors = true;
        public bool IsShowingConnectors
        {
            get { return _isShowingConnectors; }
            set
            {
                _isShowingConnectors = value;
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

        private bool isShowPreViewByDefault = false;
        public bool IsShowPreviewByDefault
        {
            get { return isShowPreViewByDefault;}
            set { isShowPreViewByDefault = value; RaisePropertyChanged("IsShowPreviewByDefault"); }
        }

        private EngineController _engineController = null;
        public EngineController EngineController
        {
            get { return _engineController; }
            protected set { _engineController = value; }
        }

        #endregion

        #region events

        /// <summary>
        /// An event triggered when evaluation completes.
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

        public delegate void CrashPromptHandler(object sender, CrashPromptArgs e);
        public event CrashPromptHandler RequestsCrashPrompt;
        public void OnRequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, args);
        }

        #endregion

        #region Constructor and Initialization

        public static DynamoController MakeSandbox(string commandFilePath = null)
        {
            //var env = new ExecutionEnvironment();

            // If a command file path is not specified or if it is invalid, then fallback.
            if (string.IsNullOrEmpty(commandFilePath) || (File.Exists(commandFilePath) == false))
                return new DynamoController(typeof(DynamoViewModel), "None", new UpdateManager.UpdateManager(), new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load());

            return new DynamoController(typeof(DynamoViewModel), "None", commandFilePath, new UpdateManager.UpdateManager(), new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load());
        }

        public DynamoController(Type viewModelType, string context, IUpdateManager updateManager, IWatchHandler watchHandler, IPreferences preferences) : 
            this(viewModelType, context, null, updateManager, watchHandler, preferences)
        {
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(Type viewModelType, string context, string commandFilePath, IUpdateManager updateManager, IWatchHandler watchHandler, IPreferences preferences)
        {
            DynamoLogger.Instance.StartLogging();

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
            UpdateManager.CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation),DynamoLogger.Instance, UpdateManager.UpdateDataAvailable));

            WatchHandler = watchHandler;

            //create the view model to which the main window will bind
            //the DynamoModel is created therein
            DynamoViewModel = (DynamoViewModel)Activator.CreateInstance(
                viewModelType, new object[] { this, commandFilePath });

            // custom node loader
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            CustomNodeManager = new CustomNodeManager(pluginsPath);
            
            SearchViewModel = new SearchViewModel();

            dynSettings.PackageLoader = new PackageLoader();

            dynSettings.PackageLoader.DoCachedPackageUninstalls();
            dynSettings.PackageLoader.LoadPackages();
            
            DynamoViewModel.Model.CurrentWorkspace.X = 0;
            DynamoViewModel.Model.CurrentWorkspace.Y = 0;

            DisposeLogic.IsShuttingDown = false;
            EngineController = new EngineController(this, false);
            //This is necessary to avoid a race condition by causing a thread join
            //inside the vm exec
            //TODO(Luke): Push this into a resync call with the engine controller
            ResetEngine();

            DynamoLogger.Instance.Log(String.Format(
                "Dynamo -- Build {0}",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.ClearCachedAssemblies();
            DynamoLoader.LoadNodeModels();

            //run tests
            if (FScheme.RunTests(DynamoLogger.Instance.Log))
            {
                DynamoLogger.Instance.Log("All Tests Passed. Core library loaded OK.");
            }

            InfoBubbleViewModel = new InfoBubbleViewModel();

            AddPythonBindings();

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrations));
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
                    SIUnit.LengthUnit = PreferenceSettings.LengthUnit;
                    break;
                case "AreaUnit":
                    SIUnit.AreaUnit = PreferenceSettings.AreaUnit;
                    break;
                case "VolumeUnit":
                    SIUnit.VolumeUnit = PreferenceSettings.VolumeUnit;
                    break;
                case "NumberFormat":
                    SIUnit.NumberFormat = PreferenceSettings.NumberFormat;
                    break;
            }
        }

        void updateManager_UpdateDownloaded(object sender, UpdateManager.UpdateDownloadedEventArgs e)
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

        public virtual void ShutDown(bool shutDownHost)
        {
            EngineController.Dispose();
            EngineController = null;

            PreferenceSettings.Save();

            dynSettings.Controller.DynamoModel.OnCleanup(null);
            dynSettings.Controller = null;
            
            DynamoSelection.Instance.ClearSelection();
            DynamoLogger.Instance.FinishLogging();
        }

        #region Running

        //protected bool _debug;
        private bool _showErrors;

        private bool runAgain;
        public bool Running { get; protected set; }

        public bool RunCancelled { get; protected internal set; }

        internal void QueueRun()
        {
            RunCancelled = true;
            runAgain = true;
        }

        public void RunExpression(bool showErrors = true)
        {
            //DynamoLogger.Instance.LogWarning("Running expression", WarningLevel.Mild);

            //If we're already running, do nothing.
            if (Running)
                return;


#if USE_DSENGINE
            EngineController.GenerateGraphSyncData(DynamoViewModel.Model.HomeSpace.Nodes);
            if (!EngineController.HasPendingGraphSyncData)
            {
                return;
            }
#endif

            _showErrors = showErrors;

            //TODO: Hack. Might cause things to break later on...
            //Reset Cancel and Rerun flags
            RunCancelled = false;
            runAgain = false;

            //We are now considered running
            Running = true;

            if (!testing)
            {
                //Setup background worker
                var worker = new BackgroundWorker();
                worker.DoWork += EvaluationThread;

                DynamoViewModel.RunEnabled = false;

                //Let's start
                worker.RunWorkerAsync();
            }
            else
                //for testing, we do not want to run
                //asynchronously, as it will finish the 
                //test before the evaluation (and the run)
                //is complete
                EvaluationThread(null, null);
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            var sw = new Stopwatch();
            sw.Start();

#if !USE_DSENGINE
            //Get our entry points (elements with nothing connected to output)
            List<NodeModel> topElements = DynamoViewModel.Model.HomeSpace.GetTopMostNodes().ToList();

            //Mark the topmost as dirty/clean
            foreach (NodeModel topMost in topElements)
            {
                topMost.MarkDirty();
            }
#endif
            try
            {

#if USE_DSENGINE
                Run();
#else
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<NodeModel, Dictionary<int, INode>>();
                foreach (NodeModel topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));

                    i++;

                    //DynamoLogger.Instance.Log(topMost);
                }

                FScheme.Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);

                // inform any objects that a run has happened

                //DynamoLogger.Instance.Log(runningExpression);
#endif
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.CancelRun = false; //Reset cancel flag
                RunCancelled = false;

                //If we are forcing this, then make sure we don't run again either.
                if (ex.Force)
                    runAgain = false;

                OnRunCompleted(this, false);
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    DynamoLogger.Instance.Log(ex);
                }

                OnRunCancelled(true);

                //Reset the flags
                runAgain = false;
                RunCancelled = false;

                OnRunCompleted(this, false);

                if (IsTestMode)
                    Assert.Fail(ex.Message + ":" + ex.StackTrace);
            }
            finally
            {
                /* Post-evaluation cleanup */

                DynamoViewModel.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (CustomNodeDefinition def in dynSettings.FunctionWasEvaluated)
                    def.RequiresRecalc = false;

                
                //If we should run again...
                if (runAgain)
                {
                    //Reset flag
                    runAgain = false;

                    RunExpression(_showErrors);
                }
                else
                {
                    OnRunCompleted(this, true);
                }

                sw.Stop();
                DynamoLogger.Instance.Log(string.Format("Evaluation completed in {0}", sw.Elapsed.ToString()));
            }
        }

        protected virtual void Run()
        {
            //Print some stuff if we're in debug mode
            if (DynamoViewModel.RunInDebug)
            {
            }

            try
            {
                bool updated = EngineController.UpdateGraph();

                // Currently just use inefficient way to refresh preview values. 
                // After we switch to async call, only those nodes that are really 
                // updated in this execution session will be required to update 
                // preview value.
                if (updated)
                {
                    var nodes = DynamoViewModel.Model.HomeSpace.Nodes;
                    foreach (NodeModel node in nodes)
                    {
                        node.IsUpdated = true;
                    }
                }
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */
                OnRunCancelled(false);
                RunCancelled = false;
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                DynamoLogger.Instance.Log(ex);

                OnRunCancelled(true);
                RunCancelled = true;
                runAgain = false;

                //If we are testing, we need to throw an exception here
                //which will, in turn, throw an Assert.Fail in the 
                //Evaluation thread.
                if (IsTestMode)
                    throw new Exception(ex.Message);
            }

            OnEvaluationCompleted(this, EventArgs.Empty);
        }

        //protected virtual void Run(List<NodeModel> topElements, FScheme.Expression runningExpression)
        //{
        //    //Print some stuff if we're in debug mode
        //    if (DynamoViewModel.RunInDebug)
        //    {
        //        if (dynSettings.Controller.UIDispatcher != null)
        //        {
        //            foreach (string exp in topElements.Select(node => node.PrintExpression()))
        //                DynamoLogger.Instance.Log("> " + exp);
        //        }
        //    }

        //    try
        //    {
        //        //Evaluate the expression
        //        FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

        //        if (dynSettings.Controller.UIDispatcher != null)
        //        {
        //            //Print some more stuff if we're in debug mode
        //            if (DynamoViewModel.RunInDebug && expr != null)
        //            {
        //                DynamoLogger.Instance.Log("Evaluating the expression...");
        //                DynamoLogger.Instance.Log(FScheme.print(expr));
        //            }
        //        }
        //    }
        //    catch (CancelEvaluationException ex)
        //    {
        //        /* Evaluation was cancelled */

        //        OnRunCancelled(false);
        //        RunCancelled = false;
        //        if (ex.Force)
        //            runAgain = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        /* Evaluation failed due to error */

        //        DynamoLogger.Instance.Log(ex);

        //        OnRunCancelled(true);
        //        RunCancelled = true;
        //        runAgain = false;

        //        //If we are testing, we need to throw an exception here
        //        //which will, in turn, throw an Assert.Fail in the 
        //        //Evaluation thread.
        //        if (Testing)
        //            throw new Exception(ex.Message);
        //    }

        //    OnEvaluationCompleted(this, EventArgs.Empty);
        //}

        protected virtual void OnRunCancelled(bool error)
        {
            //DynamoLogger.Instance.Log("Run cancelled. Error: " + error);
            if (error)
                dynSettings.FunctionWasEvaluated.Clear();
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
                EngineController.Dispose();

            EngineController = new EngineController(this, true);
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
            var command = new DynCmd.RunCancelCommand(false, true);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanCancelRunCmd(object parameter)
        {
            return true;
        }

        public void RunExpression(object parameters) // For unit test cases.
        {
            RunExpression(Convert.ToBoolean(parameters));
        }

        internal void RunExprCmd(object parameters)
        {
            bool showErrors = Convert.ToBoolean(parameters);
            var command = new DynCmd.RunCancelCommand(showErrors, false);
            DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanRunExprCmd(object parameters)
        {
            return (dynSettings.Controller != null);
        }

        internal void RunCancelInternal(bool showErrors, bool cancelRun)
        {
            if (cancelRun != false)
                RunCancelled = true;
            else
                RunExpression(showErrors);
        }

        public void DisplayFunction(object parameters)
        {
            CustomNodeManager.GetFunctionDefinition((Guid)parameters);
        }

        internal bool CanDisplayFunction(object parameters)
        {
            var id = dynSettings.CustomNodes.FirstOrDefault(x => x.Value == (Guid)parameters).Value;

            if (id != null)
                return true;

            return false;
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
            DynamoLogger.Instance.ClearLog();
        }

        internal bool CanClearLog(object parameter)
        {
            return true;
        }

        private void AddPythonBindings()
        {
            try
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Assembly ironPythonAssembly = null;

                string path;

                if (File.Exists(path = Path.Combine(assemblyPath, "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }
                else if (File.Exists(path = Path.Combine(assemblyPath, "Packages", "IronPython", "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }

                if (ironPythonAssembly == null)
                    throw new Exception();

                var pythonEngine = ironPythonAssembly.GetType("DynamoPython.PythonEngine");

                var drawingField = pythonEngine.GetField("Drawing");
                var drawDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+DrawDelegate");
                Delegate draw = Delegate.CreateDelegate(
                    drawDelegateType,
                    this,
                    typeof(DynamoController)
                        .GetMethod("DrawPython", BindingFlags.NonPublic | BindingFlags.Instance));

                drawingField.SetValue(null, draw);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        void DrawPython(FScheme.Value val, string id)
        {
            //DrawContainers(val, id);
        }

        //private void DrawContainers(FScheme.Value val, string id)
        //{
        //    if (val.IsList)
        //    {
        //        foreach (FScheme.Value v in ((FScheme.Value.List)val).Item)
        //        {
        //            DrawContainers(v, id);
        //        }
        //    }
        //    if (val.IsContainer)
        //    {
        //        var drawable = ((FScheme.Value.Container)val).Item;

        //        if (drawable is GraphicItem)
        //        {
        //            VisualizationManager.Visualizations[id].Geometry.Add(drawable);
        //        }
        //    }
        //}
    }

    public class CancelEvaluationException : Exception
    {
        public bool Force;

        public CancelEvaluationException(bool force)
            : base("Run Cancelled")
        {
            Force = force;
        }
    }
    
    public class CrashPromptArgs : EventArgs
    {
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
