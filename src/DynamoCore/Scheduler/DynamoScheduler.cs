using System;
using System.Collections.Generic;
using System.Threading;

namespace Dynamo.Scheduler
{
    public class TaskStateChangedEventArgs : EventArgs
    {
        public enum State
        {
            Scheduled,
            Discarded,
            ExecutionStarting,
            ExecutionFailed,
            ExecutionCompleted,
            CompletionHandled
        }

        internal AsyncTask Task { get; private set; }
        internal State CurrentState { get; private set; }

        internal TaskStateChangedEventArgs(AsyncTask task, State state)
        {
            Task = task;
            CurrentState = state;
        }
    }

    public delegate void TaskStateChangedEventHandler(
        DynamoScheduler sender, TaskStateChangedEventArgs e);

    /// <summary>
    /// This interface provides methods and properties used for Dynamo Scheduler.
    /// </summary>
    public interface IScheduler 
    {
        /// <summary>
        /// AsyncTask base class calls this to obtain the new time-stamp value.
        /// </summary>
        TimeStamp NextTimeStamp { get; }

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
        bool ProcessNextTask(bool waitIfTaskQueueIsEmpty);

        /// <summary>
        /// Callers of this method create an instance of AsyncTask derived 
        /// class and call this method to schedule the task for execution.
        /// </summary>
        /// <param name="asyncTask">The task to execute asynchronously.</param>
        /// 
        void ScheduleForExecution(AsyncTask asyncTask);

        /// <summary>
        /// The complete collection of all of the currently scheduled tasks
        /// </summary>
        IEnumerable<AsyncTask> Tasks { get; }
    }

    public enum TaskProcessMode
    {
        /// <summary>
        /// Scheduled task will be processed immediately
        /// </summary>
        Synchronous,
        /// <summary>
        /// Scheduled task will be processed by schedule
        /// </summary>
        Asynchronous
    }

    /// <summary>
    /// This class represents Dynamo scheduler. All the tasks are scheduled on the scheduler. Also, these tasks runs async.
    /// </summary>
    public partial class DynamoScheduler : IScheduler
    {
        #region Class Events, Properties

        /// <summary>
        /// Event that is raised when the state of an AsyncTask is changed.
        /// The state of an AsyncTask changes when it is scheduled, discarded,
        /// executed or completed. Note that this event is raised in the context
        /// of ISchedulerThread, any access to UI components or collections in 
        /// WorkspaceModel (e.g. Nodes, Connectors, etc.) should be dispatched
        /// for execution on the UI thread.
        /// </summary>
        internal event TaskStateChangedEventHandler TaskStateChanged;

        /// <summary>
        /// AsyncTask base class calls this to obtain the new time-stamp value.
        /// </summary>
        public TimeStamp NextTimeStamp { get { return generator.Next; } }

        #endregion

        #region Public Class Operational Methods

        internal DynamoScheduler(ISchedulerThread schedulerThread, TaskProcessMode processMode)
        {
            this.schedulerThread = schedulerThread;
            this.ProcessMode = processMode;

            // The host implementation of ISchedulerThread can begin access the 
            // scheduler as soon as this method is invoked. It is important for 
            // this call to be made at the very end of the constructor so we are 
            // sure everything in the scheduler is ready for access.
            this.schedulerThread.Initialize(this);
        }

        /// <summary>
        /// Call this method to properly shutdown the scheduler and its associated
        /// ISchedulerThread. This call will be blocked until the ISchedulerThread
        /// terminates. Note that the implementation of ISchedulerThread may or may 
        /// not choose to handle all the remaining AsyncTask in queue when shutdown 
        /// happens -- DynamoScheduler ensures the tasks in queue are cleared when 
        /// this call returns.
        /// </summary>
        internal void Shutdown()
        {
            // Signal shutdown event so "ProcessNextTask" returns "false" to the 
            // calling ISchedulerThread. ISchedulerThread then makes use of this 
            // return value to properly exit its loop.
            waitHandles[(int)EventIndex.Shutdown].Set();

            schedulerThread.Shutdown(); // Wait for scheduler thread to end.

            lock (taskQueue)
            {
                taskQueue.Clear();
                taskQueueUpdated = false;
            }
        }

        /// <summary>
        /// Callers of this method create an instance of AsyncTask derived 
        /// class and call this method to schedule the task for execution.
        /// </summary>
        /// <param name="asyncTask">The task to execute asynchronously.</param>
        public void ScheduleForExecution(AsyncTask asyncTask)
        {
            // When an AsyncTask is scheduled for execution during a test, it 
            // executes on the context of the thread that runs the unit test 
            // case (in a regular headless test case this is the unit test 
            // background thread; in a recorded test this is the main ui thread).
            // 
            if (ProcessMode == TaskProcessMode.Synchronous)
            {
                asyncTask.MarkTaskAsScheduled();
                NotifyTaskStateChanged(asyncTask, TaskStateChangedEventArgs.State.Scheduled);
                ProcessTaskInternal(asyncTask);
                return;
            }

            lock (taskQueue)
            {
                taskQueue.Add(asyncTask); // Append new task to the end
                asyncTask.MarkTaskAsScheduled(); // Update internal time-stamp.
                NotifyTaskStateChanged(asyncTask, TaskStateChangedEventArgs.State.Scheduled);

                // Mark the queue as being updated. This causes the next call
                // to "ProcessNextTask" method to post process the task queue.
                taskQueueUpdated = true;

                // Signal task availability so scheduler picks it up.
                waitHandles[(int)EventIndex.TaskAvailable].Set();
            }
        }

        /// <summary>
        /// Flag determining whether or not the scheduleed task will be
        /// processed immediately or not. 
        /// </summary>
        public TaskProcessMode ProcessMode { get; set; }

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
        public bool ProcessNextTask(bool waitIfTaskQueueIsEmpty)
        {
            AsyncTask nextTask = null;
            IEnumerable<AsyncTask> droppedTasks = null;

            lock (taskQueue)
            {
                if (taskQueueUpdated)
                {
                    // The task queue has been updated since the last time 
                    // a task was processed, it might need compacting.
                    droppedTasks = CompactTaskQueue();

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

            if (droppedTasks != null)
            {
                // Only notify listeners of dropping tasks here instead of
                // within CompactTaskQueue method. This way the lock on task
                // queue will not be held up for a prolonged period of time.
                // 
                foreach (var droppedTask in droppedTasks)
                {
                    NotifyTaskStateChanged(droppedTask, TaskStateChangedEventArgs.State.Discarded);
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
