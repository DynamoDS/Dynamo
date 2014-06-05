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
            IWatchHandler watchHandler, IPreferences preferences)
        {
            DebugSettings = new DebugSettings();

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

        internal void MutateTestInternal()
        {
            Random rand = new Random(1);
            //DebugSettings.VerboseLogging = true;

            String logTarget = dynSettings.DynamoLogger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            new Thread(() =>
                {
                    bool passed = false;

                    try
                    {

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            var nodes = DynamoModel.Nodes;

                            writer.WriteLine("### - Beginning eval");


                            UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);
                                    DynamoViewModel.ExecuteCommand(runCancel);

                                }));

                            while (DynamoViewModel.Controller.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }

                            writer.WriteLine("### - Eval complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning readout");


                            Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();

                            foreach (NodeModel n in nodes)
                            {
                                if (n.OutPorts.Count > 0)
                                {
                                    Guid guid = n.GUID;
                                    Object data = n.GetValue(0).Data;
                                    String val = data != null ? data.ToString() : "null";
                                    valueMap.Add(guid, val);
                                    writer.WriteLine(guid + " :: " + val);
                                    writer.Flush();

                                }
                            }

                            writer.WriteLine("### - Readout complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning delete");
                            NodeModel node = nodes[rand.Next(nodes.Count)];
                            writer.WriteLine("### - Deletion target: " + node.GUID);


                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                                {
                                    new CodeBlockNodeMutator(rand), new DeleteNodeMutator(rand)
                                };


                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            
                            int numberOfUndosNeeded = mutator.Mutate();


                            Thread.Sleep(100);

                            writer.WriteLine("### - delete complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning re-exec");


                            UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);
                                    DynamoViewModel.ExecuteCommand(runCancel);

                                }));

                            Thread.Sleep(100);

                            writer.WriteLine("### - re-exec complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning undo");


                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                
                                UIDispatcher.Invoke(new Action(() =>
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(
                                                DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        DynamoViewModel.ExecuteCommand(undoCommand);

                                    }));

                            Thread.Sleep(100);

                            }


                            writer.WriteLine("### - undo complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning re-exec");

                            UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);

                                    DynamoViewModel.ExecuteCommand(runCancel);

                                }));
                            Thread.Sleep(10);

                            while (DynamoViewModel.Controller.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }

                            writer.WriteLine("### - re-exec complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning readback");


                            foreach (NodeModel n in nodes)
                            {
                                if (n.OutPorts.Count > 0)
                                {
                                    try
                                    {



                                        String valmap = valueMap[n.GUID].ToString();
                                        Object data = n.GetValue(0).Data;
                                        String nodeVal = data != null ? data.ToString() : "null";

                                        if (valmap != nodeVal)
                                        {

                                            writer.WriteLine("!!!!!!!!!!! Read-back failed");
                                            writer.WriteLine(n.GUID);


                                            writer.WriteLine("Was: " + nodeVal);
                                            writer.WriteLine("Should have been: " + valmap);
                                            writer.Flush();
                                            return;


                                            Debug.WriteLine("==========> Failure on run: " + i);
                                            Debug.WriteLine("Lookup map failed to agree");
                                            Validity.Assert(false);

                                        }
                                    }
                                    catch (Exception)
                                    {
                                        writer.WriteLine("!!!!!!!!!!! Read-back failed");
                                        writer.Flush();
                                        return;
                                    }
                                }
                            }

                            /*
                        UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.ForceRunCancelCommand runCancelForce =
    new DynamoViewModel.ForceRunCancelCommand(false,
                                              false);
                        DynamoViewModel.ExecuteCommand(runCancelForce);
    
                            }));
                        while (DynamoViewModel.Controller.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                String nodeVal = n.GetValue(0).Data.ToString();

                                if (valmap != nodeVal)
                                {
                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);

                                }
                            }
                        }
                        */


                        }

                        passed = true;
                    }
                    finally
                    {
                        dynSettings.DynamoLogger.Log("Fuzz testing: " + (passed ? "pass" : "FAIL"));

                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }


                }).
                    Start();

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
    
}
