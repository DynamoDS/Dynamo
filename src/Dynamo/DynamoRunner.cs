using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo
{
    public class DynamoRunner
    {
        /// <summary>
        ///     The core DynamoModel object - set in the constructor
        /// </summary>
        public DynamoModel DynamoModel { get; private set; }

        /// <summary>
        ///     The FScheme environment, storing all symbols
        /// </summary>
        public ExecutionEnvironment FSchemeEnv { get; private set; }

        /// <summary>
        ///     A delegate providing the interface for run completion
        ///     observers
        /// </summary>
        /// <param name="runner">The runner object</param>
        /// <param name="success">Whether the run was a success or not</param>
        public delegate void RunCompletedHandler(object runner, bool success);

        /// <summary>
        ///     An event executed when a run completes - it is null if nothing is assigned
        /// </summary>
        public event DynamoController.RunCompletedHandler RunCompleted;

        /// <summary>
        ///     A minimal wrapper on the RunCompleted event that checks if there are observers
        ///     and executes them if necessary  
        /// </summary>
        /// <param name="sender">The caller object.  This could be null. </param>
        /// <param name="success">Whether the run was a success</param>
        public virtual void OnRunCompleted(object sender, bool success)
        {
            if (RunCompleted != null)
                RunCompleted(sender, success);
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        /// <param name="dynamoModel"> The parent DynamoModel object</param>
        /// <param name="fschemeEnv"> The enviro.  Should be able to get rid of this. </param>
        public DynamoRunner(DynamoModel dynamoModel, ExecutionEnvironment fschemeEnv)
        {
            this.DynamoModel = dynamoModel;
            this.FSchemeEnv = fschemeEnv;
        }

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

            //dynSettings.Bench.Dispatcher.Invoke(new Action(
            //   delegate { dynSettings.Bench.RunButton.IsEnabled = false; }
            //));

            DynamoModel.RunEnabled = false;

            //Let's start
            worker.RunWorkerAsync();
        }


        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            IEnumerable<dynNodeModel> topElements = DynamoModel.HomeSpace.GetTopMostNodes();

            //Mark the topmost as dirty/clean
            foreach (dynNodeModel topMost in topElements)
                topMost.MarkDirty();

            //TODO: Flesh out error handling
            try
            {
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<dynNodeModel, Dictionary<int, INode>>();
                foreach (dynNodeModel topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));

                    i++;
                }

                FScheme.Expression runningExpression = topNode.Compile();

                Run(DynamoModel.RunInDebug, topElements, runningExpression);
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

                OnRunCompleted(this, false);
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    dynSettings.Controller.DynamoViewModel.Log(ex);
                }

                OnRunCancelled(true);

                //Reset the flags
                runAgain = false;
                RunCancelled = true;

                OnRunCompleted(this, false);
            }
            finally
            {
                /* Post-evaluation cleanup */

                //Re-enable run button
                //dynSettings.Bench.Dispatcher.Invoke(new Action(
                //   delegate
                //   {
                //       dynSettings.Bench.RunButton.IsEnabled = true;
                //   }
                //));

                DynamoModel.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (FunctionDefinition def in dynSettings.FunctionWasEvaluated)
                    def.RequiresRecalc = false;


                //If we should run again...
                if (runAgain)
                {
                    //Reset flag
                    runAgain = false;

                    if (dynSettings.Bench != null)
                    {
                        //Run this method again from the main thread
                        dynSettings.Bench.Dispatcher.BeginInvoke(new Action(
                                                                     delegate { RunExpression(_showErrors); }
                                                                     ));
                    }
                }
                else
                {
                    OnRunCompleted(this, true);
                }
            }
        }

        protected internal virtual void Run(bool RunInDebug, IEnumerable<dynNodeModel> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (RunInDebug)
            {
// NOPE
                //if (dynSettings.Bench != null)
                //{
                //    //string exp = FScheme.print(runningExpression);
                //    //dynSettings.Bench.Dispatcher.Invoke(new Action(
                //    //                            delegate
                //    //                            {
                //    //                                foreach (dynNodeModel node in topElements)
                //    //                                {
                //    //                                    string exp = node.PrintExpression();
                //    //                                    dynSettings.Controller.DynamoViewModel.Log("> " + exp);
                //    //                                }
                //    //                            }
                //    //                            ));
                //}
            }

            try
            {
                //Evaluate the expression
                FScheme.Value expr = this.FSchemeEnv.Evaluate(runningExpression);

                if (dynSettings.Bench != null)
                {
                    //Print some more stuff if we're in debug mode
                    if (RunInDebug && expr != null)
                    {
// NOPE
                        //dynSettings.Bench.Dispatcher.Invoke(new Action(
                        //                            () =>
                        //                            dynSettings.Controller.DynamoViewModel.Log(FScheme.print(expr))
                        //                            ));
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
// NOPE
                //if (dynSettings.Bench != null)
                //{
                //    //Print unhandled exception
                //    if (ex.Message.Length > 0)
                //    {
                //        dynSettings.Bench.Dispatcher.Invoke(new Action(
                //                                    delegate { dynSettings.Controller.DynamoViewModel.Log(ex); }
                //                                    ));
                //    }
                //}

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

    }
}
