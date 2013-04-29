using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.Utilties;

namespace Dynamo
{
    public class DynamoController
    {

        #region properties

        private readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        private readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        private readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        
        
        private bool isProcessingCommandQueue = false;

        public CustomNodeLoader CustomNodeLoader { get; internal set; }
        public SearchViewModel SearchViewModel { get; internal set; }
        public PackageManagerLoginViewModel PackageManagerLoginViewModel { get; internal set; }
        public PackageManagerPublishViewModel PackageManagerPublishViewModel { get; internal set; }
        public PackageManagerClient PackageManagerClient { get; internal set; }
        public DynamoViewModel DynamoViewModel { get; internal set; }

        List<dynModelBase> clipBoard = new List<dynModelBase>();
        public List<dynModelBase> ClipBoard
        {
            get { return clipBoard; }
            set { clipBoard = value; }
        }

        public bool IsProcessingCommandQueue
        {
            get { return isProcessingCommandQueue; }
        }

        public Queue<Tuple<object, object>> CommandQueue
        {
            get { return commandQueue; }
        }

        public dynBench Bench { get; private set; }

        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        public Dictionary<string, TypeLoadData> BuiltInTypesByName
        {
            get { return builtinTypesByTypeName; }
        }

        public ExecutionEnvironment FSchemeEnvironment { get; private set; }

        private bool _benchActivated;
        //public DynamoSplash SplashScreen { get; set; }

        #endregion

        #region Constructor and Initialization

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(ExecutionEnvironment env, bool withUI)
        {
            dynSettings.Controller = this;

            #warning MVVM: Moved to properties on dynBenchModelView
            //RunEnabled = true;
            //CanRunDynamically = true;

            #warning MVVM: don't construct the main window with a reference to the controller
            //Bench = new dynBench(this);

            #warning MVVM : create the view model to which the main window will bind
            //the DynamoModel is created therein
            DynamoViewModel = new DynamoViewModel(this);
            //DynamoCommands.ShowSplashScreenCmd.Execute(null); // closed in bench activated

            if (withUI)
            {
                Bench = new dynBench();
                dynSettings.Bench = Bench;
                Bench.DataContext = DynamoViewModel;
            }

            // custom node loader
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            CustomNodeLoader = new CustomNodeLoader(pluginsPath);

            SearchViewModel = new SearchViewModel();
            PackageManagerClient = new PackageManagerClient(this);
            PackageManagerLoginViewModel = new PackageManagerLoginViewModel(PackageManagerClient);
            PackageManagerPublishViewModel = new PackageManagerPublishViewModel(PackageManagerClient);

            FSchemeEnvironment = env;

            #warning MVVM : moved to proper view constructor on dynBench
            DynamoViewModel.CurrentOffset = new Point(dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);
            //Bench.CurrentOffset = new Point(dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);
            //Bench.InitializeComponent();

            dynSettings.Controller.DynamoViewModel.Log(String.Format(
                "Dynamo -- Build {0}.",
                Assembly.GetExecutingAssembly().GetName().Version));

            #warning MVVM : removed parameter bench
            DynamoLoader.LoadBuiltinTypes(SearchViewModel, this);//, Bench);
            DynamoLoader.LoadSamplesMenu(Bench);

            //Bench.settings_curves.IsChecked = true;
            //Bench.settings_curves.IsChecked = false;

            if (Bench != null)
            {
                //Bench.LockUI();

                //MVVM : callback has been restructured so that it sends a command back to the view model
                //Bench.Activated += OnBenchActivated;
                dynSettings.Workbench = Bench.WorkBench;
            }

            //run tests
            if (FScheme.RunTests(dynSettings.Controller.DynamoViewModel.Log))
            {
                if (Bench != null)
                    dynSettings.Controller.DynamoViewModel.Log("All Tests Passed. Core library loaded OK.");
            }
        }

        #endregion

        #region CommandQueue
    
        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            ProcessCommandQueue();
        }

