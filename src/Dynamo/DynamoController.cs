using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dynamo.Commands;
using Dynamo.Connectors;
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

        public readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        public readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        private readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        private string UnlockLoadPath;
        
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
        public DynamoController(ExecutionEnvironment env)
        {
            dynSettings.Controller = this;

            //MVVM: Moved to properties on dynBenchModelView
            //RunEnabled = true;
            //CanRunDynamically = true;

            //MVVM: don't construct the main window with a reference to the controller
            //Bench = new dynBench(this);
            Bench = new dynBench();

            //DynamoCommands.ShowSplashScreenCmd.Execute(null); // closed in bench activated
            dynSettings.Bench = Bench;

            //MVVM : create the view model to which the main window will bind
            //the DynamoModel is created therein
            DynamoViewModel = new DynamoViewModel(this);
            Bench.DataContext = DynamoViewModel;

            // custom node loader
            //string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string pluginsPath = Path.Combine(directory, "definitions");

            //CustomNodeLoader = new CustomNodeLoader(pluginsPath);

            SearchViewModel = new SearchViewModel();
            PackageManagerClient = new PackageManagerClient(this);
            PackageManagerLoginViewModel = new PackageManagerLoginViewModel(PackageManagerClient);
            PackageManagerPublishViewModel = new PackageManagerPublishViewModel(PackageManagerClient);

            FSchemeEnvironment = env;

            Bench.CurrentOffset = new Point(dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);

            Bench.InitializeComponent();
            dynSettings.Controller.DynamoViewModel.Log(String.Format(
                "Dynamo -- Build {0}.",
                Assembly.GetExecutingAssembly().GetName().Version));

            DynamoLoader.LoadBuiltinTypes(SearchViewModel, this, Bench);
            DynamoLoader.LoadSamplesMenu(Bench);

            //Bench.settings_curves.IsChecked = true;
            //Bench.settings_curves.IsChecked = false;

            Bench.LockUI();

            Bench.Activated += OnBenchActivated;
            dynSettings.Workbench = Bench.WorkBench;

            //run tests
            if (FScheme.RunTests(dynSettings.Controller.DynamoViewModel.Log))
            {
                if (Bench != null)
                    dynSettings.Controller.DynamoViewModel.Log("All Tests Passed. Core library loaded OK.");
            }
        }

        /// <summary>
        ///     This callback is executed when the BenchUI has completed activation.
        /// </summary>
        /// <parameter>The sender (presumably the Bench) </parameter>
        /// <parameter>Any arguments it passes</parameter>
        private void OnBenchActivated(object sender, EventArgs e)
        {
            if (!_benchActivated)
            {
                _benchActivated = true;

                DynamoLoader.LoadCustomNodes(dynSettings.Bench);

                dynSettings.Controller.DynamoViewModel.Log("Welcome to Dynamo!");

                if (UnlockLoadPath != null && !OpenWorkbench(UnlockLoadPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    dynSettings.Controller.DynamoViewModel.Log("Workbench could not be opened.");

                    //dynSettings.Writer.WriteLine("Workbench could not be opened.");
                    //dynSettings.Writer.WriteLine(UnlockLoadPath);

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(UnlockLoadPath);
                    }
                }

                UnlockLoadPath = null;

                Bench.UnlockUI();
                DynamoCommands.ShowSearchCmd.Execute(null);

                HomeSpace.OnDisplayed();

                DynamoCommands.CloseSplashScreenCmd.Execute(null); // closed in bench activated
                Bench.WorkBench.Visibility = Visibility.Visible;
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
                dynSettings.Writer.WriteLine(string.Format("Bench Thread : {0}",
                                                       Bench.Dispatcher.Thread.ManagedThreadId.ToString()));
            }
        }

        /// <summary>
        ///     Sets the load path
        /// </summary>
        internal void QueueLoad(string path)
        {
            UnlockLoadPath = path;
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

            this.RunEnabled = false;

            //Let's start
            worker.RunWorkerAsync();
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            IEnumerable<dynNode> topElements = HomeSpace.GetTopMostNodes();

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

                this.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (FunctionDefinition def in dynSettings.FunctionWasEvaluated)
                    def.RequiresRecalc = false;

                //If we should run again...
                if (runAgain)
                {
                    //Reset flag
                    runAgain = false;

                    //Run this method again from the main thread
                    Bench.Dispatcher.BeginInvoke(new Action(
                                                     delegate { RunExpression(_showErrors); }
                                                     ));
                }
            }
        }

        protected internal virtual void Run(IEnumerable<dynNode> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (debug)
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

            try
            {
                //Evaluate the expression
                FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

                //Print some more stuff if we're in debug mode
                if (debug && expr != null)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                () => dynSettings.Controller.DynamoViewModel.Log(FScheme.print(expr))
                                                ));
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

                //Print unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { dynSettings.Controller.DynamoViewModel.Log(ex); }
                                                ));
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

        internal void ShowElement(dynNode e)
        {
            if (dynamicRun)
                return;

            if (!Nodes.Contains(e))
            {
                if (HomeSpace != null && HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    ViewHomeWorkspace();
                }
                else
                {
                    foreach (FunctionDefinition funcDef in dynSettings.FunctionDict.Values)
                    {
                        if (funcDef.Workspace.Nodes.Contains(e))
                        {
                            ViewCustomNodeWorkspace(funcDef);
                            break;
                        }
                    }
                }
            }

            Bench.CenterViewOnElement(e.NodeUI);
        }

        #endregion
    }
}
