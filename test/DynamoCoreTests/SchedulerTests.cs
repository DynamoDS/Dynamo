using Dynamo.Core.Threading;
using Dynamo.Interfaces;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo
{
    using TaskState = TaskStateChangedEventArgs.State;

    #region Mock Classes for Test Cases

    class TaskEventObserver
    {
        readonly List<string> events = new List<string>();

        internal IEnumerable<string> Results { get { return events; } }

        internal void OnTaskStateChanged(
            DynamoScheduler scheduler,
            TaskStateChangedEventArgs e)
        {
            AddToResultList(e.Task.ToString(), e.CurrentState);
        }

        private void AddToResultList(string content, TaskState state)
        {
            var stateString = state.ToString();
            events.Add(string.Format("{0}: {1}", stateString, content));
        }
    }

    class SampleSchedulerThread : ISchedulerThread
    {
        private DynamoScheduler scheduler;

        public void Initialize(DynamoScheduler owningScheduler)
        {
            scheduler = owningScheduler;
        }

        public void Shutdown()
        {
        }

        internal void GetSchedulerToProcessTasks()
        {
            while (scheduler.ProcessNextTask(false))
            {
                // Continue too run till tasks are exhausted.
            }
        }
    }

    class SampleAsyncTask : AsyncTask
    {
        private List<string> results;

        internal SampleAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        internal void InitializeWithResultList(List<string> resultsList)
        {
            results = resultsList;
        }

        protected void AddToResultList(string result)
        {
            if (results != null)
                results.Add(result);
        }

        protected override void ExecuteCore()
        {
        }

        protected override void HandleTaskCompletionCore()
        {
        }
    }

    /// <summary>
    /// This is an AsyncTask that is important and should never be dropped by the
    /// scheduler when it reorders its task queue. However, multiple instances of
    /// this AsyncTask needs to be prioritized, based on their Priority value.
    /// </summary>
    /// 
    class PrioritizedAsyncTask : SampleAsyncTask
    {
        internal int Priority { get; private set; }

        internal PrioritizedAsyncTask(DynamoScheduler scheduler, int priority)
            : base(scheduler)
        {
            Priority = priority; // Assign task priority.
        }

        public override string ToString()
        {
            return "PrioritizedAsyncTask: " + Priority;
        }

        protected override void ExecuteCore()
        {
            // Task execution results in string added to list.
            AddToResultList(ToString());
        }

        protected override int CompareCore(AsyncTask otherTask)
        {
            // PrioritizedAsyncTask always come before InconsequentialAsyncTask.
            if (otherTask is InconsequentialAsyncTask)
                return -1;

            // It is not compared with another PrioritizedAsyncTask, fall back.
            var task = otherTask as PrioritizedAsyncTask;
            if (task == null)
                return base.CompareCore(otherTask);

            // Priority 1 tasks always come before priority 2 tasks.
            return Priority - task.Priority;
        }
    }

    /// <summary>
    /// InconsequentialAsyncTask represents a task that is not important. If two 
    /// such tasks are compared, only one that carries a bigger punch will be kept.
    /// </summary>
    /// 
    class InconsequentialAsyncTask : SampleAsyncTask
    {
        internal int Punch { get; private set; }

        internal InconsequentialAsyncTask(DynamoScheduler scheduler, int punch)
            : base(scheduler)
        {
            Punch = punch;
        }

        public override string ToString()
        {
            return "InconsequentialAsyncTask: " + Punch;
        }

        protected override void ExecuteCore()
        {
            // Task execution results in string added to list.
            AddToResultList(ToString());
        }

        protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            var task = otherTask as InconsequentialAsyncTask;
            if (task == null)
                return base.CanMergeWithCore(otherTask);

            // The comparison only keeps the task that carries a bigger punch.
            if (Punch > task.Punch)
                return TaskMergeInstruction.KeepThis;

            return TaskMergeInstruction.KeepOther;
        }

        protected override int CompareCore(AsyncTask otherTask)
        {
            // PrioritizedAsyncTask always come before InconsequentialAsyncTask.
            if (otherTask is PrioritizedAsyncTask)
                return 1;

            // InconsequentialAsyncTask are always treated equal.
            return base.CompareCore(otherTask);
        }
    }

    class TimeStampGrabber
    {
        private ManualResetEvent doneEvent = null;

        internal TimeStampGrabber(ManualResetEvent doneEvent)
        {
            this.doneEvent = doneEvent;
        }

        internal void GrabTimeStamp(DynamoScheduler scheduler)
        {
            // Get the time-stamp value from the scheduler.
            this.TimeStampValue = scheduler.NextTimeStamp;
            this.doneEvent.Set(); // Done with grabbing.
        }

        internal TimeStamp TimeStampValue { get; private set; }
    }

    #endregion

    public class SchedulerTests
    {
        #region TimeStampGenerator Related Tests Cases

        /// <summary>
        /// This test ensures that DynamoScheduler returns an
        /// incremental set of long values starting with 1024.
        /// </summary>
        /// 
        [Test]
        [Category("UnitTests")]
        public void TimeStampGenerator00()
        {
            var scheduler = new DynamoScheduler(new SampleSchedulerThread());
            Assert.AreEqual(1024, scheduler.NextTimeStamp.Identifier);
            Assert.AreEqual(1025, scheduler.NextTimeStamp.Identifier);
            Assert.AreEqual(1026, scheduler.NextTimeStamp.Identifier);
        }

        /// <summary>
        /// This test makes use of the built-in thread pool, creating a bunch
        /// of work items that attempt to grab a time-stamp value out of scheduler 
        /// simultaneously. It tests and makes sure that the built-in lock 
        /// mechanism avoids concurrent calls to DynamoScheduler.NextTimeStamp 
        /// from getting the same time-stamp value.
        /// </summary>
        /// 
        [Test, RequiresMTA]
        [Category("UnitTests")]
        public void TimeStampGenerator01()
        {
            const int EventCount = 16;
            var events = new ManualResetEvent[EventCount];
            var grabbers = new TimeStampGrabber[EventCount];

            // Initialize events and time stamp grabbers.
            for (int index = 0; index < EventCount; ++index)
            {
                events[index] = new ManualResetEvent(false);
                grabbers[index] = new TimeStampGrabber(events[index]);
            }

            // Start all time-stamp grabbers "at one go".
            var scheduler = new DynamoScheduler(new SampleSchedulerThread());
            Parallel.For(0, EventCount, ((index) =>
            {
                grabbers[index].GrabTimeStamp(scheduler);
            }));

            WaitHandle.WaitAll(events);
            var values = new List<TimeStamp>();
            for (int index = 0; index < EventCount; ++index)
                values.Add(grabbers[index].TimeStampValue);

            // Ensure we get a list of time stamps, and that these numbers are 
            // all unique (i.e. the distinct set of numbers returned should have
            // the same count as the original list).
            // 
            Assert.AreEqual(EventCount, values.Count);
            var distinct = values.Distinct();
            Assert.AreEqual(values.Count, distinct.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void TimeStampGenerator02()
        {
            var generator = new TimeStampGenerator();

            TimeStamp first = generator.Next;
            TimeStamp next = generator.Next;
            TimeStamp nextnext = generator.Next;

            Assert.IsTrue(first < next);
            Assert.IsTrue(first < nextnext);
            Assert.IsTrue(next < nextnext);

            Assert.IsTrue(next > first);
            Assert.IsTrue(nextnext > first);
            Assert.IsTrue(nextnext > next);

            // Alisasing the variable is necessary to prevent 
            // the compiler's naive comparison check preventing this
            TimeStamp a = first;
            Assert.IsFalse(a > first);
            Assert.IsFalse(a < first);
            Assert.IsTrue(a.Equals(first));
            Assert.IsTrue(first.Equals(a));

            TimeStamp b = next;
            Assert.IsFalse(b > next);
            Assert.IsFalse(b < next);
            Assert.IsTrue(b.Equals(next));
            Assert.IsTrue(next.Equals(b));
        }

        #endregion

        #region AsyncTask Related Test Cases

        [Test]
        [Category("UnitTests")]
        public void AsyncTask00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // The first argument cannot be null.
                var task = new SampleAsyncTask(null);
            });
        }

        #endregion

        #region Scheduler Related Test Cases

        /// <summary>
        /// Test scenario when various task types are interleaving one another.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing00()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new InconsequentialAsyncTask(scheduler, 500),
                new PrioritizedAsyncTask(scheduler, 3),
                new InconsequentialAsyncTask(scheduler, 100),
                new PrioritizedAsyncTask(scheduler, 5), 
                new InconsequentialAsyncTask(scheduler, 300),
                new PrioritizedAsyncTask(scheduler, 1), 
                new InconsequentialAsyncTask(scheduler, 200),
                new PrioritizedAsyncTask(scheduler, 6), 
                new InconsequentialAsyncTask(scheduler, 700),
                new PrioritizedAsyncTask(scheduler, 2), 
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(6, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
            Assert.AreEqual("PrioritizedAsyncTask: 2", results[1]);
            Assert.AreEqual("PrioritizedAsyncTask: 3", results[2]);
            Assert.AreEqual("PrioritizedAsyncTask: 5", results[3]);
            Assert.AreEqual("PrioritizedAsyncTask: 6", results[4]);
            Assert.AreEqual("InconsequentialAsyncTask: 700", results[5]);
        }

        /// <summary>
        /// All prioritized tasks are already in correct sequence, with the 
        /// last inconsequential task being the ultimate choice.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing01()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new PrioritizedAsyncTask(scheduler, 1), 
                new PrioritizedAsyncTask(scheduler, 2), 
                new PrioritizedAsyncTask(scheduler, 3),
                new PrioritizedAsyncTask(scheduler, 5), 
                new PrioritizedAsyncTask(scheduler, 6), 
                new InconsequentialAsyncTask(scheduler, 100),
                new InconsequentialAsyncTask(scheduler, 200),
                new InconsequentialAsyncTask(scheduler, 300),
                new InconsequentialAsyncTask(scheduler, 500),
                new InconsequentialAsyncTask(scheduler, 700),
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(6, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
            Assert.AreEqual("PrioritizedAsyncTask: 2", results[1]);
            Assert.AreEqual("PrioritizedAsyncTask: 3", results[2]);
            Assert.AreEqual("PrioritizedAsyncTask: 5", results[3]);
            Assert.AreEqual("PrioritizedAsyncTask: 6", results[4]);
            Assert.AreEqual("InconsequentialAsyncTask: 700", results[5]);
        }

        /// <summary>
        /// All prioritized tasks are already in correct sequence, with the 
        /// first inconsequential task being the ultimate choice.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing02()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new PrioritizedAsyncTask(scheduler, 1), 
                new PrioritizedAsyncTask(scheduler, 2), 
                new PrioritizedAsyncTask(scheduler, 3),
                new PrioritizedAsyncTask(scheduler, 5), 
                new PrioritizedAsyncTask(scheduler, 6), 
                new InconsequentialAsyncTask(scheduler, 700),
                new InconsequentialAsyncTask(scheduler, 500),
                new InconsequentialAsyncTask(scheduler, 300),
                new InconsequentialAsyncTask(scheduler, 200),
                new InconsequentialAsyncTask(scheduler, 100),
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(6, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
            Assert.AreEqual("PrioritizedAsyncTask: 2", results[1]);
            Assert.AreEqual("PrioritizedAsyncTask: 3", results[2]);
            Assert.AreEqual("PrioritizedAsyncTask: 5", results[3]);
            Assert.AreEqual("PrioritizedAsyncTask: 6", results[4]);
            Assert.AreEqual("InconsequentialAsyncTask: 700", results[5]);
        }

        /// <summary>
        /// Test scenario when scheduler thread tries to 
        /// pick up a task when there is only one scheduled.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing03()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new PrioritizedAsyncTask(scheduler, 1), 
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
        }

        /// <summary>
        /// Test scenario when scheduler thread tries to pick up a 
        /// task when there are only two scheduled (in the wrong order).
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing04()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new InconsequentialAsyncTask(scheduler, 100),
                new PrioritizedAsyncTask(scheduler, 1), 
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
            Assert.AreEqual("InconsequentialAsyncTask: 100", results[1]);
        }

        /// <summary>
        /// Test scenario when scheduler thread tries to pick up a 
        /// task when there are only two scheduled (in the right order).
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing05()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new PrioritizedAsyncTask(scheduler, 1), 
                new InconsequentialAsyncTask(scheduler, 100),
            };

            var results = new List<string>();
            foreach (SampleAsyncTask asyncTask in asyncTasks)
            {
                asyncTask.InitializeWithResultList(results);
                scheduler.ScheduleForExecution(asyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("PrioritizedAsyncTask: 1", results[0]);
            Assert.AreEqual("InconsequentialAsyncTask: 100", results[1]);
        }

        /// <summary>
        /// Test scenario when scheduler thread tries to 
        /// pick up a task when there is none scheduled.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing06()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);

            schedulerThread.GetSchedulerToProcessTasks();
            Assert.Pass("Scheduler thread successfully exits");
        }

        /// <summary>
        /// Test scenario when various task types are interleaving one another.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskStateChangedEventHandling()
        {
            var observer = new TaskEventObserver();
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread);
            scheduler.TaskStateChanged += observer.OnTaskStateChanged;

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new InconsequentialAsyncTask(scheduler, 500),
                new PrioritizedAsyncTask(scheduler, 3),
                new InconsequentialAsyncTask(scheduler, 100),
                new PrioritizedAsyncTask(scheduler, 5), 
            };

            foreach (SampleAsyncTask asyncTask in asyncTasks)
                scheduler.ScheduleForExecution(asyncTask);

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            var expected = new List<string>
            {
                "Scheduled: InconsequentialAsyncTask: 500",
                "Scheduled: PrioritizedAsyncTask: 3",
                "Scheduled: InconsequentialAsyncTask: 100",
                "Scheduled: PrioritizedAsyncTask: 5",
                "Discarded: InconsequentialAsyncTask: 100",
                "ExecutionStarting: PrioritizedAsyncTask: 3",
                "ExecutionCompleted: PrioritizedAsyncTask: 3",
                "CompletionHandled: PrioritizedAsyncTask: 3",
                "ExecutionStarting: PrioritizedAsyncTask: 5",
                "ExecutionCompleted: PrioritizedAsyncTask: 5",
                "CompletionHandled: PrioritizedAsyncTask: 5",
                "ExecutionStarting: InconsequentialAsyncTask: 500",
                "ExecutionCompleted: InconsequentialAsyncTask: 500",
                "CompletionHandled: InconsequentialAsyncTask: 500"
            };

            Assert.AreEqual(expected.Count, observer.Results.Count());

            int index = 0;
            foreach (var actual in observer.Results)
            {
                Assert.AreEqual(expected[index++], actual);
            }
        }

        #endregion

        #region Private Test Helper Methods

        #endregion
    }
}