        /// <summary>
        ///     Run all of the commands in the CommandQueue
        /// </summary>
        public void ProcessCommandQueue()
        {
            while (commandQueue.Count > 0)
            {
                var cmdData = commandQueue.Dequeue();
                var cmd = cmdData.Item1 as ICommand;
                if (cmd != null)
                {
                    if (cmd.CanExecute(cmdData.Item2))
                    {
                        cmd.Execute(cmdData.Item2);
                    }
                }
            }
            commandQueue.Clear();

            if (Bench != null)
            {
                DynamoLogger.Instance.Log(string.Format("Bench Thread : {0}",
                                                       Bench.Dispatcher.Thread.ManagedThreadId.ToString()));
            }
        }

        #endregion

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

            //Set run auto flag
            //this.DynamicRunEnabled = !showErrors;

            //Setup background worker
            var worker = new BackgroundWorker();
            worker.DoWork += EvaluationThread;

            //Disable Run Button

            //Bench.Dispatcher.Invoke(new Action(
            //   delegate { Bench.RunButton.IsEnabled = false; }
            //));

            DynamoViewModel.RunEnabled = false;

            //Let's start
            worker.RunWorkerAsync();
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            IEnumerable<dynNode> topElements = DynamoViewModel.Model.HomeSpace.GetTopMostNodes();

            //Mark the topmost as dirty/clean
            foreach (dynNode topMost in topElements)
                topMost.MarkDirty();

            //TODO: Flesh out error handling
            try
            {
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();
                foreach (dynNode topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));

                    i++;
                }
                

                FScheme.Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.CancelRun = false; //Reset cancel flag
                RunCancelled = true;

                //If we are forcing this, then make sure we don't run again either.
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { dynSettings.Controller.DynamoViewModel.Log(ex); }
                                                ));
                }

                OnRunCancelled(true);

                //Reset the flags
                runAgain = false;
                RunCancelled = true;
            }
            finally
            {
                /* Post-evaluation cleanup */

                //Re-enable run button
                //Bench.Dispatcher.Invoke(new Action(
                //   delegate
                //   {
                //       Bench.RunButton.IsEnabled = true;
                //   }
                //));

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

                    if (Bench != null)
                    {
                        //Run this method again from the main thread
                        Bench.Dispatcher.BeginInvoke(new Action(
                                                         delegate { RunExpression(_showErrors); }
                                                         ));
                    }
                }
            }
        }

        protected internal virtual void Run(IEnumerable<dynNode> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (DynamoViewModel.RunInDebug)
            {
                if (Bench != null)
                {
                    //string exp = FScheme.print(runningExpression);
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate
                                                    {
                                                        foreach (dynNode node in topElements)
                                                        {
                                                            string exp = node.PrintExpression();
                                                            dynSettings.Controller.DynamoViewModel.Log("> " + exp);
                                                        }
                                                    }
                                                ));
                }
            }

            try
            {
                //Evaluate the expression
                FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

                if (Bench != null)
                {
                    //Print some more stuff if we're in debug mode
                    if (DynamoViewModel.RunInDebug && expr != null)
                    {
                        Bench.Dispatcher.Invoke(new Action(
                                                    () =>
                                                    dynSettings.Controller.DynamoViewModel.Log(FScheme.print(expr))
                                                    ));
                    }
                }
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.RunCancelled = false;
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                if (Bench != null)
                {
                    //Print unhandled exception
                    if (ex.Message.Length > 0)
                    {
                        Bench.Dispatcher.Invoke(new Action(
                                                    delegate { dynSettings.Controller.DynamoViewModel.Log(ex); }
                                                    ));
                    }
                }

                OnRunCancelled(true);
                RunCancelled = true;
                runAgain = false;
            }

            OnEvaluationCompleted();
        }

        protected virtual void OnRunCancelled(bool error)
        {
            if (error)
                dynSettings.FunctionWasEvaluated.Clear();
        }

        protected virtual void OnEvaluationCompleted()
        {
        }

        

        #endregion
    }
}
