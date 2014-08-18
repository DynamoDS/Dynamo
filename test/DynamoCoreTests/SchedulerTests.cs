using Dynamo.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo
{
    #region Mock Classes for Test Cases

    class SampleAsyncTask : AsyncTask
    {
        internal SampleAsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
            : base(scheduler, callback)
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

        internal long TimeStampValue { get; private set; }
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
        public void TimeStampGenerator00()
        {
            var scheduler = new DynamoScheduler();
            Assert.AreEqual(1024, scheduler.NextTimeStamp);
            Assert.AreEqual(1025, scheduler.NextTimeStamp);
            Assert.AreEqual(1026, scheduler.NextTimeStamp);
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
            var scheduler = new DynamoScheduler();
            Parallel.For(0, EventCount, ((index) =>
            {
                grabbers[index].GrabTimeStamp(scheduler);
            }));

            WaitHandle.WaitAll(events);
            var values = new List<long>();
            for (int index = 0; index < EventCount; ++index)
                values.Add(grabbers[index].TimeStampValue);

            // Ensure we get a list of numbers, and that these numbers are all 
            // unique (i.e. the distinct set of numbers returned should have the 
            // same count as the original list).
            // 
            Assert.AreEqual(EventCount, values.Count);
            var distinct = values.Distinct();
            Assert.AreEqual(values.Count, distinct.Count());
        }

        #endregion

        #region AsyncTask Related Test Cases

        [Test]
        public void AsyncTask00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // The first argument cannot be null.
                var task = new SampleAsyncTask(null, null);
            });

            var dummyCallback = new Action<AsyncTask>((task) =>
            {
                // Dummy callback method that does not do anything.
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Exception is thrown regardless of the second parameter.
                var task = new SampleAsyncTask(null, dummyCallback);
            });
        }

        #endregion

        #region Private Test Helper Methods

        #endregion
    }
}
