#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Environment
{
    [TestClass]
    public class EnvironmentTests
    {
        [TestMethod]
        public void TestableEnvironment_Constructor_Sets_Properties_Correctly()
        {
            var environment = new TestableEnvironment("en-gb")
            {
                CharacterSet = "iso-8550-1",
                FlashVersion = "11.0.1b",
                ScreenColorDepth = 32,
                JavaEnabled = true,
                ScreenHeight = 1050,
                ScreenWidth = 1920,
                ViewportHeight = 768,
                ViewportWidth = 1024
            };

            Assert.AreEqual("iso-8550-1", environment.CharacterSet);
            Assert.AreEqual("en-gb", environment.LanguageCode);
            Assert.AreEqual("11.0.1b", environment.FlashVersion);
            Assert.AreEqual(32u, environment.ScreenColorDepth);
            Assert.AreEqual(true, environment.JavaEnabled);
            Assert.AreEqual(1050u, environment.ScreenHeight);
            Assert.AreEqual(1920u, environment.ScreenWidth);
            Assert.AreEqual(768u, environment.ViewportHeight);
            Assert.AreEqual(1024u, environment.ViewportWidth);
        }
    }
}
