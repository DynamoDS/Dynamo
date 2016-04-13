using System;
using System.Collections.Generic;

namespace Dynamo.Scheduler
{
    /// <summary>
    /// This delegate is used in AsyncTask events: Completed and Discarded.
    /// </summary>
    /// <param name="asyncTask"><see cref="AsyncTask"/></param>
    public delegate void AsyncTaskCompletedHandler(AsyncTask asyncTask);

    /// <summary>
    /// DynamoScheduler makes use of this comparer class to sort its internal 
    /// task queue. Refer to AsyncTask.Compare for ordering requirements.
    /// </summary>
    /// 
    internal class AsyncTaskComparer : IComparer<AsyncTask>
    {
        public int Compare(AsyncTask x, AsyncTask y)
        {
            // This method will be called for the same element in the list, and
            // in this case ReferenceEquals will be true, return 0 for that case.
            return ReferenceEquals(x, y) ? 0 : x.Compare(y);
        }
    }

    /// <summary>
    /// This is the base class for async tasks, that are running on Dynamo Scheduler.
    /// </summary>
    public abstract class AsyncTask
    {
        #region Private Class Data Members

        /// <summary>
        /// Returns priority of the <see cref="AsyncTask"/>.
        /// </summary>
        public enum TaskPriority
        {
            Critical,
            Highest,
            AboveNormal,
            Normal,
            BelowNormal,
            Lowest,
            Idle
        }

        /// <summary>
        /// Merge instruction obtained from a call to AsyncTask.CanMergeWith.
        /// </summary>
        public enum TaskMergeInstruction
        {
            /// <summary>
            /// Both the AsyncTask objects in comparison should be kept.
            /// </summary>
            KeepBoth,

            /// <summary>
            /// The current instance of AsyncTask should be kept 
            /// while discarding the other AsyncTask in comparison.
            /// </summary>
            KeepThis,

            /// <summary>
            /// The current instance of AsyncTask should be discarded 
            /// while keeping the other AsyncTask in comparison.
            /// </summary>
            KeepOther
        }

        /// <summary>
        /// Dynamo Scheduler.
        /// </summary>
        internal readonly IScheduler scheduler;

        #endregion

        #region Public Class Properties, Events

        internal TimeStamp CreationTime { get; private set; }
        internal TimeStamp ScheduledTime { get; private set; }
        internal TimeStamp ExecutionStartTime { get; private set; }
        internal TimeStamp ExecutionEndTime { get; private set; }
        internal Exception Exception { get; private set; }

        /// <summary>
        /// DynamoScheduler sorts tasks base on two key factors: the priority of
        /// a task, and the relative importance between two tasks that has the 
        /// same priority. During task reprioritization process, DynamoScheduler 
        /// first sorts the tasks in accordance to their AsyncTask.Priority
        /// property. The resulting ordered list is then sorted again by calling
        /// AsyncTask.Compare method to determine the relative importance among
        /// tasks having the same priority.
        /// </summary>
        /// 
        public abstract TaskPriority Priority { get; }

        /// <summary>
        /// This event is raised when the AsyncTask is completed. The event is 
        /// being raised in the context of ISchedulerThread, any UI element 
        /// access that is needed should be dispatched onto the UI dispatcher.
        /// </summary>
        /// 
        public event AsyncTaskCompletedHandler Completed;

        /// <summary>
        /// Raised if the AsyncTask is discarded by an IScheduler and will not be executed
        /// </summary>
        internal event AsyncTaskCompletedHandler Discarded;

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Constructs an instance of AsyncTask object.
        /// </summary>
        /// <param name="scheduler">A reference to the DynamoScheduler, this 
        /// parameter cannot be null.</param>
        /// 
        protected AsyncTask(IScheduler scheduler)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            this.scheduler = scheduler;
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
        /// This method is called by DynamoScheduler when it compacts the task 
        /// queue. The result of this call indicates if either of the tasks in 
        /// comparison should be dropped from the task queue, or both should be 
        /// kept. Tasks that are discarded during this phase will not be executed.
        /// </summary>
        /// <param name="otherTask">Another task to compare with.</param>
        /// <returns>Returns the comparison result. See Comparison enumeration 
        /// for details of the possible values.</returns>
        /// 
        internal TaskMergeInstruction CanMergeWith(AsyncTask otherTask)
        {
            return ReferenceEquals(this, otherTask)
                ? TaskMergeInstruction.KeepBoth
                : CanMergeWithCore(otherTask);
        }

        /// <summary>
        /// Call this method to determine the relative priority of two AsyncTask
        /// objects. DynamoScheduler makes use of this method to determine the 
        /// order in which AsyncTask objects are sorted in its internal task queue.
        /// </summary>
        /// <param name="otherTask">A task to compare this task with.</param>
        /// <returns>Returns -1 if this AsyncTask object should be processed
        /// before the other AsyncTask; returns 1 if this AsyncTask object should
        /// be processed after the other AsyncTask; or 0 if both AsyncTask objects
        /// have the same priority and can be processed in the current order.
        /// </returns>
        /// 
        internal int Compare(AsyncTask otherTask)
        {
            return ReferenceEquals(this, otherTask) ? 0 : CompareCore(otherTask);
        }

        /// <summary>
        /// This method is called when the SchedulerThread decides to execute a 
        /// scheduled AsyncTask in the task queue. Derived tasks overrides 
        /// ExecuteCore method to perform relevant operations. This method is 
        /// invoked in the context of the SchedulerThread and not the main thread.
        /// </summary>
        /// <returns>Returns true if execution was successful, or false if an 
        /// exception was thrown.</returns>
        /// 
        internal bool Execute()
        {
            ExecutionStartTime = scheduler.NextTimeStamp;

            try
            {
                HandleTaskExecutionCore();
            }
            catch (Exception exception)
            {
                Exception = exception;
            }
            finally
            {
                ExecutionEndTime = scheduler.NextTimeStamp;
            }

            return Exception == null; // Exception thrown == execution failed.
        }

        /// <summary>
        /// This method is called by DynamoScheduler after AsyncTask.Execute 
        /// method returns. It is guaranteed to be called even when exception 
        /// is thrown from within AsyncTask.Execute.
        /// </summary>
        /// 
        internal void HandleTaskCompletion()
        {
            HandleTaskCompletionCore();

            // Notify registered event handlers of task completion.
            if (Completed != null)
                Completed(this);
        }


        internal void HandleTaskDiscarded()
        {
            if (Discarded != null)
            {
                Discarded(this);
            }
        }

        #endregion

        #region Protected/Private Class Helper Methods

        protected abstract void HandleTaskExecutionCore();
        protected abstract void HandleTaskCompletionCore();

        protected virtual TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            return TaskMergeInstruction.KeepBoth; // Keeping both tasks by default.
        }

        protected virtual int CompareCore(AsyncTask otherTask)
        {
            return 0; // Having the same priority by default.
        }

        #endregion
    }
}
