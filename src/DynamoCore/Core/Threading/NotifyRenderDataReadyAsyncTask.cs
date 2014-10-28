using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// During execution this task just triggers the event about 
    /// all render packages are already computed. It's scheduled 
    /// after all UpdateRenderPackageAsyncTask objects are 
    /// scheduled for execution. 
    /// Mainly such notification is needed for DynamoWebServer 
    /// to know when all computations are done and it can send
    /// ComputationResponse to Flood
    /// </summary>
    class NotifyRenderDataReadyAsyncTask: AsyncTask
    {
        private DynamoModel dynamoModel;

        public NotifyRenderDataReadyAsyncTask(DynamoScheduler scheduler)
            : base(scheduler) { }


        /// <summary>
        /// Call this method to determine if the task should be scheduled for 
        /// execution.
        /// </summary>
        /// <param name="dynamoModel">DynamoModel instance to 
        /// trigger the event</param>
        /// <returns>Returns true if the task should be scheduled 
        /// for execution, or false otherwise.</returns>
        internal bool Initialize(DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;
            return (dynamoModel != null);
        }

        protected override void HandleTaskCompletionCore() { }

        protected override void ExecuteCore()
        {
            dynamoModel.OnNodesRenderPackagesUpdated(null, null);
        }
    }
}
