using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dynamo.Scheduler
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

        public IEnumerable<AsyncTask> Tasks
        {
            get {
                lock (taskQueue)
                {
                    return taskQueue.ToList();
                } 
            }
        }

        /// <summary>
        /// Return true if task queue is not empty.
        /// </summary>
        public bool HasPendingTasks
        {
            get
            {
                lock (taskQueue)
                {
                    return taskQueue.Any();
                }
            }
        }
        #endregion

        #region Private Class Helper Methods

        private void NotifyTaskStateChanged(AsyncTask task, TaskStateChangedEventArgs.State state)
        {
            var stateChangedHandler = TaskStateChanged;
            if (stateChangedHandler == null)
                return; // No event handler, bail.

            var e = new TaskStateChangedEventArgs(task, state);
            stateChangedHandler(this, e);

            if (state == TaskStateChangedEventArgs.State.Discarded)
            {
                task.HandleTaskDiscarded();
            }
        }

        /// <summary>
        /// This method is called when ISchedulerThread calls ProcessNextTask 
        /// method to process next available task in queue. It is only called 
        /// if the task queue was updated (one or more AsyncTask scheduled)
        /// before ISchedulerThread picks up the next task.
        /// </summary>
        /// <returns>Returns a list of AsyncTask objects that were dropped, if
        /// any, during task queue compact. The return list can be empty but it 
        /// will never be null.
        /// </returns>
        /// 
        private IEnumerable<AsyncTask> CompactTaskQueue()
        {
            // This list keeps track of tasks before they are removed.
            var droppedTasks = new List<AsyncTask>();

            if (taskQueue.Count < 2) // Cannot compact further.
                return droppedTasks;

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
                            droppedTasks.Add(taskQueue[index]);
                            taskQueue.RemoveAt(index);
                            break; // Keep the base task.

                        case AsyncTask.TaskMergeInstruction.KeepOther:
                            removeBaseTask = true;
                            break;
                    }
                }

                if (removeBaseTask)
                {
                    droppedTasks.Add(taskQueue[start]);
                    taskQueue.RemoveAt(start);
                    start = start - 1;
                }
            }

            return droppedTasks;
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

            // First sort tasks by AsyncTask.Priority property, then sort them 
            // by calling AsyncTask.Compare method for relative importance.
            taskQueue.AddRange(temp.OrderBy(t => t.Priority)
                .ThenBy(t => t, new AsyncTaskComparer()));
        }

        private void ProcessTaskInternal(AsyncTask asyncTask)
        {
            NotifyTaskStateChanged(asyncTask, TaskStateChangedEventArgs.State.ExecutionStarting);

            var executionState = asyncTask.Execute()
                ? TaskStateChangedEventArgs.State.ExecutionCompleted
                : TaskStateChangedEventArgs.State.ExecutionFailed;

            NotifyTaskStateChanged(asyncTask, executionState);
            asyncTask.HandleTaskCompletion();
            NotifyTaskStateChanged(asyncTask, TaskStateChangedEventArgs.State.CompletionHandled);
        }

        #endregion
    }
}
