using DynamoTests.Utils;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Threading;

namespace DynamoUITests
{
    public class WebView2Tests : DynamoSession
    {

        //[OneTimeSetUp]
        //public static void ClassInitialize()
        //{
        //    Setup(false);
        //    /*

        //    testConfiguration = ConfigurationHelper
        //        .GetTestConfiguration<SmokeTestMenusConfiguration>(
        //            testFile: "smokeTest02", 
        //            section: "SmokeTest02", 
        //            path: "Smoke/");
        //    */
        //}

        [SetUp]
        public void Setup()
        {
            Setup(false);
        }

        [Test]
        public void TestSplashScreen()
        {            
            session.FindElementByName("Dynamo SplashScreen");

            // Appium structure
            var firstPane = session.FindElementByClassName("HwndHost");
            var textElement = firstPane.FindElementByClassName("Static");
            var WV2Container = textElement.FindElementByClassName("Chrome_WidgetWin_0");
            var WV2Control = WV2Container.FindElementByClassName("Chrome_WidgetWin_1");

            bool WV2IsCreated = WV2Container != null && WV2Control != null;
            Assert.IsTrue(WV2IsCreated);

            string htmlContent = WV2Control.GetAttribute("Name");
            string emptyHtml = "about:blank";

            session.CloseApp();

            Assert.AreNotEqual(htmlContent, emptyHtml, $"The Html content is : {htmlContent}");
        }
    }
}
