#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using DSNodeServices;

using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Services;

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

        private bool cancelSet;
        private int? execInternval;
        private Thread evaluationThread = null;

        public bool Running { get; protected set; }

        /// <summary>
        ///     Does the workflow need to be run again due to changes since the last run?
        /// </summary>
        public bool NeedsAdditionalRun { get; protected set; }

        public void CancelAsync(EngineController engineController)
        {
            if (Running)
            {
                cancelSet = true;
                engineController.LiveRunnerCore.RequestCancellation();
                
                // We need to wait for evaluation thread to complete after a cancellation
                // until the LR and Engine controller are reset properly
                if (evaluationThread != null)
                    evaluationThread.Join();
            }
        }

        public void RunExpression(HomeWorkspaceModel workspaceModel, int? executionInterval = null)
        {
            if (workspaceModel == null)
            {
                return;
            }

            var dynamoModel = workspaceModel.DynamoModel;

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
                    workspaceModel.PreloadedTraceData;
                workspaceModel.PreloadedTraceData = null; // Reset.

                if (dynamoModel == null || dynamoModel.EngineController == null)
                {
                    return;
                }

                dynamoModel.EngineController.LiveRunnerCore.SetTraceDataForNodes(traceData); 

                //We are now considered running
                Running = true;
            }

            if (!DynamoModel.IsTestMode)
            {
                //Setup background worker
                dynamoModel.RunEnabled = false;

                //As we are the only place that is allowed to activate this, it is a trap door, so this is safe
                lock (RunControlMutex)
                {
                    Validity.Assert(Running);
                }
                RunAsync(workspaceModel);
            }
            else
            {
                //for testing, we do not want to run asynchronously, as it will finish the 
                //test before the evaluation (and the run) is complete
                //RunThread(evaluationWorker, new DoWorkEventArgs(executionInterval));
                RunSync(workspaceModel);
            }
        }

        private void RunAsync(HomeWorkspaceModel workspaceModel)
        {
            evaluationThread = new Thread(() => RunSync(workspaceModel));
            evaluationThread.Start();
        }

        private void RunSync(HomeWorkspaceModel workspaceModel)
        {
            do
            {
                Evaluate(workspaceModel);

                if (execInternval == null)
                    break;

                int sleep = execInternval.Value;
                Thread.Sleep(sleep);
            } while (!cancelSet);

            RunComplete(workspaceModel);
        }

        /// <summary>
        ///     Method to group together all the tasks associated with an execution being complete
        /// </summary>
        private void RunComplete(HomeWorkspaceModel workspaceModel)
        {
            var dynamoModel = workspaceModel.DynamoModel;

            dynamoModel.OnRunCompleted(this, false);

            lock (RunControlMutex)
            {
                Running = false;
                dynamoModel.RunEnabled = true;
            }

            if (cancelSet)
            {
                dynamoModel.ResetEngine(true);
                cancelSet = false;
            }
        }

        protected virtual void Evaluate(HomeWorkspaceModel workspace)
        {
            var dynamoModel = workspace.DynamoModel;

            var sw = new Stopwatch();

            try
            {
                sw.Start();

                dynamoModel.EngineController.GenerateGraphSyncData(workspace.Nodes);

                if (dynamoModel.EngineController.HasPendingGraphSyncData)
                {
                    ExecutionEvents.OnGraphPreExecution();
                    Eval(workspace);
                }
            }
            catch (Exception ex)
            {
                //Catch unhandled exception
                if (ex.Message.Length > 0)
                    dynamoModel.Logger.Log(ex);

                OnRunCancelled(true);

                if (DynamoModel.IsTestMode) // Throw exception for NUnit.
                    throw new Exception(ex.Message + ":" + ex.StackTrace);
            }
            finally
            {
                sw.Stop();

                InstrumentationLogger.LogAnonymousEvent(/*NXLT*/"Run", /*NXLT*/"Eval");
                InstrumentationLogger.LogAnonymousTimedEvent(/*NXLT*/"Perf", /*NXLT*/"EvalTime", sw.Elapsed);

                dynamoModel.Logger.Log(
                    string.Format(Properties.Resources.EvaluationComleted, sw.Elapsed));
            }

            // When evaluation is completed, we mark all
            // nodes as ForceReexecuteOfNode = false to prevent
            // cyclical graph updates. It is therefore the responsibility 
            // of the node implementor to mark this flag = true, if they
            // want to require update.
            foreach (var n in workspace.Nodes)
            {
                n.ForceReExecuteOfNode = false;
            }

            // Notify handlers that evaluation took place.
            var e = new EvaluationCompletedEventArgs(true);
            dynamoModel.OnEvaluationCompleted(this, e);
            ExecutionEvents.OnGraphPostExecution();
        }

        private void Eval(HomeWorkspaceModel workspaceModel)
        {
            var dynamoModel = workspaceModel.DynamoModel;

            //Print some stuff if we're in debug mode
            if (dynamoModel.RunInDebug) { }

            // We have caught all possible exceptions in UpdateGraph call, I am 
            // not certain if this try-catch block is still meaningful or not.
            try
            {
                Exception fatalException = null;
                bool updated = dynamoModel.EngineController.UpdateGraph(ref fatalException);

                // If there's a fatal exception, show it to the user, unless of course 
                // if we're running in a unit-test, in which case there's no user. I'd 
                // like not to display the dialog and hold up the continuous integration.
                // 
                if (DynamoModel.IsTestMode == false && (fatalException != null))
                {
                    Action showFailureMessage =
                        () => Nodes.Utilities.DisplayEngineFailureMessage(dynamoModel, fatalException);

                    // The "Run" method is guaranteed to be called on a background 
                    // thread (for Revit's case, it is the idle thread). Here we 
                    // schedule the message to show up when the UI gets around and 
                    // handle it.
                    // 
                    dynamoModel.OnRequestDispatcherBeginInvoke(showFailureMessage);
                }

                // Currently just use inefficient way to refresh preview values. 
                // After we switch to async call, only those nodes that are really 
                // updated in this execution session will be required to update 
                // preview value.
                if (updated)
                {
                    ObservableCollection<NodeModel> nodes =
                        workspaceModel.Nodes;
                    foreach (NodeModel node in nodes)
                        node.IsUpdated = true;
                }
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                dynamoModel.Logger.Log(ex);

                OnRunCancelled(true);

                //If we are testing, we need to throw an exception here
                //which will, in turn, throw an Assert.Fail in the 
                //Evaluation thread.
                if (DynamoModel.IsTestMode)
                    throw new Exception(ex.Message);
            }
        }

        protected virtual void OnRunCancelled(bool error)
        {
            //dynamoModel.DynamoLogger.Log("Run cancelled. Error: " + error);
        }
    }
}
