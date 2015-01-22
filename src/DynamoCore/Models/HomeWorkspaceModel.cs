﻿using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using ProtoCore.Namespace;

namespace Dynamo.Models
{
    public class HomeWorkspaceModel : WorkspaceModel
    {
        public EngineController EngineController { get; private set; }
        private readonly DynamoScheduler scheduler;

        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                if (Equals(value, runEnabled)) return;
                runEnabled = value;
                RaisePropertyChanged("RunEnabled");
            }
        }

        public bool DynamicRunEnabled;

        public readonly bool VerboseLogging;

        public HomeWorkspaceModel(EngineController engine, DynamoScheduler scheduler, 
            NodeFactory factory, bool verboseLogging, bool isTestMode, string fileName="")
            : this(
                engine,
                scheduler,
                factory,
                Enumerable.Empty<KeyValuePair<Guid, List<string>>>(),
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                0,
                0, verboseLogging, isTestMode, new ElementResolver(), fileName) { }

        public HomeWorkspaceModel(
            EngineController engine, DynamoScheduler scheduler, NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<string>>> traceData, IEnumerable<NodeModel> e, IEnumerable<NoteModel> n, 
            double x, double y, bool verboseLogging,
            bool isTestMode, ElementResolver elementResolver, string fileName = "")
            : base("Home", e, n, x, y, factory, elementResolver, fileName)
        {
            RunEnabled = true;
            PreloadedTraceData = traceData;
            this.scheduler = scheduler;
            VerboseLogging = verboseLogging;

            ResetEngine(engine);

            IsTestMode = isTestMode;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (EngineController != null)
            {
                EngineController.MessageLogged -= Log;
                EngineController.LibraryServices.LibraryLoaded -= LibraryLoaded;
            }
        }

        /// <summary>
        /// This does not belong here, period. It is here simply because there is 
        /// currently no better place to put it. A DYN file is loaded by DynamoModel,
        /// subsequently populating WorkspaceModel, along the way, the trace data 
        /// gets preloaded with the file. The best place for this cached data is in 
        /// the EngineController (or even LiveRunner), but the engine gets reset in 
        /// a rather nondeterministic way (for example, when Revit idle thread 
        /// decides it is time to execute a pre-scheduled engine reset). And it gets 
        /// done more than once during file open. So that's out. The second best 
        /// place to store this information is then the WorkspaceModel, where file 
        /// loading is SUPPOSED TO BE done. As of now we let DynamoModel sets the 
        /// loaded data (since it deals with loading DYN file), but in near future,
        /// the file loading mechanism will be completely moved into WorkspaceModel,
        /// that's the time we removed this property setter below.
        /// </summary>
        internal IEnumerable<KeyValuePair<Guid, List<string>>> PreloadedTraceData
        {
            get
            {
                return preloadedTraceData;
            }

            set
            {
                if (value != null && (preloadedTraceData != null))
                {
                    const string message = "PreloadedTraceData cannot be set twice";
                    throw new InvalidOperationException(message);
                }

                preloadedTraceData = value;
            }
        }
        private IEnumerable<KeyValuePair<Guid, List<string>>> preloadedTraceData;

        private bool runEnabled;

        internal bool IsEvaluationPending
        {
            get
            {
                return false;
            }
        }

        protected override void OnNodeRemoved(NodeModel node)
        {
            base.OnNodeRemoved(node);
            EngineController.NodeDeleted(node);
        }

        protected override void ResetWorkspaceCore()
        {
            // Reset Run Automatic option to false on resetting the workspace
#if DEBUG
            DynamicRunEnabled = true;
#else
            DynamicRunEnabled = false;
#endif
        }

        private void LibraryLoaded(object sender, LibraryServices.LibraryLoadedEventArgs e)
        {
            // Mark all nodes as dirty so that AST for the whole graph will be
            // regenerated.
            foreach (var node in Nodes)
            {
                node.MarkNodeAsModified();
            }
            OnNodesModified();
        }

        public override void OnNodesModified()
        {
            base.OnNodesModified();

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check that shutdown has not been requested.
            if (DynamicRunEnabled && EngineController != null)
            {
                DynamoModel.OnRequestDispatcherBeginInvoke(Run);
            }
        }

        /// <summary>
        ///     Clears this workspace of nodes, notes, and connectors.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            PreloadedTraceData = null;
        }

        #region evaluation
        /// <summary>
        /// Call this method to reset the virtual machine, avoiding a race 
        /// condition by using a thread join inside the vm executive.
        /// TODO(Luke): Push this into a resync call with the engine controller
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="markNodesAsDirty">Set this parameter to true to force 
        ///     reset of the execution substrait. Note that setting this parameter 
        ///     to true will have a negative performance impact.</param>
        public void ResetEngine(EngineController controller, bool markNodesAsDirty = false)
        {
            if (EngineController != null)
            {
                EngineController.MessageLogged -= Log;
                EngineController.LibraryServices.LibraryLoaded -= LibraryLoaded;
            }

            EngineController = controller;
            controller.MessageLogged += Log;
            controller.LibraryServices.LibraryLoaded += LibraryLoaded;
            
            if (markNodesAsDirty)
            {
                // Mark all nodes as dirty so that AST for the whole graph will be
                // regenerated.
                foreach (var node in Nodes)
                {
                    node.MarkNodeAsModified();
                }
                OnNodesModified();
            }

            if (DynamicRunEnabled)
                Run();
        }

        /// <summary>
        /// This callback method is invoked in the context of ISchedulerThread 
        /// when UpdateGraphAsyncTask is completed.
        /// </summary>
        /// <param name="task">The original UpdateGraphAsyncTask instance.</param>
        private void OnUpdateGraphCompleted(AsyncTask task)
        {
            var updateTask = (UpdateGraphAsyncTask)task;
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

            // Refresh values of nodes that took part in update.
            foreach (var modifiedNode in updateTask.ModifiedNodes)
                modifiedNode.RequestValueUpdateAsync(scheduler, EngineController);

            foreach (var node in Nodes)
            {
                node.ClearDirtyFlag();
            }

            // Notify listeners (optional) of completion.
            RunEnabled = true; // Re-enable 'Run' button.

            // This method is guaranteed to be called in the context of 
            // ISchedulerThread (for Revit's case, it is the idle thread).
            // Dispatch the failure message display for execution on UI thread.
            // 
            EvaluationCompletedEventArgs e = task.Exception == null || IsTestMode
                ? new EvaluationCompletedEventArgs(true)
                : new EvaluationCompletedEventArgs(true, task.Exception);

            OnEvaluationCompleted(e);
        }

        /// <summary>
        ///     Flag specifying if this workspace is operating in "test mode".
        /// </summary>
        public bool IsTestMode { get; set; }

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
                if (setTraceDataTask.Initialize(EngineController, this))
                    scheduler.ScheduleForExecution(setTraceDataTask);
            }

            // If one or more custom node have been updated, make sure they
            // are compiled first before the home workspace gets evaluated.
            // 
            EngineController.ProcessPendingCustomNodeSyncData(scheduler);

            var task = new UpdateGraphAsyncTask(scheduler, VerboseLogging);
            if (task.Initialize(EngineController, this))
            {
                task.Completed += OnUpdateGraphCompleted;
                RunEnabled = false; // Disable 'Run' button.
                scheduler.ScheduleForExecution(task);
            }
            else
            {
                // Notify handlers that evaluation did not take place.
                var e = new EvaluationCompletedEventArgs(false);
                OnEvaluationCompleted(e);
            }
        }

        public event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;
        protected virtual void OnEvaluationCompleted(EvaluationCompletedEventArgs e)
        {
            var handler = EvaluationCompleted;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}
