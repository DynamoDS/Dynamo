#if ENABLE_DYNAMO_SCHEDULER

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// This async task is scheduled after all UpdateRenderPackageAsyncTask are 
    /// scheduled on all nodes. This task will always be executed after all the 
    /// NodeModel objects have been updated with their render packages. It serves
    /// as a way to notify VisualizationManager when all NodeModel objects have 
    /// their render packages updated, providing a consistent way for performing 
    /// post render package generation actions.
    /// </summary>
    /// 
    class NotifyRenderPackagesReadyAsyncTask : AsyncTask
    {
        internal NotifyRenderPackagesReadyAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        protected override void ExecuteCore()
        {
            // Does nothing...
        }

        protected override void HandleTaskCompletionCore()
        {
        }
    }
}

#endif
