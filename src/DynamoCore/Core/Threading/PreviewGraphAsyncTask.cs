
using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoScript.Runners;

using BuildWarning = ProtoCore.BuildData.WarningEntry;
using RuntimeWarning = ProtoCore.BuildData.WarningEntry;
using Dynamo.Interfaces;

namespace Dynamo.Core.Threading
{
    class PreviewGraphAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        private GraphSyncData graphSyncData;
        private EngineController engineController;
        private IEnumerable<NodeModel> modifiedNodes;
        private bool verboseLogging;
        public List<Guid> previewGraphData;
        
        internal override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        internal IEnumerable<NodeModel> ModifiedNodes
        {
            get { return modifiedNodes; }
        }

        #endregion

        #region Public Class Operational Methods

        internal PreviewGraphAsyncTask(IScheduler scheduler, bool verboseLogging1)
            : base(scheduler)
        {
            verboseLogging = verboseLogging;
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
        /// <param name="dynamoLogger"> Logs the error message</param>
        /// <returns>Returns the list of node id's that will be executed in the next run
        /// for execution).</returns>
        internal List<Guid> Initialize(EngineController controller, WorkspaceModel workspace)
        {
            try
            {
                engineController = controller;
                TargetedWorkspace = workspace;                
                modifiedNodes = ComputeModifiedNodes(workspace);                
                previewGraphData = engineController.PreviewGraphSyncData(modifiedNodes,verboseLogging);
                return previewGraphData;
            }
            catch (Exception e)
            {             
                return null;
            }
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            
        }

        protected override void HandleTaskCompletionCore()
        {
           
        }

        #endregion

        #region Public Class Properties
        internal WorkspaceModel TargetedWorkspace { get; private set; }       
        #endregion

        #region Private Class Helper Methods

        private static IEnumerable<NodeModel> ComputeModifiedNodes(WorkspaceModel workspace)
        {
            return workspace.Nodes; // TODO(Ben): Implement dirty node subsetting.
        }

        #endregion
    }
}

