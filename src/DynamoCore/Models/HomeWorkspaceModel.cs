using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;
using DSNodeServices;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Services;
using Dynamo.Utilities;
using DynamoUtilities;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        private EngineController engineController;
        private readonly DynamoScheduler scheduler;
        
        public bool RunEnabled;
        public bool DynamicRunEnabled;

        public HomeWorkspaceModel(LibraryServices libraryServices, DynamoScheduler scheduler)
            : this(
                libraryServices,
                scheduler,
                Enumerable.Empty<KeyValuePair<Guid, List<string>>>(),
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<ConnectorModel>(),
                Enumerable.Empty<NoteModel>(),
                0,
                0) { }

        public HomeWorkspaceModel(
            LibraryServices libraryServices, DynamoScheduler scheduler,
            IEnumerable<KeyValuePair<Guid, List<string>>> traceData, IEnumerable<NodeModel> e,
            IEnumerable<ConnectorModel> c, IEnumerable<NoteModel> n, double x, double y)
            : base("Home", e, c, n, x, y)
        {
            PreloadedTraceData = traceData;
            this.scheduler = scheduler;

            engineController = new EngineController(
                libraryServices,
                DynamoPathManager.Instance.GeometryFactory);
            engineController.MessageLogged += Log;

            runExpressionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            runExpressionTimer.Tick += OnRunExpression;
        }

        public override void Dispose()
        {
            base.Dispose();
            engineController.MessageLogged -= Log;
            engineController.Dispose();
            runExpressionTimer.Stop();
            runExpressionTimer.Tick -= OnRunExpression;
        }

        private readonly DispatcherTimer runExpressionTimer;

        internal bool IsEvaluationPending
        {
            get
            {
                return runExpressionTimer != null && runExpressionTimer.IsEnabled;
            }
        }

        private void OnRunExpression(object sender, EventArgs e)
        {
// ReSharper disable once PossibleNullReferenceException
            (sender as DispatcherTimer).Stop();
            Run();
        }
        
        protected override void OnModified()
        {
            base.OnModified();

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check that shutdown has not been requested.
            //TODO(Steve): Move ShutdownRequested management to scheduler
            if (DynamicRunEnabled && !dynamoModel.ShutdownRequested)
            {
                // This dispatch timer is to avoid updating graph too frequently.
                // It happens when we are modifying a bunch of connections in 
                // a short time frame. E.g., when we delete some nodes with a 
                // bunch of connections, each deletion of connection will call 
                // RequestSync(). Or, when we are modifying the content in a code 
                // block. 
                // 
                // Each time when RequestSync() is called, runExpressionTimer will
                // be reset and until no RequestSync events flood in, the updating
                // of graph will get executed. 
                //
                // We use DispatcherTimer so that the update of graph happens on
                // the main UI thread.
                runExpressionTimer.Stop();
                runExpressionTimer.Start(); // reset timer
            }
        }

        protected override void ResetWorkspaceCore()
        {
            runExpressionTimer.Stop();
            base.ResetWorkspaceCore();
        }

        #region evaluation

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="definition"></param>
        public void RegisterCustomNodeDefinitionWithEngine(CustomNodeDefinition definition)
        {
            engineController.GenerateGraphSyncDataForCustomNode(Nodes, definition);
        }

        /// <summary>
        /// Call this method to reset the virtual machine, avoiding a race 
        /// condition by using a thread join inside the vm executive.
        /// TODO(Luke): Push this into a resync call with the engine controller
        /// </summary>
        /// <param name="customNodeDefinitions"></param>
        /// <param name="markNodesAsDirty">Set this parameter to true to force 
        ///     reset of the execution substrait. Note that setting this parameter 
        ///     to true will have a negative performance impact.</param>
        public virtual void ResetEngine(IEnumerable<CustomNodeDefinition> customNodeDefinitions, bool markNodesAsDirty = false)
        {
            ResetEngineInternal(customNodeDefinitions);
            if (markNodesAsDirty)
            {
                foreach (var node in Nodes)
                    node.ForceReExecuteOfNode = true;
                OnModified();
            }
        }

        private void ResetEngineInternal(IEnumerable<CustomNodeDefinition> customNodeDefinitions)
        {
            var libServices = engineController.LibraryServices;

            if (engineController != null)
            {
                engineController.Dispose();
                engineController = null;
            }

            var geomFactory = DynamoPathManager.Instance.GeometryFactory;
            engineController = new EngineController(libServices, geomFactory);
            
            foreach (var def in customNodeDefinitions)
                RegisterCustomNodeDefinitionWithEngine(def);
        }

        /// <summary>
        /// This callback method is invoked in the context of ISchedulerThread 
        /// when UpdateGraphAsyncTask is completed.
        /// </summary>
        /// <param name="task">The original UpdateGraphAsyncTask instance.</param>
        private void OnUpdateGraphCompleted(AsyncTask task)
        {
            var updateTask = task as UpdateGraphAsyncTask;
            var messages = new Dictionary<Guid, string>();

            // Runtime warnings take precedence over build warnings.
            foreach (var warning in updateTask.RuntimeWarnings)
            {
                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            foreach (var warning in updateTask.BuildWarnings)
            {
                // If there is already runtime warnings for 
                // this node, then ignore the build warnings.
                if (messages.ContainsKey(warning.Key))
                    continue;

                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            var workspace = updateTask.TargetedWorkspace;
            foreach (var message in messages)
            {
                var guid = message.Key;
                var node = workspace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node == null)
                    continue;

                node.Warning(message.Value); // Update node warning message.
            }

            // This method is guaranteed to be called in the context of 
            // ISchedulerThread (for Revit's case, it is the idle thread).
            // Dispatch the failure message display for execution on UI thread.
            // 
            if (task.Exception != null && (dynamoModel.IsTestMode == false))
            {
                Action showFailureMessage = () =>
                    Dynamo.Nodes.Utilities.DisplayEngineFailureMessage(dynamoModel, task.Exception);

                OnRequestDispatcherBeginInvoke(showFailureMessage);
            }

            // Refresh values of nodes that took part in update.
            foreach (var modifiedNode in updateTask.ModifiedNodes)
            {
                modifiedNode.RequestValueUpdateAsync(scheduler, engineController);
            }

            // Notify listeners (optional) of completion.
            RunEnabled = true; // Re-enable 'Run' button.

            // Notify handlers that evaluation took place.
            var e = new EvaluationCompletedEventArgs(true);
            OnEvaluationCompleted(this, e);
        }

        /// <summary>
        /// This method is typically called from the main application thread (as 
        /// a result of user actions such as button click or node UI changes) to
        /// schedule an update of the graph. This call may or may not represent 
        /// an actual update. In the event that the user action does not result 
        /// in actual graph update (e.g. moving of node on UI), the update task 
        /// will not be scheduled for execution.
        /// </summary>
        public void Run()
        {
            var traceData = PreloadedTraceData;
            if ((traceData != null) && traceData.Any())
            {
                // If we do have preloaded trace data, set it here first.
                var setTraceDataTask = new SetTraceDataAsyncTask(scheduler);
                if (setTraceDataTask.Initialize(engineController, this))
                    scheduler.ScheduleForExecution(setTraceDataTask);
            }

            // If one or more custom node have been updated, make sure they
            // are compiled first before the home workspace gets evaluated.
            // 
            engineController.ProcessPendingCustomNodeSyncData(scheduler);

            var task = new UpdateGraphAsyncTask(scheduler);
            if (task.Initialize(engineController, this))
            {
                task.Completed += OnUpdateGraphCompleted;
                RunEnabled = false; // Disable 'Run' button.
                scheduler.ScheduleForExecution(task);
            }
            else
            {
                // Notify handlers that evaluation did not take place.
                var e = new EvaluationCompletedEventArgs(false);
                OnEvaluationCompleted(this, e);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="customNodeDefinitions"></param>
        public void ForceRun(IEnumerable<CustomNodeDefinition> customNodeDefinitions)
        {
            Log("Beginning engine reset");
            ResetEngine(customNodeDefinitions);
            Log("Reset complete");

            Run();
        }

        #endregion
    }
}
