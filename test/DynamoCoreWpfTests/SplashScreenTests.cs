

using DynamoUtilities;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    internal class SplashScreenTests
    {
        [SetUp]
        public void SetUp()
        {
            TestUtilities.WebView2Tag = TestContext.CurrentContext.Test.Name;
        }

        [TearDown]
        public void CleanUp()
        {
            TestUtilities.WebView2Tag = string.Empty;
        }

        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect1()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.RequestLaunchDynamo(true);
            Assert.IsFalse(ss.CloseWasExplicit);

            ss.CloseWindow();
        }

        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect2()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            Assert.IsFalse(ss.CloseWasExplicit);

            ss.CloseWindow();
        }

        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect3()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.CloseWindow();
            Assert.IsTrue(ss.CloseWasExplicit);
        }
    }
}
