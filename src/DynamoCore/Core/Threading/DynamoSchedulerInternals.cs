using System;
using System.Collections.Generic;
using System.Threading;

namespace Dynamo.Core.Threading
{
    partial class DynamoScheduler
    {
        #region Private Class Data Members

        private enum EventIndex
        {
            TaskAvailable, Shutdown
        }

        private readonly ManualResetEvent[] waitHandles =
        {
            new ManualResetEvent(false), // Task available event
            new ManualResetEvent(false)  // Scheduler shutdown event
        };

        private bool taskQueueUpdated;
        private readonly List<AsyncTask> taskQueue = new List<AsyncTask>();
        private readonly TimeStampGenerator generator = new TimeStampGenerator();

        #endregion

        #region Private Class Helper Methods

        private void ScheduleTask(AsyncTask asyncTask)
        {
            lock (taskQueue)
            {
                taskQueue.Add(asyncTask);
                asyncTask.MarkTaskAsScheduled(); // Update internal time-stamp.
                taskQueueUpdated = true; // Mark task queue as being updated.

                // Signal task availability so scheduler picks it up.
                waitHandles[(int)EventIndex.TaskAvailable].Set();
            }
        }

        private void CompactTaskQueue()
        {
            // TODO: Add queue compacting codes here.
        }

        private void ReprioritizeTasksInQueue()
        {
            // TODO: Add queue re-ordering codes here.
        }

        private static void ProcessTaskInternal(AsyncTask asyncTask)
        {
            try
            {
                asyncTask.Execute(); // Internally sets the ExecutionStartTime
                asyncTask.HandleTaskCompletion(null); // Completed successfully.
            }
            catch (Exception exception)
            {
                // HandleTaskCompletion internally sets the ExecutionEndTime time,
                // it also invokes the registered callback Action if there is one.
                // 
                asyncTask.HandleTaskCompletion(exception);
            }
        }

        #endregion
    }
}
