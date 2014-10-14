#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

using DSNodeServices;

using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Services;

#endregion

namespace Dynamo.Core
{
    /// <summary>
    ///     Class that handles requests to run Dynamo graph
    /// </summary>
    public class DynamoRunner : LogSourceBase
    {
        /// <summary>
        ///     Protect guard for setting the run flag
        /// </summary>
        protected static Object RunControlMutex = new object();

        private bool cancelSet;
        private int? execInternval;
        private Thread evaluationThread;

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

        public void RunExpression(HomeWorkspaceModel workspaceModel, EngineController engineController, bool isTestMode, int? executionInterval = null)
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
                    workspaceModel.PreloadedTraceData;
                workspaceModel.PreloadedTraceData = null; // Reset.

                engineController.LiveRunnerCore.SetTraceDataForNodes(traceData);

                //We are now considered running
                Running = true;
            }

            if (!isTestMode)
            {
                //Setup background worker
                OnRunStarted();

                //As we are the only place that is allowed to activate this, it is a trap door, so this is safe
                lock (RunControlMutex)
                {
                    Validity.Assert(Running);
                }
                RunAsync(workspaceModel, engineController);
            }
            else
            {
                //for testing, we do not want to run asynchronously, as it will finish the 
                //test before the evaluation (and the run) is complete
                //RunThread(evaluationWorker, new DoWorkEventArgs(executionInterval));
                RunSync(workspaceModel, engineController);
            }
        }

        public event Action RunStarted;
        protected virtual void OnRunStarted()
        {
            var handler = RunStarted;
            if (handler != null) handler();
        }

        private void RunAsync(HomeWorkspaceModel workspaceModel, EngineController engineController)
        {
            evaluationThread = new Thread(() => RunSync(workspaceModel, engineController));
            evaluationThread.Start();
        }

        private void RunSync(HomeWorkspaceModel workspaceModel, EngineController engineController)
        {
            do
            {
                Evaluate(workspaceModel, engineController);

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
        private void RunComplete()
        {
            lock (RunControlMutex)
            {
                Running = false;
                OnRunCompleted(false, cancelSet);
                cancelSet = false;
            }
        }

        public event Action<bool, bool> RunCompleted;
        protected virtual void OnRunCompleted(bool obj, bool obj2)
        {
            var handler = RunCompleted;
            if (handler != null) handler(obj, obj2);
        }

        protected virtual void Evaluate(HomeWorkspaceModel workspace, EngineController engineController)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();

                engineController.GenerateGraphSyncData(workspace.Nodes);

                //No additional work needed
                if (engineController.HasPendingGraphSyncData)
                {
                    ExecutionEvents.OnGraphPreExecution();
                    Eval(workspace, engineController);
                }
            }
            finally
            {
                sw.Stop();

                InstrumentationLogger.LogAnonymousEvent("Run", "Eval");
                InstrumentationLogger.LogAnonymousTimedEvent("Perf", "EvalTime", sw.Elapsed);

                Log(string.Format("Evaluation completed in {0}", sw.Elapsed));
            }

            OnEvaluationCompleted();
            ExecutionEvents.OnGraphPostExecution();
        }

        public event Action EvaluationCompleted;
        protected virtual void OnEvaluationCompleted()
        {
            var handler = EvaluationCompleted;
            if (handler != null) handler();
        }

        private void Eval(HomeWorkspaceModel workspaceModel, EngineController engineController)
        {
            // We have caught all possible exceptions in UpdateGraph call, I am 
            // not certain if this try-catch block is still meaningful or not.
            try
            {
                Exception fatalException;
                bool updated = engineController.UpdateGraph(out fatalException);

                if (fatalException != null)
                    OnExceptionOccurred(fatalException, true);
                
                // Currently just use inefficient way to refresh preview values. 
                // After we switch to async call, only those nodes that are really 
                // updated in this execution session will be required to update 
                // preview value.
                if (updated)
                {
                    ObservableCollection<NodeModel> nodes = workspaceModel.Nodes;
                    foreach (NodeModel node in nodes)
                        node.IsUpdated = true;
                }
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                Log(ex);

                //OnRunCancelled(true);
                OnExceptionOccurred(ex, false);
            }
        }

        public event Action<Exception, bool> ExceptionOccurred;
        protected virtual void OnExceptionOccurred(Exception obj, bool fatal)
        {
            var handler = ExceptionOccurred;
            if (handler != null) handler(obj, fatal);
        }

        //protected virtual void OnRunCancelled(bool error)
        //{
        //    //dynamoModel.DynamoLogger.Log("Run cancelled. Error: " + error);
        //}
    }
}
