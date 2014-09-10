using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class ExceptionActivityTests
    {
        [TestMethod]
        public void ExceptionActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new ExceptionActivity("description", true);

            Assert.AreEqual("description", activity.Description);
            Assert.AreEqual(true, activity.IsFatal);
        }
    }
}