using CSharpAnalytics;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.AutoMeasurement
{
    [TestClass]
    public class AnalyticsScreenNameAttributeTests
    {
        [TestMethod]
        public void AnalyticsScreenNameAttribute_Constructor_Sets_Correct_Properties()
        {
            const string expected = "Some screen name";

            var attribute = new AnalyticsScreenNameAttribute(expected);

            Assert.AreEqual(expected, attribute.ScreenName);
        }
    }
}