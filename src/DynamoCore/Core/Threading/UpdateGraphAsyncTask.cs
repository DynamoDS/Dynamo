#if ENABLE_DYNAMO_SCHEDULER

using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoScript.Runners;

using BuildWarning = ProtoCore.BuildData.WarningEntry;
using RuntimeWarning = ProtoCore.RuntimeData.WarningEntry;

namespace Dynamo.Core.Threading
{
    class UpdateGraphAsyncTask : AsyncTask
    {
        private GraphSyncData graphSyncData;
        private EngineController engineController;
        private IEnumerable<NodeModel> modifiedNodes;

        #region Public Class Operational Methods

        internal UpdateGraphAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        /// <summary>
        /// This method is called by codes that intent to start a graph update.
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

                modifiedNodes = ComputeModifiedNodes(workspace);
                graphSyncData = engineController.ComputeSyncData(modifiedNodes);
                return graphSyncData != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
        {
            // Updating graph in the context of ISchedulerThread.
            engineController.UpdateGraphImmediate(graphSyncData);
        }

        protected override void HandleTaskCompletionCore()
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
            foreach (var modifiedNode in modifiedNodes)
            {
                modifiedNode.IsUpdated = true;
                if (modifiedNode.State == ElementState.Warning)
                    modifiedNode.ClearRuntimeError();
            }
        }

        #endregion

        #region Public Class Properties

        internal WorkspaceModel TargetedWorkspace { get; private set; }
        internal IDictionary<Guid, List<BuildWarning>> BuildWarnings { get; private set; }
        internal IDictionary<Guid, List<RuntimeWarning>> RuntimeWarnings { get; private set; }

        #endregion

        #region Private Class Helper Methods

        private static IEnumerable<NodeModel> ComputeModifiedNodes(WorkspaceModel workspace)
        {
            return workspace.Nodes; // TODO(Ben): Implement dirty node subsetting.
        }

        #endregion
    }
}

#endif
