#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

using DSNodeServices;

using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;

#endregion

namespace Dynamo.Core
{
    /// <summary>
    ///     Class that handles requests to run Dynamo graph
    /// </summary>
    public class DynamoRunner
    {
        /// <summary>
        ///     Protect guard for setting the run flag
        /// </summary>
        protected static Object RunControlMutex = new object();

        private readonly DynamoController controller = dynSettings.Controller;

        private bool cancelSet;
        private int? execInternval;

        public bool Running { get; protected set; }

        /// <summary>
        ///     Does the workflow need to be run again due to changes since the last run?
        /// </summary>
        public bool NeedsAdditionalRun { get; protected set; }

        public void CancelAsync()
        {
            cancelSet = true;
        }


        public void RunExpression(int? executionInterval = null)
        {
            execInternval = executionInterval;

            lock (RunControlMutex)
            {
                if (Running)
                {
                    //We're already running, so we might need an additional run
                    NeedsAdditionalRun = true;
                    return;
                }
                //We're new so if we needed an additional run, this one counts
                NeedsAdditionalRun = false;

                // If there is preloaded trace data, send that along to the current
                // LiveRunner instance. Here we make sure it is done exactly once 
                // by resetting WorkspaceModel.PreloadedTraceData property after it 
                // is obtained.
                // 
                IEnumerable<KeyValuePair<Guid, List<string>>> traceData =
                    controller.DynamoViewModel.Model.HomeSpace.PreloadedTraceData;
                controller.DynamoViewModel.Model.HomeSpace.PreloadedTraceData = null; // Reset.
                controller.EngineController.LiveRunnerCore.SetTraceDataForNodes(traceData);

                //We are now considered running
                Running = true;
            }

            if (!DynamoController.IsTestMode)
            {
                //Setup background worker
                controller.DynamoViewModel.RunEnabled = false;

                //As we are the only place that is allowed to activate this, it is a trap door, so this is safe
                lock (RunControlMutex)
                {
                    Validity.Assert(Running);
                }
                RunAsync();
            }
            else
            {
                //for testing, we do not want to run asynchronously, as it will finish the 
                //test before the evaluation (and the run) is complete
                //RunThread(evaluationWorker, new DoWorkEventArgs(executionInterval));
                RunSync();
            }
        }

        private void RunAsync()
        {
            new Thread(RunSync).Start();
        }

        private void RunSync()
        {
            do
            {
                Evaluate();

                if (execInternval == null)
                    break;

                int sleep = execInternval.Value;
                Thread.Sleep(sleep);
            } while (!cancelSet);

            RunComplete();
        }

        /// <summary>
        ///     Method to group together all the tasks associated with an execution being complete
        /// </summary>
        public void RunComplete()
        {
            controller.OnRunCompleted(this, false);

            lock (RunControlMutex)
            {
                Running = false;
                controller.DynamoViewModel.RunEnabled = true;
            }
        }


        protected virtual void Evaluate()
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();

                controller.EngineController.GenerateGraphSyncData(
                    controller.DynamoViewModel.Model.HomeSpace.Nodes);

                //No additional work needed
                if (controller.EngineController.HasPendingGraphSyncData)
                    Eval();
            }
            catch (Exception ex)
            {
                //Catch unhandled exception
                if (ex.Message.Length > 0)
                    dynSettings.DynamoLogger.Log(ex);

                OnRunCancelled(true);

                if (DynamoController.IsTestMode) // Throw exception for NUnit.
                    throw new Exception(ex.Message + ":" + ex.StackTrace);
            }
            finally
            {
                sw.Stop();

                InstrumentationLogger.LogAnonymousEvent("Run", "Eval");
                InstrumentationLogger.LogAnonymousTimedEvent("Perf", "EvalTime", sw.Elapsed); 

                dynSettings.DynamoLogger.Log(
                    string.Format("Evaluation completed in {0}", sw.Elapsed));
            }

            controller.OnEvaluationCompleted(this, EventArgs.Empty);
        }

        private void Eval()
        {
            //Print some stuff if we're in debug mode
            if (controller.DynamoViewModel.RunInDebug) { }

            // We have caught all possible exceptions in UpdateGraph call, I am 
            // not certain if this try-catch block is still meaningful or not.
            try
            {
                Exception fatalException = null;
                bool updated = controller.EngineController.UpdateGraph(ref fatalException);

                // If there's a fatal exception, show it to the user, unless of course 
                // if we're running in a unit-test, in which case there's no user. I'd 
                // like not to display the dialog and hold up the continuous integration.
                // 
                if (DynamoController.IsTestMode == false && (fatalException != null))
                {
                    Action showFailureMessage =
                        () => Nodes.Utilities.DisplayEngineFailureMessage(fatalException);

                    // The "Run" method is guaranteed to be called on a background 
                    // thread (for Revit's case, it is the idle thread). Here we 
                    // schedule the message to show up when the UI gets around and 
                    // handle it.
                    // 
                    if (controller.UIDispatcher != null)
                        controller.UIDispatcher.BeginInvoke(showFailureMessage);
                }

                // Currently just use inefficient way to refresh preview values. 
                // After we switch to async call, only those nodes that are really 
                // updated in this execution session will be required to update 
                // preview value.
                if (updated)
                {
                    ObservableCollection<NodeModel> nodes =
                        controller.DynamoViewModel.Model.HomeSpace.Nodes;
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
                if (DynamoController.IsTestMode)
                    throw new Exception(ex.Message);
            }
        }

        protected virtual void OnRunCancelled(bool error)
        {
            //dynSettings.Controller.DynamoLogger.Log("Run cancelled. Error: " + error);
        }
    }
}
