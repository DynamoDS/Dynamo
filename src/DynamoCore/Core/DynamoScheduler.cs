using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamo.Core
{
    partial class DynamoScheduler
    {
        #region Public Class Operational Methods

        /// <summary>
        /// AsyncTask base class calls this to obtain the new time-stamp value.
        /// </summary>
        internal long NextTimeStamp { get { return this.timeStamp.Next; } }

        /// <summary>
        /// An ISchedulerThread implementation calls this method so scheduler 
        /// starts to process the next task in the queue, if there is any.
        /// </summary>
        /// <param name="waitIfTaskQueueIsEmpty">This parameter is only used if 
        /// the task queue is empty at the time this method is invoked. When the
        /// task queue becomes empty, setting this to true will cause this call  
        /// to block until either the next task becomes available, or when the 
        /// scheduler is requested to shutdown.</param>
        /// <returns>This method returns true if there is at least one task 
        /// found queued in the task queue, or false otherwise. Note that this 
        /// method returns false when scheduler begins to shutdown.</returns>
        /// 
        internal bool ProcessNextTask(bool waitIfTaskQueueIsEmpty)
        {
            AsyncTask nextTask = null;

            lock (taskQueue)
            {
                if (taskQueueModified)
                {
                    // TODO: Add queue compacting/re-ordering codes here.
                    taskQueueModified = false; // Done manipulating queue.
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
                this.ProcessTaskInternal(nextTask);
                return true; // This method should be called again.
            }

            // If there's no more task and wait is not desired...
            if (waitIfTaskQueueIsEmpty == false)
                return false; // An immediate call to this method is not needed.

            // Block here if ISchedulerThread requests to wait.
            int index = WaitHandle.WaitAny(waitHandles);

            // If a task becomes available, then an immediate call to this method is 
            // desired, otherwise return false (in the case of scheduler shutdown).
            return ((index == ((int)EventIndex.TaskAvailable)) ? true : false);
        }

        #endregion
    }
}
