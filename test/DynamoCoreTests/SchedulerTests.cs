using Dynamo.Core;
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
    #region Mock Classes for Test Cases

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
    }

    class SampleAsyncTask : AsyncTask
    {
        internal SampleAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        protected override void ExecuteCore()
        {
        }

        protected override void HandleTaskCompletionCore()
        {
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

        #region Private Test Helper Methods

        #endregion
    }
}
