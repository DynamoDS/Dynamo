using System.Threading;

namespace Dynamo.Core.Threading
{
    public partial class DynamoScheduler
    {
        #region Public Class Operational Methods

        /// <summary>
        /// AsyncTask base class calls this to obtain the new time-stamp value.
        /// </summary>
        internal TimeStamp NextTimeStamp { get { return generator.Next; } }

        /// <summary>
        /// An ISchedulerThread implementation calls this method so scheduler 
        /// starts to process the next task in the queue, if there is any. Note 
        /// that this method is meant to process only one task in queue. The 
        /// implementation of ISchedulerThread is free to call this method again
        /// in a fashion that matches its task fetching behavior.
        /// </summary>
        /// <param name="waitIfTaskQueueIsEmpty">This parameter is only used if 
        /// the task queue is empty at the time this method is invoked. When the
        /// task queue becomes empty, setting this to true will cause this call  
        /// to block until either the next task becomes available, or when the 
        /// scheduler is requested to shutdown.</param>
        /// <returns>This method returns true if the task queue is not empty, or
        /// false otherwise. Note that this method returns false when scheduler
        /// begins to shutdown, even when the task queue is not empty.</returns>
        /// 
        public bool ProcessNextTask(bool waitIfTaskQueueIsEmpty)
        {
            AsyncTask nextTask = null;

            lock (taskQueue)
            {
                if (taskQueueUpdated)
                {
                    // The task queue has been updated since the last time 
                    // a task was processed, it might need compacting.
                    CompactTaskQueue();
                    ReprioritizeTasksInQueue();
                    taskQueueUpdated = false;
                }

                if (taskQueue.Count > 0)
                {
                    nextTask = taskQueue[0];
                    taskQueue.RemoveAt(0);
                }
                else
                {
                    // No more task in queue, reset wait handle.
                    waitHandles[(int)EventIndex.TaskAvailable].Reset();
                }
            }

            if (nextTask != null)
            {
                ProcessTaskInternal(nextTask);
                return true; // This method should be called again.
            }

            // If there's no more task and wait is not desired...
            if (!waitIfTaskQueueIsEmpty)
                return false; // The task queue is now empty.

            // Block here if ISchedulerThread requests to wait.
            // ReSharper disable once CoVariantArrayConversion
            int index = WaitHandle.WaitAny(waitHandles);

            // If a task becomes available, this method returns true to indicate 
            // that an immediate call may be required (subjected to the decision 
            // of the ISchedulerThread's implementation). In the event that the 
            // scheduler is shutting down, then this method returns false.
            // 
            return (index == ((int)EventIndex.TaskAvailable));
        }

        #endregion
    }
}
