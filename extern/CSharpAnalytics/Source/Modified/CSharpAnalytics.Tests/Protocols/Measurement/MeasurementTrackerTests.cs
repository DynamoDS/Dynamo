using System;
using System.Collections.Generic;
using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementTrackerTests
    {
        [TestMethod]
        public void MeasurementTracker_Track_Ends_Session()
        {
            var actual = new List<Uri>();
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, sessionManager, MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            tracker.Track(new ScreenViewActivity("Testing") { EndSession = true });

            Assert.AreEqual(SessionStatus.Ending, sessionManager.SessionStatus);
            StringAssert.Contains(actual.Last().OriginalString, "sc=end");
        }

        [TestMethod]
        public void MeasurementTracker_Track_Sends_Request()
        {
            var actual = new List<Uri>();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            tracker.Track(new ScreenViewActivity("Testing"));

            Assert.AreEqual(1, actual.Count);
        }

        [TestMethod]
        public void MeasurementTracker_Track_Does_Not_Send_Request_When_Opted_Out()
        {
            var actual = new List<Uri>();
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, sessionManager, MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            sessionManager.VisitorStatus = VisitorStatus.OptedOut;
            tracker.Track(new ScreenViewActivity("Testing"));

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void MeasurementTracker_Track_Does_Not_Buffer_While_Opted_Out()
        {
            var actual = new List<Uri>();
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, sessionManager, MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            sessionManager.VisitorStatus = VisitorStatus.OptedOut;
            tracker.Track(new ScreenViewActivity("OptedOut"));
            sessionManager.VisitorStatus = VisitorStatus.Active;
            tracker.Track(new ScreenViewActivity("OptedIn"));

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].OriginalString, "cd=OptedIn");
        }

        [TestMethod]
        public void MeasurementTracker_Track_Carries_Forward_Last_Transaction()
        {
            var actual = new List<Uri>();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            var transaction = new TransactionActivity { OrderId = "123", Currency = "GBP" };
            tracker.Track(transaction);

            var transactionItem = new TransactionItemActivity("ABC", "Unit Test", 1.23m, 4);
            tracker.Track(transactionItem);

            Assert.AreEqual(transaction, transactionItem.Transaction);
            StringAssert.Contains(actual.Last().OriginalString, "ti=123");
        }
    }
}