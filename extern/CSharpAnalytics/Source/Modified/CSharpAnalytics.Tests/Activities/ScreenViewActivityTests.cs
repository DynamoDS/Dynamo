using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class ScreenViewActivityTests
    {
        [TestMethod]
        public void ScreenViewViewActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new ScreenViewActivity("screenName");

            Assert.AreEqual("screenName", activity.ScreenName);
        }
    }
}