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
        internal override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        internal NotifyRenderPackagesReadyAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
        }

        protected override void HandleTaskExecutionCore()
        {
            // Does nothing...
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            var theOtherTask = otherTask as NotifyRenderPackagesReadyAsyncTask;
            if (theOtherTask == null)
                return base.CanMergeWithCore(otherTask);

            // Comparing to another NotifyRenderPackagesReadyAsyncTask, the one 
            // that gets scheduled more recently stay, while the earlier one 
            // gets dropped. If this task has a higher tick count, keep this.
            // 
            if (ScheduledTime.TickCount > theOtherTask.ScheduledTime.TickCount)
                return TaskMergeInstruction.KeepThis;

            return TaskMergeInstruction.KeepOther; // Otherwise, keep the other.
        }
    }
}
