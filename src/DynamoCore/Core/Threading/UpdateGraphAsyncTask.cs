using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.DSEngine;
using Dynamo.Models;

using GraphLayout;

using ProtoCore.BuildData;

using ProtoScript.Runners;

using BuildWarning = ProtoCore.BuildData.WarningEntry;
using RuntimeWarning = ProtoCore.RuntimeData.WarningEntry;

namespace Dynamo.Core.Threading
{
#if ENABLE_DYNAMO_SCHEDULER

    class UpdateGraphAsyncTask : AsyncTask
    {
        private GraphSyncData graphSyncData;
        private EngineController engineController;

        #region Public Class Operational Methods

        internal UpdateGraphAsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
            : base(scheduler, callback)
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

                var updatedNodes = ComputeModifiedNodes(workspace);
                graphSyncData = engineController.ComputeSyncData(updatedNodes);
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

#endif
}
