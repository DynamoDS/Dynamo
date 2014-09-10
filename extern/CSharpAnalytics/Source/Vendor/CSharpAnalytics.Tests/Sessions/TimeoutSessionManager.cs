using System.Globalization;
using CSharpAnalytics.Sessions;
using System;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class TimeoutSessionManagerTests
    {
        [TestMethod]
        public void TimeoutSessionManager_Can_Be_Created_From_State()
        {
            var timeout = TimeSpan.FromMinutes(5);
            var state = CreateSampleState();

            var sessionManager = new TimeoutSessionManager(state, timeout);

            Assert.AreEqual(timeout, sessionManager.Timeout);
            Assert.AreEqual(state.VisitorId, sessionManager.Visitor.ClientId);
        }

        [TestMethod]
        public void TimeoutSessionManager_Created_From_Null_State_Is_Fresh()
        {
            var timeout = TimeSpan.FromHours(1.25);

            var sessionManager = new TimeoutSessionManager(null, timeout);

            Assert.AreEqual(timeout, sessionManager.Timeout);
            Assert.IsNull(sessionManager.Referrer);
        }

        [TestMethod]
        public void TimeoutSessionManager_Created_From_State_Provides_Same_State()
        {
            var expected = CreateSampleState();

            var sessionManager = new TimeoutSessionManager(expected, TimeSpan.FromDays(1));
            var actual = sessionManager.GetState();

            Assert.AreEqual(expected.VisitorId, actual.VisitorId);
        }

        private static readonly Random random = new Random();

        private static SessionState CreateSampleState()
        {
            return new SessionState
            {
                VisitorId = Guid.NewGuid(),
                LastActivityAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                Referrer = new Uri("http://damieng.com/" + random.Next().ToString(CultureInfo.InvariantCulture))
            };
        }
    }
}