

using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    internal class SplashScreenTests
    {
        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect1()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.RequestLaunchDynamo(true);
            Assert.IsFalse(ss.CloseWasExplicit);
        }
        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect2()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            Assert.IsFalse(ss.CloseWasExplicit);
        }
        [Test]
        public async void SplashScreen_CloseExplicitPropIsCorrect3()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.CloseWindow();
            Assert.IsTrue(ss.CloseWasExplicit);
        }


    }
}
