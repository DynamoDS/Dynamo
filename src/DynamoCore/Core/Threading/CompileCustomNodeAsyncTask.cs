using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using ProtoScript.Runners;

namespace Dynamo.Core.Threading
{
    class CompileCustomNodeParams
    {
        internal GraphSyncData SyncData { get; set; }
        internal EngineController EngineController { get; set; }
    }

    /// <summary>
    /// Schedule this task to compile a CustomNodeDefinition asynchronously.
    /// </summary>
    class CompileCustomNodeAsyncTask : AsyncTask
    {
        private GraphSyncData graphSyncData;
        private EngineController engineController;

        internal override TaskPriority Priority
        {
            get { return TaskPriority.AboveNormal; }
        }

        #region Public Class Operational Methods

        internal CompileCustomNodeAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
        }

        /// <summary>
        /// Call this method to intialize a CompileCustomNodeAsyncTask with an 
        /// EngineController and an GraphSyncData that is required to compile the 
        /// associated custom node.
        /// </summary>
        /// <param name="initParams">Input parameters required for custom node 
        /// graph updates.</param>
        /// <returns>Returns true if GraphSyncData is not empty and that the 
        /// CompileCustomNodeAsyncTask should be scheduled for execution. Returns
        /// false otherwise.</returns>
        /// 
        internal bool Initialize(CompileCustomNodeParams initParams)
        {
            if (initParams == null)
                throw new ArgumentNullException("initParams");

            engineController = initParams.EngineController;
            graphSyncData = initParams.SyncData;

            if (engineController == null)
                throw new ArgumentNullException("engineController");
            if (graphSyncData == null)
                throw new ArgumentNullException("graphSyncData");

            var added = graphSyncData.AddedSubtrees;
            var deleted = graphSyncData.DeletedSubtrees;
            var modified = graphSyncData.ModifiedSubtrees;

            // Returns true if there is any actual data.
            return ((added != null && added.Count > 0) ||
                    (modified != null && modified.Count > 0) ||
                    (deleted != null && deleted.Count > 0));
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            // Updating graph in the context of ISchedulerThread.
            engineController.UpdateGraphImmediate(graphSyncData);
        }

        protected override void HandleTaskCompletionCore()
        {
            // Nothing needs to be done here for now, unless we 
            // want to display custom node execution related errors.
        }

        #endregion
    }
}
