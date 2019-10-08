using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Engine;
using ProtoCore.BuildData;
using ProtoScript.Runners;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Scheduler
{
    class UpdateGraphAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        private GraphSyncData graphSyncData;
        private EngineController engineController;
        private bool verboseLogging;

        public override TaskPriority Priority
        {
            get { return TaskPriority.AboveNormal; }
        }

        public IEnumerable<NodeModel> ModifiedNodes { get; protected set; }
        private List<NodeModel> executedNodes;

        public IEnumerable<NodeModel> ExecutedNodes { get { return executedNodes; } }

        #endregion

        #region Public Class Operational Methods

        internal UpdateGraphAsyncTask(IScheduler scheduler, bool verboseLogging1) : base(scheduler)
        {
            this.verboseLogging = verboseLogging1;
        }

        /// <summary>
        /// This method is called by code that intends to start a graph update.
        /// This method is called on the main thread where node collection in a 
        /// WorkspaceModel can be safely accessed.
        /// </summary>
        /// <param name="controller">Reference to an instance of EngineController 
        /// to assist in generating GraphSyncData object for the given set of nodes.
        /// </param>
        /// <param name="workspace">Reference to the WorkspaceModel from which a 
        /// set of updated nodes is computed. The EngineController generates the 
        /// resulting GraphSyncData from this list of updated nodes.</param>
        /// <returns>Returns true if there is any GraphSyncData, or false otherwise
        /// (in which case there will be no need to schedule UpdateGraphAsyncTask 
        /// for execution).</returns>
        /// 
        internal bool Initialize(EngineController controller, WorkspaceModel workspace)
        {
            try
            {
                engineController = controller;
                TargetedWorkspace = workspace;

                ModifiedNodes = ComputeModifiedNodes(workspace);
                graphSyncData = engineController.ComputeSyncData(workspace.Nodes, ModifiedNodes, verboseLogging);
                if (graphSyncData == null)
                    return false;

                if (engineController.ProfilingSession != null)
                {
                    engineController.ProfilingSession.UnregisterDeletedNodes(workspace.Nodes);
                }

                // We clear dirty flags before executing the task. If we clear
                // flags after the execution of task, for example in
                // AsyncTask.Completed or in HandleTaskCompletionCore(), as both
                // are executed in the other thread, although some nodes are
                // modified and we request graph execution, but just before
                // computing sync data, the task completion handler jumps in
                // and clear dirty flags. Now graph sync data will be null and
                // graph is in wrong state.
                foreach (var nodeGuid in graphSyncData.NodeIDs)
                {
                    var node = workspace.Nodes.FirstOrDefault(n => n.GUID.Equals(nodeGuid));
                    if (node != null)
                        node.ClearDirtyFlag();
                }

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("UpgradeGraphAsyncTask saw: " + e.ToString());
                return false;
            }
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            // Updating graph in the context of ISchedulerThread.

            // EngineController might be disposed and become invalid.
            // After MAGN-5167 is done, we could remove this checking.
            if (!engineController.IsDisposed)
                engineController.UpdateGraphImmediate(graphSyncData);
        }

        protected override void HandleTaskCompletionCore()
        {
            if (engineController.IsDisposed)
            {
                BuildWarnings = new Dictionary<Guid, List<WarningEntry>>();
                RuntimeWarnings = new Dictionary<Guid, List<ProtoCore.Runtime.WarningEntry>>();
            }
            else
            {
                // Retrieve warnings in the context of ISchedulerThread.
                BuildWarnings = engineController.GetBuildWarnings();
                RuntimeWarnings = engineController.GetRuntimeWarnings();

                // Mark all modified nodes as being updated (if the task has been 
                // successfully scheduled, executed and completed, it is expected 
                // for "modifiedNodes" to be both non-null and non-empty.
                // 
                // In addition to marking modified nodes as being updated, their 
                // warning states are cleared (which include the tool-tip). Any node
                // that has build/runtime warnings assigned to it will properly be 
                // restored to warning state when task completion handler sets the 
                // corresponding build/runtime warning on it.
                // 
                executedNodes = new List<NodeModel>();
                var executedNodeGuids = engineController.GetExecutedAstGuids(graphSyncData.SessionID);
                foreach (var guid in executedNodeGuids)
                {
                    var node = TargetedWorkspace.Nodes.FirstOrDefault(n => n.GUID.Equals(guid));
                    if (node != null)
                    {
                        executedNodes.Add(node);
                    }
                }

                foreach (var node in executedNodes)
                {
                    node.WasInvolvedInExecution = true;
                    node.WasRenderPackageUpdatedAfterExecution = false;
                    if (node.State == ElementState.Warning)
                        node.ClearErrorsAndWarnings();
                }

                engineController.RemoveRecordedAstGuidsForSession(graphSyncData.SessionID);
            }
        }

        /// <summary>
        /// Returns true if this task's graph sync data is a super set of the other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Contains(UpdateGraphAsyncTask other)
        {
            // Be more conservative here. Not only check ModifiedNodes, but
            // also check *all* nodes in graph sync data. For example, consider
            // a CBN outputs to a node, the node is a downstream node of cbn.
            //
            // Now if node is modified and request a run, task's ModifiedNodes
            // will cotain this node; then CBN is modified and request a run,
            // ModifiedNodes would contain both CBN and the node, and previous
            // task will be thrown away.
            if (!other.ModifiedNodes.All(ModifiedNodes.Contains))
                return false;

            if (graphSyncData == null)
                return other.graphSyncData == null;
            else if (other.graphSyncData == null)
                return true;

            return other.graphSyncData.AddedNodeIDs.All(graphSyncData.AddedNodeIDs.Contains) &&
                   other.graphSyncData.ModifiedNodeIDs.All(graphSyncData.ModifiedNodeIDs.Contains) &&
                   other.graphSyncData.DeletedNodeIDs.All(graphSyncData.DeletedNodeIDs.Contains);
        }

        private bool IsScheduledAfter(UpdateGraphAsyncTask other)
        {
            return CreationTime > other.CreationTime;
        }

        protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            var theOtherTask = otherTask as UpdateGraphAsyncTask;
            if (theOtherTask == null)
                return base.CanMergeWithCore(otherTask);

            if (theOtherTask.IsScheduledAfter(this) && theOtherTask.Contains(this))
                return TaskMergeInstruction.KeepOther;
            else if (this.IsScheduledAfter(theOtherTask) && this.Contains(theOtherTask))
                return TaskMergeInstruction.KeepThis;
            else
                return TaskMergeInstruction.KeepBoth;
        }

        #endregion

        #region Public Class Properties

        internal WorkspaceModel TargetedWorkspace { get; private set; }
        internal IDictionary<Guid, List<WarningEntry>> BuildWarnings { get; private set; }
        internal IDictionary<Guid, List<ProtoCore.Runtime.WarningEntry>> RuntimeWarnings { get; private set; }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// This method is called to gather all the nodes whose cached values 
        /// should be updated after graph update is done. This set of nodes 
        /// includes nodes that are explicitly marked as requiring update, as 
        /// well as all its downstream nodes.
        /// </summary>
        /// <param name="workspace">The WorkspaceModel from which nodes are to
        /// be retrieved.</param>
        /// <returns>Returns a list of NodeModel whose cached values are to be 
        /// updated after the evaluation.</returns>
        /// 
        private static IEnumerable<NodeModel> ComputeModifiedNodes(WorkspaceModel workspace)
        {
            var nodesToUpdate = new List<NodeModel>();
            //Get those modified nodes that are not frozen
            foreach (var node in workspace.Nodes.Where(n => n.IsModified && !n.IsFrozen))
            {
                GetDownstreamNodes(node, nodesToUpdate);
            }

            return nodesToUpdate;
        }

        /// <summary>
        /// Call this method to recursively gather downstream nodes of a given node.
        /// Returns only those nodes that are in RUN state.
        /// </summary>
        /// <param name="node">A NodeModel whose downstream nodes are to be gathered.</param>
        /// <param name="gathered">A list of all downstream nodes.</param>
        /// 
        private static void GetDownstreamNodes(NodeModel node, ICollection<NodeModel> gathered)
        {
            if (gathered.Contains(node) || node.IsFrozen) // Considered this node before, bail.pu
                return;

            gathered.Add(node);

            var sets = node.OutputNodes.Values;
            var outputNodes = sets.SelectMany(set => set.Select(t => t.Item2)).Distinct();
            foreach (var outputNode in outputNodes)
            {
                // Recursively get all downstream nodes.
                GetDownstreamNodes(outputNode, gathered);
            }
        }
        
        #endregion
    }
}
