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
    public class TimedEventActivityTests
    {
        [TestMethod]
        public void TimedEventActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new TimedEventActivity("category", "variable", TimeSpan.FromSeconds(1));

            Assert.AreEqual("category", activity.Category);
            Assert.AreEqual("variable", activity.Variable);
            Assert.AreEqual(TimeSpan.FromSeconds(1), activity.Time);

            Assert.IsNull(activity.Label);
        }

        [TestMethod]
        public void TimedEventActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new TimedEventActivity("category", "variable", TimeSpan.FromSeconds(1.5), "label");

            Assert.AreEqual("category", activity.Category);
            Assert.AreEqual("variable", activity.Variable);
            Assert.AreEqual(TimeSpan.FromSeconds(1.5), activity.Time);
            Assert.AreEqual("label", activity.Label);
        }
    }
}