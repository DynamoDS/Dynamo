using System;

namespace Dynamo.Core.Threading
{
    internal abstract class AsyncTask
    {
        #region Private Class Data Members

        private readonly DynamoScheduler scheduler;
        private readonly Action<AsyncTask> callback;

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Constructs an instance of AsyncTask object.
        /// </summary>
        /// <param name="scheduler">A reference to the DynamoScheduler, this 
        /// parameter cannot be null.</param>
        /// <param name="callback">A delegate to be invoked when the AsyncTask 
        /// completes asynchronously. This parameter is optional.</param>
        /// 
        protected AsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.scheduler = scheduler;
            this.callback = callback;
            CreationTime = scheduler.NextTimeStamp;
        }

        /// <summary>
        /// This method is invoked when DynamoScheduler places this AsyncTask 
        /// into its internal task queue. This method is only meant to be used 
        /// internally by the DynamoScheduler.
        /// </summary>
        /// 
        internal void MarkTaskAsScheduled()
        {
            ScheduledTime = scheduler.NextTimeStamp;
        }

        /// <summary>
        /// This method is called when the SchedulerThread decides to execute a 
        /// scheduled AsyncTask in the task queue. Derived tasks overrides 
        /// ExecuteCore method to perform relevant operations. This method is 
        /// invoked in the context of the SchedulerThread and not the main thread.
        /// </summary>
        /// 
        internal void Execute()
        {
            ExecutionStartTime = scheduler.NextTimeStamp;
            ExecuteCore();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception">The exception that is thrown from a prior 
        /// call to Execute method will be caught and passed through this 
        /// parameter. This value may be null if no exception was thrown.</param>
        /// 
        internal void HandleTaskCompletion(Exception exception)
        {
            ExecutionEndTime = scheduler.NextTimeStamp;
            Exception = exception;
            HandleTaskCompletionCore();

            // If exists, the registered callback Action is invoked.
            if (callback != null)
                callback(this);
        }

        #endregion

        #region Public Class Properties

        internal TimeStamp CreationTime { get; private set; }
        internal TimeStamp ScheduledTime { get; private set; }
        internal TimeStamp ExecutionStartTime { get; private set; }
        internal TimeStamp ExecutionEndTime { get; private set; }
        internal Exception Exception { get; private set; }

        #endregion

        #region Protected/Private Class Helper Methods

        protected abstract void ExecuteCore();
        protected abstract void HandleTaskCompletionCore();

        #endregion
    }
}
