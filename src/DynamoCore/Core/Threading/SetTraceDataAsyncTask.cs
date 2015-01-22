using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Core.Threading
{
    class SetTraceDataAsyncTask : AsyncTask
    {
        private EngineController engineController;
        private IEnumerable<KeyValuePair<Guid, List<string>>> traceData;

        internal override TaskPriority Priority
        {
            get { return TaskPriority.Highest; }
        }

        internal SetTraceDataAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
        }

        /// <summary>
        /// This method is called by task creator to associate the trace data with
        /// the current instance of virtual machine. The given WorkspaceModel can
        /// optionally contain saved trace data in a previous execution session. As
        /// a side-effect, this method resets "WorkspaceModel.PreloadedTraceData"
        /// data member to ensure the correctness of the execution flow.
        /// </summary>
        /// <param name="controller">Reference to the EngineController on which the 
        /// loaded trace data should be set.</param>
        /// <param name="workspace">The workspace from which the trace data should 
        /// be retrieved.</param>
        /// <returns>If the given WorkspaceModel contains saved trace data, this 
        /// method returns true, in which case the task needs to be scheduled.
        /// Otherwise, the method returns false.</returns>
        /// 
        internal bool Initialize(EngineController controller, HomeWorkspaceModel workspace)
        {
            if (controller == null || (controller.LiveRunnerCore == null))
                return false;

            engineController = controller;
            traceData = workspace.PreloadedTraceData;

            TargetedWorkspace = workspace;
            workspace.PreloadedTraceData = null;
            return ((traceData != null) && traceData.Any());
        }

        protected override void HandleTaskExecutionCore()
        {
            engineController.LiveRunnerCore.SetTraceDataForNodes(traceData);
        }

        protected override void HandleTaskCompletionCore()
        {
            // Setting of trace data is a transparent 
            // operation and does not require any follow-up.
        }

        #region Public Class Properties

        internal WorkspaceModel TargetedWorkspace { get; private set; }

        #endregion
    }
}
