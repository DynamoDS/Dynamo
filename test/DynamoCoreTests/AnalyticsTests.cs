using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Analytics.Core;
using Autodesk.Analytics.Events;
using DesignScript.Builtin;
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

    public class AnalyticsTestsBase : DynamoModelTestBase
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
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackEvent(Actions.New, Categories.NodeOperations, "New Node", 5), times);

            Analytics.TrackScreenView("TestScreen");
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackScreenView("TestScreen"), times);

            Analytics.TrackActivityStatus("User");
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackActivityStatus("User"), times);

            TestAnalytics.TrackException<InvalidOperationException>(false);
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackException(It.IsAny<InvalidOperationException>(), false), times);

            var time = TimeSpan.FromMinutes(3);
            var variable = "TimeVariable";
            var description = "Some description";
            Analytics.TrackTimedEvent(Categories.Stability, variable, time, description);
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackTimedEvent(Categories.Stability, variable, time, description), times);

            using (var x = Analytics.CreateTaskTimedEvent(Categories.Performance, variable, description).Result)
            {
                clientMoq.Verify(c => c.CreateTaskTimedEvent(Categories.Performance, variable, description, null), times);
            }

            var e = Analytics.TrackTaskCommandEvent("TestCommand").Result;
            clientMoq.Verify(c => c.CreateTaskCommandEvent("TestCommand", "", null, null), times);

            e = Analytics.TrackTaskCommandEvent("TestCommand", "TestCommand description", null, new Dictionary<string, object>() { } ).Result;
            clientMoq.Verify(c => c.CreateTaskCommandEvent("TestCommand", "", null, null), times);

            e = Analytics.TrackFileOperationEvent(this.TempFolder, Actions.Read, 5);
            Thread.Sleep(100);
            clientMoq.Verify(c => c.TrackFileOperationEvent(this.TempFolder, Actions.Read, 5, ""), times);
        }
    }

    public class AnalyticsTests : AnalyticsTestsBase
    {
        [Test, Order(1)]
        public void EventTrackingEnabled()
        {
            VerifyEventTracking(Times.Exactly(1));
        }

        [Test, Order(2)]
        public void EventTrackingDisabled()
        {
            Analytics.ShutDown();
            Thread.Sleep(100);

            VerifyEventTracking(Times.Never());
        }
    }

    public class DynamoAnalyticsTests : AnalyticsTestsBase
    {
        protected Mock<TrackerFactory> factoryMoq;
        protected Mock<IEventTracker> trackerMoq;

        protected const string factoryName = "DynamoAnalyticsTests";

        public DynamoAnalyticsTests()
        {
            dynamoSettings = new PreferenceSettings();
        }

        protected override Mock<IAnalyticsClient> MockClient()
        {
            var client = new Mock<DynamoAnalyticsClient>(DynamoModel.HostAnalyticsInfo) { CallBase = true };
            var session = MockAnalyticsSession();
            client.Setup(c => c.Session).Returns(session);
            return client.As<IAnalyticsClient>();
        }

        private IAnalyticsSession MockAnalyticsSession()
        {
            var session = new Mock<IAnalyticsSession>();
            session.Setup(s => s.UserId).Returns("DynamoTestUser");
            session.Setup(s => s.SessionId).Returns("UniqueSession");
            session.Setup(s => s.Start()).Callback(SetupServices);
            return session.Object;
        }

        private void SetupServices()
        {
            factoryMoq = new Mock<TrackerFactory>() { CallBase = true };
            factoryMoq.Setup(f => f.UniqueName).Returns(factoryName);

            trackerMoq = new Mock<IEventTracker>();
            factoryMoq.Object.Register<AnalyticsEvent>(trackerMoq.Object);

            Service.Instance.Register(factoryMoq.Object);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Service.Instance.Unregister(factoryName);
        }

        [Test, Order(1)]
        public void AnalyticsTrackingEnabled()
        {
            VerifyEventTracking(Times.Exactly(1));
            //1 ApplicationLifecycle Start + 3 for exception + 6 other events
            trackerMoq.Verify(t => t.Track(It.IsAny<AnalyticsEvent>(), factoryMoq.Object), Times.AtLeast(10));
        }

        [Test, Order(1)]
        public void CreateDisposableEvents()
        {
            var variable = "TimeVariable";
            var description = "Some description";

            IDisposable e = Analytics.CreateTaskTimedEvent(Categories.Performance, variable, description).Result;
            Assert.IsInstanceOf<TimedEvent>(e);
            e.Dispose();
            //1 Dispose, Timed event is not tracked for creation.
            trackerMoq.Verify(t => t.Track(e as TimedEvent, factoryMoq.Object), Times.Exactly(1));

            e = Analytics.TrackTaskCommandEvent("TestCommand").Result;
            Assert.IsInstanceOf<CommandEvent>(e);
            e.Dispose();
            //1 Create + 1 Dispose
            trackerMoq.Verify(t => t.Track(e as CommandEvent, factoryMoq.Object), Times.Exactly(2));

            e = Analytics.TrackTaskFileOperationEvent(this.TempFolder, Actions.Save, 5).Result;
            Assert.IsInstanceOf<FileOperationEvent>(e);
            e.Dispose();

            //1 Create + 1 Dispose
            trackerMoq.Verify(t => t.Track(e as FileOperationEvent, factoryMoq.Object), Times.Exactly(2));
        }
        
    }
}
