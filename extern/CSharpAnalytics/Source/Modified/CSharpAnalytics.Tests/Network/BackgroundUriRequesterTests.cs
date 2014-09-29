using CSharpAnalytics.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class BackgroundUriRequesterTests
    {
        [TestMethod]
        public void BackgroundUriRequester_Start_Uses_Previous_List()
        {
            var expectedList = TestHelpers.CreateRequestList(4);
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundUriRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList);

            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expectedList.Count, actualList.Count);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundUriRequester_Start_Uses_Previous_List_First()
        {
            var expectedList = TestHelpers.CreateRequestList(10);
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundUriRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList.Take(5));
            foreach (var uri in expectedList.Skip(5))
                requester.Add(uri);

            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expectedList.Count, actualList.Count);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundUriRequester_StopAsync_Stops_Requesting()
        {
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundUriRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10));

            Task.Delay(1000, CancellationToken.None).GetAwaiter().GetResult();

            requester.StopAsync().Wait(CancellationToken.None);
            foreach (var uri in TestHelpers.CreateRequestList(3))
                requester.Add(uri);

            Assert.AreEqual(0, actualList.Count);
            Assert.AreEqual(3, requester.QueueCount);
        }

        [TestMethod]
        public async Task BackgroundUriRequester_StopAsync_Stops_Current_Active_Request()
        {
            var cancelled = false;
            Func<Uri, CancellationToken, bool> processor = (u, c) => DoProcessing(c, out cancelled);

            var requester = new BackgroundUriRequester(processor);
            requester.Add(TestHelpers.CreateRequestList(1)[0]);
            requester.Start(TimeSpan.FromMilliseconds(10));

            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);

            Assert.IsFalse(cancelled);
            await requester.StopAsync();
            Assert.IsTrue(cancelled);
        }

        private static bool DoProcessing(CancellationToken c, out bool cancelled)
        {
            using (var mre = new ManualResetEventSlim())
            {
                try
                {
                    mre.Wait(c);
                }
                catch (AggregateException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                cancelled = c.IsCancellationRequested;
                return true;
            }
        }
    }
}