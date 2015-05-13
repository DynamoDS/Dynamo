using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core.Threading;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class AsyncTaskExtensionsTests
    {
        [Test]
        public void AllComplete_ExecutesCallbackImmediatelyWhenPassedZeroLength()
        {
            var run = false;

            var l = new List<AsyncTask>();

            l.AllComplete((_) =>
            {
                run = true;
            });

            Assert.True(run);
        }

        [Test]
        public void AllComplete_ExecutesCallbackWhenAllAreCompleted()
        {
            var run = false;

            var s = MockMaker.Empty<IScheduler>();

            var t1 = new DelegateBasedAsyncTask(s );
            var t2 = new DelegateBasedAsyncTask(s);

            var l = new List<AsyncTask>()
            {
                t1,
                t2
            };

            l.AllComplete((_) =>
            {
                run = true;
            });

            t1.HandleTaskCompletion();
            t2.HandleTaskCompletion();

            Assert.True(run);
        }

        [Test]
        public void AllComplete_ExecutesCallbackWhenAllAreDiscarded()
        {
            var run = false;

            var s = MockMaker.Empty<IScheduler>();

            var t1 = new DelegateBasedAsyncTask(s);
            var t2 = new DelegateBasedAsyncTask(s);

            var l = new List<AsyncTask>()
            {
                t1,
                t2
            };

            l.AllComplete((_) =>
            {
                run = true;
            });

            t1.HandleTaskDiscarded();
            t2.HandleTaskDiscarded();

            Assert.True(run);
        }

        [Test]
        public void AllComplete_ExecutesCallbackWhenSomeDiscardedSomeCompleted()
        {
            var run = false;

            var s = MockMaker.Empty<IScheduler>();

            var t1 = new DelegateBasedAsyncTask(s);
            var t2 = new DelegateBasedAsyncTask(s);

            var l = new List<AsyncTask>()
            {
                t1,
                t2
            };

            l.AllComplete((_) =>
            {
                run = true;
            });

            t1.HandleTaskDiscarded();
            t2.HandleTaskCompletion();

            Assert.True(run);
        }

        [Test]
        public void AllComplete_ReturnsDisposableThatDisposesAllRegistrations()
        {
            var run = false;

            var s = MockMaker.Empty<IScheduler>();

            var t1 = new DelegateBasedAsyncTask(s);
            var t2 = new DelegateBasedAsyncTask(s);

            var l = new List<AsyncTask>()
            {
                t1,
                t2
            };

            l.AllComplete((_) =>
            {
                run = true;
            }).Dispose();

            t1.HandleTaskCompletion();
            t2.HandleTaskDiscarded();

            Assert.False(run);
        }
    }
}
