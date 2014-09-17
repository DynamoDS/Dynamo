namespace Dynamo.Core.Threading
{
#if ENABLE_DYNAMO_SCHEDULER

    class CompileCustomNodeParams
    {
        internal EngineController EngineController { get; set; }
        internal CustomNodeDefinition Definition { get; set; }
        internal IEnumerable<NodeModel> Nodes { get; set; }
        internal IEnumerable<string> Parameters { get; set; }
        internal IEnumerable<AssociativeNode> Outputs { get; set; }
    }

    /// <summary>
    /// Schedule this task to compile a CustomNodeDefinition asynchronously.
    /// </summary>
    class CompileCustomNodeAsyncTask : AsyncTask
    {
        private GraphSyncData graphSyncData;
        private EngineController engineController;

        #region Public Class Operational Methods

        internal CompileCustomNodeAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        /// <summary>
        /// Call this method to intialize a CompileCustomNodeAsyncTask with an 
        /// EngineController, nodes from the corresponding CustomNodeWorkspaceModel,
        /// and inputs/outputs of the CustomNodeDefinition.
        /// </summary>
        /// <param name="initParams">Input parameters required for compilation of 
        /// the CustomNodeDefinition.</param>
        /// <returns>Returns true if GraphSyncData is generated successfully and 
        /// that the CompileCustomNodeAsyncTask should be scheduled for execution.
        /// Returns false otherwise.</returns>
        /// 
        internal bool Initialize(CompileCustomNodeParams initParams)
        {
            engineController = initParams.EngineController;

            try
            {
                graphSyncData = engineController.ComputeSyncData(initParams);
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
            // Nothing needs to be done here for now, unless we 
            // want to display custom node execution related errors.
        }

        #endregion
    }

#endif
}
