using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // Go through all the items in list, comparing each of them 
            // with others that followed (cross checking each pair in list).
            for (int start = 0; start < taskQueue.Count - 1; ++start)
            {
                var removeBaseTask = false;
                var baseTask = taskQueue[start];

                int index = start + 1;
                while (index < taskQueue.Count && (removeBaseTask == false))
                {
                    switch (baseTask.CanMergeWith(taskQueue[index]))
                    {
                        case AsyncTask.TaskMergeInstruction.KeepBoth:
                            index = index + 1;
                            break; // Do nothing here.

                        case AsyncTask.TaskMergeInstruction.KeepThis:
                            taskQueue.RemoveAt(index);
                            break; // Keep the base task.

                        case AsyncTask.TaskMergeInstruction.KeepOther:
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
            if (taskQueue.Count < 2) // Nothing to reprioritize here.
                return;

            // "List.Sort" method performs an unstable sort, which means if two 
            // entries have the same key value, the resulting order may not be 
            // the same order they originally appear on the list. Use "OrderBy" 
            // method here to perform stable sort.
            // 
            var temp = taskQueue.ToList(); // Duplicate the source list.
            taskQueue.Clear(); // Then it's safe to clear the source list.
            taskQueue.AddRange(temp.OrderBy(t => t, new AsyncTaskComparer()));
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
