using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Dynamo.DSEngine;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.Practices.Prism.ViewModel;
using NUnit.Framework;
using String = System.String;

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
        #region properties

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
        private Dictionary<Guid, RenderDescription> _renderDescriptions = new Dictionary<Guid, RenderDescription>();
        public Dictionary<Guid, RenderDescription> RenderDescriptions
        {
            get { return _renderDescriptions; }
        }

        private readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        private readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        private bool testing = false;

        public CustomNodeManager CustomNodeManager { get; internal set; }
        public SearchViewModel SearchViewModel { get; internal set; }
        public DynamoViewModel DynamoViewModel { get; internal set; }
        public DynamoModel DynamoModel { get; set; }
        public Dispatcher UIDispatcher { get; set; }
        
        /// <summary>
        /// Testing flag is used to defer calls to run in the idle thread
        /// with the assumption that the entire test will be wrapped in an
        /// idle thread call.
        /// </summary>
        public bool Testing 
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

        public ExecutionEnvironment FSchemeEnvironment { get; private set; }

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

        private ConnectorType _connectorType;
        public ConnectorType ConnectorType
        {
            get { return _connectorType; }
            set
            {
                _connectorType = value;
            }
        }

        #endregion

        #region events

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

        public event EventHandler NodeSubmittedForRendering;
        public virtual void OnNodeSubmittedForRendering(object sender, EventArgs e)
        {
            if (NodeSubmittedForRendering != null)
                NodeSubmittedForRendering(sender, e);
        }

        public event EventHandler NodeRemovedFromRendering;
        public virtual void OnNodeRemovedFromRendering(object sender, EventArgs e)
        {
            if (NodeRemovedFromRendering != null)
                NodeRemovedFromRendering(sender, e);
        }

        public event DispatchedToUIThreadHandler DispatchedToUI;
        public void OnDispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            if (DispatchedToUI != null)
                DispatchedToUI(this, e);
        }

        public delegate void CrashPromptHandler(object sender, DispatcherUnhandledExceptionEventArgs e);
        public event CrashPromptHandler RequestsCrashPrompt;
        public void OnRequestsCrashPrompt(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (RequestsCrashPrompt != null)
                RequestsCrashPrompt(this, e);
        }

        #endregion

        #region Constructor and Initialization

        public static DynamoController MakeSandbox()
        {
            return new DynamoController(new ExecutionEnvironment(), typeof (DynamoViewModel), "None");
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(ExecutionEnvironment env, Type viewModelType, string context)
        {
            dynSettings.Controller = this;

            this.Context = context;

            //Start heartbeat reporting
            Services.InstrumentationLogger.Start();

            //create the view model to which the main window will bind
            //the DynamoModel is created therein
            this.DynamoViewModel = (DynamoViewModel)Activator.CreateInstance(viewModelType,new object[]{this});

            // custom node loader
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            CustomNodeManager = new CustomNodeManager(pluginsPath);
            
            SearchViewModel = new SearchViewModel();

            dynSettings.PackageLoader = new PackageLoader();

            dynSettings.PackageLoader.DoCachedPackageUninstalls();
            dynSettings.PackageLoader.LoadPackages();
            
            FSchemeEnvironment = env;

            DynamoViewModel.Model.CurrentWorkspace.X = 0;
            DynamoViewModel.Model.CurrentWorkspace.Y = 0;

            DynamoLogger.Instance.Log(String.Format(
                "Dynamo -- Build {0}",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.ClearCachedAssemblies();
            DynamoLoader.LoadBuiltinTypes();

            //run tests
            if (FScheme.RunTests(DynamoLogger.Instance.Log))
            {
                DynamoLogger.Instance.Log("All Tests Passed. Core library loaded OK.");
            }

            NodeSubmittedForRendering += new EventHandler(Controller_NodeSubmittedForRendering);
            NodeRemovedFromRendering += new EventHandler(Controller_NodeRemovedFromRendering);

        }

        #endregion

        public virtual void ShutDown()
        {
            dynSettings.Controller = null;
            Selection.DynamoSelection.Instance.ClearSelection();
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
            //If we're already running, do nothing.
            if (Running)
                return;

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
            //Get our entry points (elements with nothing connected to output)
            List<NodeModel> topElements = DynamoViewModel.Model.HomeSpace.GetTopMostNodes().ToList();

            //Mark the topmost as dirty/clean
            foreach (NodeModel topMost in topElements)
            {
                /*
                AstBuilder builder = new AstBuilder();
                topMost.CompileToAstNode(builder);
                builder.Execute();
                */

                topMost.MarkDirty();
            }

            try
            {
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<NodeModel, Dictionary<int, INode>>();
                foreach (NodeModel topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));

                    i++;

                    DynamoLogger.Instance.Log(topMost);
                }

                FScheme.Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);

                // inform any objects that a run has happened

                DynamoLogger.Instance.Log(runningExpression);
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

                if (Testing)
                    Assert.Fail(ex.Message + ":" + ex.StackTrace);
            }
            finally
            {
                /* Post-evaluation cleanup */

                DynamoViewModel.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (FunctionDefinition def in dynSettings.FunctionWasEvaluated)
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
            }
        }

        protected virtual void Run(List<NodeModel> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (DynamoViewModel.RunInDebug)
            {
                if (dynSettings.Controller.UIDispatcher != null)
                {
                    foreach (string exp in topElements.Select(node => node.PrintExpression()))
                        DynamoLogger.Instance.Log("> " + exp);
                }
            }

            try
            {
                DynamoLogger.Instance.Log("Evaluating the expression...");

                //Evaluate the expression
                FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

                if (dynSettings.Controller.UIDispatcher != null)
                {
                    //Print some more stuff if we're in debug mode
                    if (DynamoViewModel.RunInDebug && expr != null)
                    {
                        DynamoLogger.Instance.Log(FScheme.print(expr));
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

                if (dynSettings.Controller.UIDispatcher != null)
                {
                    //Print unhandled exception
                    if (ex.Message.Length > 0)
                    {
                        dynSettings.Controller.DispatchOnUIThread(() => DynamoLogger.Instance.Log(ex));
                    }
                }

                OnRunCancelled(true);
                RunCancelled = true;
                runAgain = false;

                //If we are testing, we need to throw an exception here
                //which will, in turn, throw an Assert.Fail in the 
                //Evaluation thread.
                if (Testing)
                    throw new Exception(ex.Message);
            }

            OnEvaluationCompleted();
        }

        protected virtual void OnRunCancelled(bool error)
        {
            //DynamoLogger.Instance.Log("Run cancelled. Error: " + error);
            if (error)
                dynSettings.FunctionWasEvaluated.Clear();
        }

        protected virtual void OnEvaluationCompleted()
        {
        }

        /// <summary>
        /// Callback for node being unregistered from rendering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Controller_NodeRemovedFromRendering(object sender, EventArgs e)
        {
            var node = sender as NodeModel;
            if (_renderDescriptions.ContainsKey(node.GUID))
                _renderDescriptions.Remove(node.GUID);
        }

        /// <summary>
        /// Callback for the node being registered for rendering.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Controller_NodeSubmittedForRendering(object sender, EventArgs e)
        {
            var node = sender as NodeModel;
            if (!_renderDescriptions.ContainsKey(node.GUID))
            {
                //don't allow an empty render description
                IDrawable d = node as IDrawable;
                if (d.RenderDescription == null)
                    d.RenderDescription = new RenderDescription();
                _renderDescriptions.Add(node.GUID, d.RenderDescription);
            }

        }

    #endregion

        public void RequestRedraw()
        {
            OnRequestsRedraw(this, EventArgs.Empty);
        }

        public void RequestClearDrawables()
        {
            var drawables = DynamoModel.Nodes.Where(x => x is IDrawable);
            drawables.ToList().ForEach(x=>((IDrawable)x).RenderDescription.ClearAll());
        }

        /// <summary>
        /// Called by nodes for behavior that they want to dispatch on the UI thread
        /// Triggers event to be received by the UI. If no UI exists, behavior will not be executed.
        /// </summary>
        /// <param name="a"></param>
        public void DispatchOnUIThread(Action a)
        {
            OnDispatchedToUI(this, new UIDispatcherEventArgs(a));
        }

        public void CancelRun(object parameter)
        {
            RunCancelled = true;
        }

        internal bool CanCancelRun(object parameter)
        {
            return true;
        }

        public void RunExpression(object parameters)
        {
            RunExpression(Convert.ToBoolean(parameters));
        }

        internal bool CanRunExpression(object parameters)
        {
            if (dynSettings.Controller == null)
            {
                return false;
            }
            return true;
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
            Process.Start("https://github.com/ikeough/Dynamo/issues?state=open");
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

}
