﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Dynamo.Services;

namespace Dynamo.Core.Threading
{
    internal delegate void AsyncTaskCompletedHandler(AsyncTask asyncTask);

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

    internal abstract class AsyncTask
    {
        #region Private Class Data Members

        /// <summary>
        /// Merge instruction obtained from a call to AsyncTask.CanMergeWith.
        /// </summary>
        internal enum TaskMergeInstruction
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

        private readonly DynamoScheduler scheduler;

        #endregion

        #region Public Class Properties, Events

        internal TimeStamp CreationTime { get; private set; }
        internal TimeStamp ScheduledTime { get; private set; }
        internal TimeStamp ExecutionStartTime { get; private set; }
        internal TimeStamp ExecutionEndTime { get; private set; }
        internal Exception Exception { get; private set; }

        /// <summary>
        /// This event is raised when the AsyncTask is completed. The event is 
        /// being raised in the context of ISchedulerThread, any UI element 
        /// access that is needed should be dispatched onto the UI dispatcher.
        /// </summary>
        internal event AsyncTaskCompletedHandler Completed;

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Constructs an instance of AsyncTask object.
        /// </summary>
        /// <param name="scheduler">A reference to the DynamoScheduler, this 
        /// parameter cannot be null.</param>
        /// 
        protected AsyncTask(DynamoScheduler scheduler)
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
            if (ReferenceEquals(this, otherTask))
                return TaskMergeInstruction.KeepBoth; // Both tasks are the same.

            return CanMergeWithCore(otherTask);
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
            if (ReferenceEquals(this, otherTask))
                return 0; // Both tasks are the same.

            return CompareCore(otherTask);
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
                ExecuteCore();
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

        #endregion

        #region Protected/Private Class Helper Methods

        protected abstract void ExecuteCore();
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
