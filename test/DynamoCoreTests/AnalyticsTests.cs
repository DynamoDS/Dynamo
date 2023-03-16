using System;
using Autodesk.Analytics.Core;
using Autodesk.Analytics.Events;
using Dynamo.Configuration;
using Dynamo.Logging;
using Dynamo.Models;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class TestAnalytics
    {
        public static void Init(IAnalyticsClient client, DynamoModel model)
        {
            Analytics.Start(client);
        }

        public static bool IsEnabled 
        { 
            get { return Analytics.IsEnabled; }
        }

        public static void Disable()
        {
            Analytics.ShutDown();
        }

        public static void Throw<T>() where T : Exception, new()
        {
            throw new T();
        }

        public static void TrackException<T>(bool isFatal) where T : Exception, new()
        {
            try
            {
                Throw<T>();
            }
            catch(Exception ex)
            {
                Analytics.TrackException(ex, isFatal);
            }
        }
    }

    public class AnalyticsTests : DynamoModelTestBase
    {
        protected Mock<IAnalyticsClient> clientMoq;

        public override void Setup()
        {
            base.Setup();

            clientMoq = MockClient();
            //Setup mock client and start analytics tracking.
            TestAnalytics.Init(clientMoq.Object, CurrentDynamoModel);
        }

        protected virtual Mock<IAnalyticsClient> MockClient()
        {
            return new Mock<IAnalyticsClient>();
        }

        public override void Cleanup()
        {
            clientMoq.Verify(c => c.Start(), Times.Exactly(1));
            base.Cleanup();

            if (TestAnalytics.IsEnabled)
            {
                clientMoq.Verify(c => c.ShutDown());
            }
        }

        protected virtual void VerifyEventTracking(Times times)
        {
            Analytics.TrackEvent(Actions.New, Categories.NodeOperations, "New Node", 5);
            clientMoq.Verify(c => c.TrackEvent(Actions.New, Categories.NodeOperations, "New Node", 5), times);

            Analytics.TrackScreenView("TestScreen");
            clientMoq.Verify(c => c.TrackScreenView("TestScreen"), times);

            TestAnalytics.TrackException<InvalidOperationException>(false);
            clientMoq.Verify(c => c.TrackException(It.IsAny<InvalidOperationException>(), false), times);

            var time = TimeSpan.FromMinutes(3);
            var variable = "TimeVariable";
            var description = "Some description";
            Analytics.TrackTimedEvent(Categories.Stability, variable, time, description);
            clientMoq.Verify(c => c.TrackTimedEvent(Categories.Stability, variable, time, description), times);

            using (var x = Analytics.CreateTimedEvent(Categories.Performance, variable, description))
            {
                clientMoq.Verify(c => c.CreateTimedEvent(Categories.Performance, variable, description, null), times);
            }

            var e = Analytics.TrackCommandEvent("TestCommand");
            clientMoq.Verify(c => c.CreateCommandEvent("TestCommand", "", null), times);

            e = Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Read, 5);
            clientMoq.Verify(c => c.TrackFileOperationEvent(this.TempFolder, Actions.Read, 5, ""), times);

            Analytics.LogPiiInfo("tag", "data");
            clientMoq.Verify(c => c.LogPiiInfo("tag", "data"), times);
        }

        [Test]
        public void EventTrackingEnabled()
        {
            VerifyEventTracking(Times.Exactly(1));
        }

        [Test]
        public void EventTrackingDisabled()
        {
            TestAnalytics.Disable(); //Disable analytics tracking.
            VerifyEventTracking(Times.Never());
        }
    }

    public class DynamoAnalyticsTests : AnalyticsTests
    {
        protected Mock<TrackerFactory> factoryMoq;
        protected Mock<IEventTracker> trackerMoq;

        protected const string factoryName = "DynamoAnalyticsTests";

        public DynamoAnalyticsTests()
        {
            dynamoSettings = new PreferenceSettings() { IsAnalyticsReportingApproved = true, IsUsageReportingApproved = true };
        }

        protected override Mock<IAnalyticsClient> MockClient()
        {
            var client = new Mock<DynamoAnalyticsClient>(CurrentDynamoModel) { CallBase = true };
            var session = MockAnalyticsSession();
            client.Setup(c => c.Session).Returns(session);
            return client.As<IAnalyticsClient>();
        }

        private IAnalyticsSession MockAnalyticsSession()
        {
            var session = new Mock<IAnalyticsSession>();
            session.Setup(s => s.UserId).Returns("DynamoTestUser");
            session.Setup(s => s.SessionId).Returns("UniqueSession");
            session.Setup(s => s.Start(It.IsAny<DynamoModel>())).Callback(SetupServices);
            return session.Object;
        }

        private void SetupServices()
        {
            factoryMoq = new Mock<TrackerFactory>() { CallBase = true };
            factoryMoq.Setup(f => f.UniqueName).Returns(factoryName);

            trackerMoq = new Mock<IEventTracker>();
            factoryMoq.Object.Register<AnalyticsEvent>(trackerMoq.Object);

            Service.Instance.Register(factoryMoq.Object);

            Service.Instance.AddTrackerFactoryFilter(factoryName, () => {
                return CurrentDynamoModel.PreferenceSettings.IsAnalyticsReportingApproved;
                }
            );
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Service.Instance.Unregister(factoryName);
        }

        [Test]
        public void AnalyticsTrackingEnabled()
        {
            VerifyEventTracking(Times.Exactly(1));
            //1 ApplicationLifecycle Start + 3 for exception + 6 other events
            trackerMoq.Verify(t => t.Track(It.IsAny<AnalyticsEvent>(), factoryMoq.Object), Times.AtLeast(10));
        }

        [Test]
        public void AnalyticsTrackingDisabled()
        {
            //Modify preferences
            dynamoSettings.IsAnalyticsReportingApproved = false;
            dynamoSettings.IsUsageReportingApproved = false;

            //Trigger events and tracks
            VerifyEventTracking(Times.Exactly(1));
            
            //Reset preferences
            dynamoSettings.IsAnalyticsReportingApproved = true;
            dynamoSettings.IsUsageReportingApproved = true;

            //1 startup + 1 analytics optin status event (google analytics)
            trackerMoq.Verify(t => t.Track(It.IsAny<AnalyticsEvent>(), factoryMoq.Object), Times.Exactly(2));
        }

        [Test]
        public void CreateDisposableEvents()
        {
            var variable = "TimeVariable";
            var description = "Some description";
            
            var e = Analytics.CreateTimedEvent(Categories.Performance, variable, description);
            Assert.IsInstanceOf<TimedEvent>(e);
            e.Dispose();
            //1 Dispose, Timed event is not tracked for creation.
            trackerMoq.Verify(t => t.Track(e as TimedEvent, factoryMoq.Object), Times.Exactly(1));

            e = Analytics.TrackCommandEvent("TestCommand");
            Assert.IsInstanceOf<CommandEvent>(e);
            e.Dispose();
            //1 Create + 1 Dispose
            trackerMoq.Verify(t => t.Track(e as CommandEvent, factoryMoq.Object), Times.Exactly(2));

            e = Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Save, 5);
            Assert.IsInstanceOf<FileOperationEvent>(e);
            e.Dispose();

            //1 Create + 1 Dispose
            trackerMoq.Verify(t => t.Track(e as FileOperationEvent, factoryMoq.Object), Times.Exactly(2));
        }

        [Test]
        public void DummyDisposableEvents()
        {
            //Modify preferences
            dynamoSettings.IsAnalyticsReportingApproved = false;
            var variable = "TimeVariable";
            var description = "Some description";
            
            Analytics.DisableAnalytics = true;
            var e = Analytics.CreateTimedEvent(Categories.Performance, variable, description);
            Assert.IsNotInstanceOf<TimedEvent>(e);
            e.Dispose();

            Analytics.DisableAnalytics = false;

            //1 ApplicationLifecycle Start
            trackerMoq.Verify(t => t.Track(It.IsAny<TimedEvent>(), factoryMoq.Object), Times.Exactly(1));

            e = Analytics.TrackCommandEvent("TestCommand");
            // CommandEvent will be created unless DisableAnalytics is on (because Dynamo does not know the ADP opted-in status)
            Assert.IsInstanceOf<CommandEvent>(e);
            e.Dispose();
            
            trackerMoq.Verify(t => t.Track(It.IsAny<CommandEvent>(), factoryMoq.Object), Times.Never());

            e = Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Save, 5);
            // CommandEvent will be created unless DisableAnalytics is on (because Dynamo does not know the ADP opted-in status)
            Assert.IsInstanceOf<FileOperationEvent>(e);
            e.Dispose();

            dynamoSettings.IsAnalyticsReportingApproved = false;

            trackerMoq.Verify(t => t.Track(It.IsAny<FileOperationEvent>(), factoryMoq.Object), Times.Never());

            //Reset preferences
            dynamoSettings.IsAnalyticsReportingApproved = true;
        }
    }
}
