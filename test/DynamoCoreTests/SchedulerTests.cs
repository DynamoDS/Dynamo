using System.IO;
using System.Reflection;

using DSCoreNodesUI;

using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using DynamoShapeManager;
using DynamoUtilities;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ProtoCore.AST;
using ProtoCore.DSASM;

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
        private IScheduler scheduler;

        internal bool Initialized { get; private set; }
        internal bool Destroyed { get; private set; }

        public void Initialize(IScheduler owningScheduler)
        {
            scheduler = owningScheduler;
            Initialized = true;
        }

        public void Shutdown()
        {
            Destroyed = true;
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

        override internal TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

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

        protected override void HandleTaskExecutionCore()
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
        internal int CurrPriority { get; private set; }

        override internal TaskPriority Priority
        {
            get { return TaskPriority.AboveNormal; }
        }

        internal PrioritizedAsyncTask(DynamoScheduler scheduler, int priority)
            : base(scheduler)
        {
            CurrPriority = priority; // Assign task priority.
        }

        public override string ToString()
        {
            return "PrioritizedAsyncTask: " + CurrPriority;
        }

        protected override void HandleTaskExecutionCore()
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
            return CurrPriority - task.CurrPriority;
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

        override internal TaskPriority Priority
        {
            get { return TaskPriority.BelowNormal; }
        }

        internal InconsequentialAsyncTask(DynamoScheduler scheduler, int punch)
            : base(scheduler)
        {
            Punch = punch;
        }

        public override string ToString()
        {
            return "InconsequentialAsyncTask: " + Punch;
        }

        protected override void HandleTaskExecutionCore()
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

    /// <summary>
    /// This task, when executed by DynamoScheduler, throws an exception. It is 
    /// used to verify that TaskStateChangedEventArgs.State.ExecutionFailed is 
    /// promptly sent out to DynamoScheduler.TaskStateChanged event handler.
    /// </summary>
    /// 
    class ErrorProneAsyncTask : SampleAsyncTask
    {
        internal int Value { get; private set; }

        override internal TaskPriority Priority
        {
            // Same as PrioritizedAsyncTask.
            get { return TaskPriority.AboveNormal; }
        }

        internal ErrorProneAsyncTask(DynamoScheduler scheduler, int value)
            : base(scheduler)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "ErrorProneAsyncTask: " + Value;
        }

        protected override void HandleTaskExecutionCore()
        {
            // Throws an exception when executed.
            throw new InvalidOperationException();
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

    #region Built-in AsyncTask Test Classes

    class FakeAsyncTaskData
    {
        private static int currentSerialNumber = 0;
        private readonly int serialNumber;

        internal DynamoScheduler Scheduler { get; set; }
        internal List<string> Results { get; set; }

        internal static void ResetSerialNumber()
        {
            currentSerialNumber = 0;
        }

        internal FakeAsyncTaskData()
        {
            serialNumber = currentSerialNumber++;
        }

        internal void WriteExecutionLog(AsyncTask asyncTask)
        {
            var name = asyncTask.GetType().Name;
            Results.Add(string.Format("{0}: {1}", name, serialNumber));
        }
    }

    internal class FakeAggregateRenderPackageAsyncTask : AggregateRenderPackageAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal Guid TargetedNodeId { set { targetedNodeId = value; } }

        internal FakeAggregateRenderPackageAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeCompileCustomNodeAsyncTask : CompileCustomNodeAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeCompileCustomNodeAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeDelegateBasedAsyncTask : DelegateBasedAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeDelegateBasedAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeNotifyRenderPackagesReadyAsyncTask : NotifyRenderPackagesReadyAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeNotifyRenderPackagesReadyAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeQueryMirrorDataAsyncTask : QueryMirrorDataAsyncTask
    {
        private FakeAsyncTaskData data;

        internal FakeQueryMirrorDataAsyncTask(QueryMirrorDataParams initParams)
            : base(initParams)
        {
        }

        internal void Initialize(FakeAsyncTaskData data)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeSetTraceDataAsyncTask : SetTraceDataAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeSetTraceDataAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
    }

    internal class FakeUpdateGraphAsyncTask : UpdateGraphAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeUpdateGraphAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler, false)
        {
            this.data = data;
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }

        protected override void HandleTaskCompletionCore()
        {
            // Avoid base method which access EngineController.
        }
    }

    internal class FakeUpdateRenderPackageAsyncTask : UpdateRenderPackageAsyncTask
    {
        private readonly FakeAsyncTaskData data;

        internal FakeUpdateRenderPackageAsyncTask(FakeAsyncTaskData data)
            : base(data.Scheduler)
        {
            this.data = data;
        }

        internal Guid NodeGuid
        {
            set { nodeGuid = value; }
        }

        protected override void HandleTaskExecutionCore()
        {
            data.WriteExecutionLog(this);
        }
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
            var scheduler = new DynamoScheduler(new SampleSchedulerThread(), true);
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
            var scheduler = new DynamoScheduler(new SampleSchedulerThread(), true);
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
        /// Ensure that DynamoScheduler.Shutdown properly initializes
        /// and destroys the associated ISchedulerThread.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestSchedulerCreationDestruction()
        {
            var schedulerThread = new SampleSchedulerThread();
            Assert.IsFalse(schedulerThread.Initialized);
            Assert.IsFalse(schedulerThread.Destroyed);

            var scheduler = new DynamoScheduler(schedulerThread, false);
            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsFalse(schedulerThread.Destroyed);

            scheduler.Shutdown();
            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsTrue(schedulerThread.Destroyed);
        }

        /// <summary>
        /// Test scenario when various task types are interleaving one another.
        /// </summary>
        /// 
        [Test, Category("UnitTests")]
        public void TestTaskQueuePreProcessing00()
        {
            var schedulerThread = new SampleSchedulerThread();
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);

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
            var scheduler = new DynamoScheduler(schedulerThread, false);
            scheduler.TaskStateChanged += observer.OnTaskStateChanged;

            // Start scheduling a bunch of tasks.
            var asyncTasks = new AsyncTask[]
            {
                new ErrorProneAsyncTask(scheduler, 7), 
                new InconsequentialAsyncTask(scheduler, 100),
                new PrioritizedAsyncTask(scheduler, 1),
                new PrioritizedAsyncTask(scheduler, 5),
                new ErrorProneAsyncTask(scheduler, 3), 
                new InconsequentialAsyncTask(scheduler, 500),
                new InconsequentialAsyncTask(scheduler, 300),
                new PrioritizedAsyncTask(scheduler, 3), 
                new ErrorProneAsyncTask(scheduler, 5), 
            };

            foreach (SampleAsyncTask asyncTask in asyncTasks)
                scheduler.ScheduleForExecution(asyncTask);

            schedulerThread.GetSchedulerToProcessTasks();

            // Drops all InconsequentialAsyncTask and leave behind one.
            // Kept all PrioritizedAsyncTask instances and sorted them.
            var expected = new List<string>
            {
                // Scheduling notifications...

                "Scheduled: ErrorProneAsyncTask: 7",
                "Scheduled: InconsequentialAsyncTask: 100",
                "Scheduled: PrioritizedAsyncTask: 1",
                "Scheduled: PrioritizedAsyncTask: 5",
                "Scheduled: ErrorProneAsyncTask: 3",
                "Scheduled: InconsequentialAsyncTask: 500",
                "Scheduled: InconsequentialAsyncTask: 300",
                "Scheduled: PrioritizedAsyncTask: 3",
                "Scheduled: ErrorProneAsyncTask: 5",

                // Task discarded notifications...

                "Discarded: InconsequentialAsyncTask: 100",
                "Discarded: InconsequentialAsyncTask: 300",

                // Execution of remaining tasks...

                "ExecutionStarting: ErrorProneAsyncTask: 7",
                "ExecutionFailed: ErrorProneAsyncTask: 7",
                "CompletionHandled: ErrorProneAsyncTask: 7",

                "ExecutionStarting: PrioritizedAsyncTask: 1",
                "ExecutionCompleted: PrioritizedAsyncTask: 1",
                "CompletionHandled: PrioritizedAsyncTask: 1",

                "ExecutionStarting: PrioritizedAsyncTask: 5",
                "ExecutionCompleted: PrioritizedAsyncTask: 5",
                "CompletionHandled: PrioritizedAsyncTask: 5",

                "ExecutionStarting: ErrorProneAsyncTask: 3",
                "ExecutionFailed: ErrorProneAsyncTask: 3",
                "CompletionHandled: ErrorProneAsyncTask: 3",

                "ExecutionStarting: PrioritizedAsyncTask: 3",
                "ExecutionCompleted: PrioritizedAsyncTask: 3",
                "CompletionHandled: PrioritizedAsyncTask: 3",

                "ExecutionStarting: ErrorProneAsyncTask: 5",
                "ExecutionFailed: ErrorProneAsyncTask: 5",
                "CompletionHandled: ErrorProneAsyncTask: 5",

                // Execution of InconsequentialAsyncTask last...

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
    }

    /// <summary>
    /// Most AsyncTask derived classes require NodeModel, WorkspaceModel or 
    /// DynamoModel to work. For these cases, a DynamoModel is created to 
    /// facilitate the creation of such model classes. Note that however, in 
    /// order to allow actual task scheduling in "DynamoModel.scheduler", 
    /// "DynamoModel.IsTestMode" is set to "false".
    /// </summary>
    /// 
    public class SchedulerIntegrationTests : UnitTestBase
    {
        private DynamoModel dynamoModel;
        private Preloader preloader;
        private SampleSchedulerThread schedulerThread;
        private List<string> results;

        #region Integration Test Cases

        /// <summary>
        /// Test that shutting down DynamoModel correctly shuts down the
        /// DynamoScheduler, which in turn shuts down ISchedulerThread.
        /// </summary>
        /// 
        [Test]
        public void TestShutdownWithDynamoModel00()
        {
            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsFalse(schedulerThread.Destroyed);

            dynamoModel.ShutDown(false); // Shutting down Dynamo scenario.
            dynamoModel = null; // Nullify so we don't shutdown twice.

            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsTrue(schedulerThread.Destroyed);
        }

        /// <summary>
        /// Test that shutting down DynamoModel correctly shuts down the
        /// DynamoScheduler, which in turn shuts down ISchedulerThread.
        /// </summary>
        /// 
        [Test]
        public void TestShutdownWithDynamoModel01()
        {
            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsFalse(schedulerThread.Destroyed);

            dynamoModel.ShutDown(true); // Shutting down host scenario.
            dynamoModel = null; // Nullify so we don't shutdown twice.

            Assert.IsTrue(schedulerThread.Initialized);
            Assert.IsTrue(schedulerThread.Destroyed);
        }

        [Test]
        public void TestTaskQueuePreProcessing00()
        {
            var nodes = CreateBaseNodes().ToArray();
            var tasksToSchedule = new List<AsyncTask>()
            {
                // Query value for a given named variable.
                MakeQueryMirrorDataAsyncTask("variableOne"),

                // This older task is kept because it wasn't re-scheduled.
                MakeUpdateRenderPackageAsyncTask(nodes[0].GUID),

                // These older tasks are to be dropped.
                MakeUpdateRenderPackageAsyncTask(nodes[1].GUID),
                MakeUpdateRenderPackageAsyncTask(nodes[2].GUID),

                // These older tasks are to be dropped.
                MakeNotifyRenderPackagesReadyAsyncTask(),
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),

                // This higher priority task moves to the front.
                MakeUpdateGraphAsyncTask(),

                // Query value for a given named variable.
                MakeQueryMirrorDataAsyncTask("variableOne"),

                // These newer tasks will be kept.
                MakeUpdateRenderPackageAsyncTask(nodes[1].GUID),
                MakeUpdateRenderPackageAsyncTask(nodes[2].GUID),

                // This higher priority task moves to the front.
                MakeUpdateGraphAsyncTask(),

                // These newer tasks will be kept.
                MakeNotifyRenderPackagesReadyAsyncTask(),
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),
            };

            var scheduler = dynamoModel.Scheduler;
            foreach (var stubAsyncTask in tasksToSchedule)
            {
                scheduler.ScheduleForExecution(stubAsyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            var expected = new List<string>
            {
                "FakeUpdateGraphAsyncTask: 6",
                "FakeUpdateGraphAsyncTask: 10",
                "FakeQueryMirrorDataAsyncTask: 0",
                "FakeUpdateRenderPackageAsyncTask: 1",
                "FakeQueryMirrorDataAsyncTask: 7",
                "FakeUpdateRenderPackageAsyncTask: 8",
                "FakeUpdateRenderPackageAsyncTask: 9",
                "FakeNotifyRenderPackagesReadyAsyncTask: 11",
                "FakeAggregateRenderPackageAsyncTask: 12",
            };

            Assert.AreEqual(expected.Count, results.Count);

            int index = 0;
            foreach (var actual in results)
            {
                Assert.AreEqual(expected[index++], actual);
            }
        }

        [Test]
        public void TestTaskQueuePreProcessing01()
        {
            // Everything is scheduled in order of priority.
            var tasksToSchedule = new List<AsyncTask>()
            {
                MakeSetTraceDataAsyncTask(),                        // Highest
                MakeCompileCustomNodeAsyncTask(),                   // Above normal
                MakeUpdateGraphAsyncTask(),                         // Above normal
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),    // Normal
                MakeDelegateBasedAsyncTask(),                       // Normal
                MakeQueryMirrorDataAsyncTask("variableName"),       // Normal
                MakeNotifyRenderPackagesReadyAsyncTask(),           // Normal
                MakeUpdateRenderPackageAsyncTask(Guid.NewGuid()),   // Normal
            };

            var scheduler = dynamoModel.Scheduler;
            foreach (var stubAsyncTask in tasksToSchedule)
            {
                scheduler.ScheduleForExecution(stubAsyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            var expected = new List<string>
            {
                "FakeSetTraceDataAsyncTask: 0",
                "FakeCompileCustomNodeAsyncTask: 1",
                "FakeUpdateGraphAsyncTask: 2",
                "FakeAggregateRenderPackageAsyncTask: 3",
                "FakeDelegateBasedAsyncTask: 4",
                "FakeQueryMirrorDataAsyncTask: 5",
                "FakeNotifyRenderPackagesReadyAsyncTask: 6",
                "FakeUpdateRenderPackageAsyncTask: 7"
            };

            Assert.AreEqual(expected.Count, results.Count);

            int index = 0;
            foreach (var actual in results)
            {
                Assert.AreEqual(expected[index++], actual);
            }
        }

        [Test]
        public void TestTaskQueuePreProcessing02()
        {
            // Everything is scheduled in reversed order of priority.
            var tasksToSchedule = new List<AsyncTask>()
            {
                MakeUpdateRenderPackageAsyncTask(Guid.NewGuid()),   // Normal
                MakeNotifyRenderPackagesReadyAsyncTask(),           // Normal
                MakeQueryMirrorDataAsyncTask("variableName"),       // Normal
                MakeDelegateBasedAsyncTask(),                       // Normal
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),    // Normal
                MakeUpdateGraphAsyncTask(),                         // Above normal
                MakeCompileCustomNodeAsyncTask(),                   // Above normal
                MakeSetTraceDataAsyncTask(),                        // Highest
            };

            var scheduler = dynamoModel.Scheduler;
            foreach (var stubAsyncTask in tasksToSchedule)
            {
                scheduler.ScheduleForExecution(stubAsyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            var expected = new List<string>
            {
                "FakeSetTraceDataAsyncTask: 7",
                "FakeUpdateGraphAsyncTask: 5",
                "FakeCompileCustomNodeAsyncTask: 6",
                "FakeUpdateRenderPackageAsyncTask: 0",
                "FakeNotifyRenderPackagesReadyAsyncTask: 1",
                "FakeQueryMirrorDataAsyncTask: 2",
                "FakeDelegateBasedAsyncTask: 3",
                "FakeAggregateRenderPackageAsyncTask: 4",
            };

            Assert.AreEqual(expected.Count, results.Count);

            int index = 0;
            foreach (var actual in results)
            {
                Assert.AreEqual(expected[index++], actual);
            }
        }

        [Test]
        public void TestTaskQueuePreProcessing03()
        {
            var specificGuid = Guid.NewGuid();

            // Everything is scheduled in reversed order of priority.
            var tasksToSchedule = new List<AsyncTask>()
            {
                MakeUpdateRenderPackageAsyncTask(Guid.NewGuid()),   // Normal
                MakeNotifyRenderPackagesReadyAsyncTask(),           // Normal
                MakeDelegateBasedAsyncTask(),                       // Normal
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),    // Normal
                MakeAggregateRenderPackageAsyncTask(specificGuid),  // Normal
                MakeQueryMirrorDataAsyncTask("variableName"),       // Normal
                MakeAggregateRenderPackageAsyncTask(Guid.Empty),    // Normal
                MakeAggregateRenderPackageAsyncTask(specificGuid),  // Normal
                MakeQueryMirrorDataAsyncTask("variableName"),       // Normal
                MakeUpdateGraphAsyncTask(),                         // Above normal
                MakeCompileCustomNodeAsyncTask(),                   // Above normal
                MakeSetTraceDataAsyncTask(),                        // Highest
            };

            var scheduler = dynamoModel.Scheduler;
            foreach (var stubAsyncTask in tasksToSchedule)
            {
                scheduler.ScheduleForExecution(stubAsyncTask);
            }

            schedulerThread.GetSchedulerToProcessTasks();

            var expected = new List<string>
            {
                "FakeSetTraceDataAsyncTask: 11",
                "FakeUpdateGraphAsyncTask: 9",
                "FakeCompileCustomNodeAsyncTask: 10",
                "FakeUpdateRenderPackageAsyncTask: 0",
                "FakeNotifyRenderPackagesReadyAsyncTask: 1",
                "FakeDelegateBasedAsyncTask: 2",
                "FakeQueryMirrorDataAsyncTask: 5",
                "FakeAggregateRenderPackageAsyncTask: 6",
                "FakeAggregateRenderPackageAsyncTask: 7",
                "FakeQueryMirrorDataAsyncTask: 8",
            };

            Assert.AreEqual(expected.Count, results.Count);

            int index = 0;
            foreach (var actual in results)
            {
                Assert.AreEqual(expected[index++], actual);
            }
        }

        #endregion

        #region Test Setup, TearDown, Helper Methods

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            StartDynamo();

            FakeAsyncTaskData.ResetSerialNumber();
            results = new List<string>();
        }

        public override void Cleanup()
        {
            if (dynamoModel != null)
            {
                dynamoModel.ShutDown(false);
                dynamoModel = null;
            }

            preloader = null;
            base.Cleanup();
        }

        protected void StartDynamo()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));
            preloader.Preload();

            schedulerThread = new SampleSchedulerThread();
            dynamoModel = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    // See documentation for 'SchedulerIntegrationTests' above.
                    StartInTestMode = false,
                    SchedulerThread = schedulerThread,
                    GeometryFactoryPath = preloader.GeometryFactoryPath
                });
        }

        private IEnumerable<NodeModel> CreateBaseNodes()
        {
            var nodes = new List<NodeModel>
            {
                new DoubleInput(),
                new DoubleInput(),
                new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"))
            };

            var workspace = dynamoModel.CurrentWorkspace;
            foreach (var node in nodes)
            {
                workspace.AddNode(node, false);
            }

            Assert.AreEqual(3, workspace.Nodes.Count);
            return nodes;
        }

        #endregion

        #region AsyncTask Class Creation Methods

        private AsyncTask MakeAggregateRenderPackageAsyncTask(Guid nodeGuid)
        {
            return new FakeAggregateRenderPackageAsyncTask(MakeAsyncTaskData())
            {
                TargetedNodeId = nodeGuid
            };
        }

        private AsyncTask MakeCompileCustomNodeAsyncTask()
        {
            return new FakeCompileCustomNodeAsyncTask(MakeAsyncTaskData());
        }

        private AsyncTask MakeDelegateBasedAsyncTask()
        {
            return new FakeDelegateBasedAsyncTask(MakeAsyncTaskData());
        }

        private AsyncTask MakeNotifyRenderPackagesReadyAsyncTask()
        {
            return new FakeNotifyRenderPackagesReadyAsyncTask(MakeAsyncTaskData());
        }

        private AsyncTask MakeQueryMirrorDataAsyncTask(string variableName)
        {
            var task = new FakeQueryMirrorDataAsyncTask(
                new QueryMirrorDataParams()
                {
                    Scheduler = dynamoModel.Scheduler,
                    EngineController = dynamoModel.EngineController,
                    VariableName = variableName
                });

            task.Initialize(MakeAsyncTaskData());
            return task;
        }

        private AsyncTask MakeSetTraceDataAsyncTask()
        {
            return new FakeSetTraceDataAsyncTask(MakeAsyncTaskData());
        }

        private AsyncTask MakeUpdateGraphAsyncTask()
        {
            return new FakeUpdateGraphAsyncTask(MakeAsyncTaskData());
        }

        private AsyncTask MakeUpdateRenderPackageAsyncTask(Guid nodeGuid)
        {
            return new FakeUpdateRenderPackageAsyncTask(MakeAsyncTaskData())
            {
                NodeGuid = nodeGuid
            };
        }

        private FakeAsyncTaskData MakeAsyncTaskData()
        {
            return new FakeAsyncTaskData()
            {
                Scheduler = dynamoModel.Scheduler,
                Results = results
            };
        }

        #endregion
    }
}
