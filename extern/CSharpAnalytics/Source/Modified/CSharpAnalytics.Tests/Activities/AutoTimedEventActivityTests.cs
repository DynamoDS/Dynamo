using System;
using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class AutoTimedEventActivityTests
    {
        [TestMethod]
        public void AutoTimedEventActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new AutoTimedEventActivity("category", "variable");

            Assert.AreEqual("category", activity.Category);
            Assert.AreEqual("variable", activity.Variable);
            Assert.IsTrue(activity.Time >= TimeSpan.Zero);

            Assert.IsNull(activity.Label);
        }

        [TestMethod]
        public void AutoTimedEventActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new AutoTimedEventActivity("category", "variable", "label");

            Assert.AreEqual("category", activity.Category);
            Assert.AreEqual("variable", activity.Variable);
            Assert.AreEqual("label", activity.Label);
        }

        [TestMethod]
        public void AutoTimedEventActivity_Starts_At_Correct_Time()
        {
            var earliest = DateTimeOffset.Now;
            var activity = new AutoTimedEventActivity("category", "variable");
            var latest = DateTimeOffset.Now;

            Assert.IsTrue(activity.StartedAt >= earliest, "StartedAt too early expected after {0} found {1}", earliest, activity.StartedAt);
            Assert.IsTrue(activity.StartedAt <= latest, "StartedAt too late expected before {0} found {1}", latest, activity.StartedAt);
        }

        [TestMethod]
        public void AutoTimedEventActivity_Ends_At_Correct_Time()
        {
            var earliest = DateTimeOffset.Now;
            var activity = new AutoTimedEventActivity("category", "variable");

            activity.End();
            var latest = DateTimeOffset.Now;

            Assert.IsNotNull(activity.EndedAt);
            Assert.IsTrue(activity.EndedAt >= earliest, "EndedAt too early expected after {0} found {1}", earliest, activity.EndedAt);
            Assert.IsTrue(activity.EndedAt <= latest, "EndedAt too late expected before {0} found {1}", latest, activity.EndedAt);
        }

        [TestMethod]
        public void AutoTimedEventActivity_EndedAt_Property_Can_Be_Set()
        {
            var activity = new AutoTimedEventActivity("category", "variable");

            var expectedEndedAt = new DateTimeOffset(2001, 11, 5, 1, 2, 3, 4, TimeSpan.Zero);
            activity.EndedAt = expectedEndedAt;

            Assert.AreEqual(expectedEndedAt, activity.EndedAt);
        }

        [TestMethod]
        public void AutoTimedEventActivity_EndedAt_Property_Can_Be_Nulled()
        {
            var activity = new AutoTimedEventActivity("category", "variable");

            activity.End();
            activity.EndedAt = null;

            Assert.IsNull(activity.EndedAt);
        }
    }
}