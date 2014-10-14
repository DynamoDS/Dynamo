using System;
using System.Collections.Generic;
using System.Threading;

using Dynamo.Interfaces;

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
        private readonly ISchedulerThread schedulerThread;
        private readonly List<AsyncTask> taskQueue = new List<AsyncTask>();
        private readonly TimeStampGenerator generator = new TimeStampGenerator();

        #endregion

        #region Private Class Helper Methods

        private void CompactTaskQueue()
        {
            if (taskQueue.Count < 2) // Cannot compact further.
                return;

            for (int start = 0; start < taskQueue.Count - 1; ++start)
            {
                var removeBaseTask = false;
                var baseTask = taskQueue[start];

                int index = start + 1;
                while (index < taskQueue.Count && (removeBaseTask == false))
                {
                    switch (baseTask.Compare(taskQueue[index]))
                    {
                        case AsyncTask.Comparison.KeepBoth:
                            index = index + 1;
                            break; // Do nothing here.

                        case AsyncTask.Comparison.KeepThis:
                            taskQueue.RemoveAt(index);
                            break; // Keep the base task.

                        case AsyncTask.Comparison.KeepOther:
                            removeBaseTask = true;
                            break;
                    }
                }

                if (removeBaseTask)
                {
                    taskQueue.RemoveAt(start);
                    start = start - 1;
                }
            }
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
