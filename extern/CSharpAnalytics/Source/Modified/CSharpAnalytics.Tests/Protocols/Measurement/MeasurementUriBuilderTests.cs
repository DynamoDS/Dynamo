using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Test.Environment;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementUriBuilderTests
    {
        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Configuration_Returns_Correct_Keys()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "AppName", "1.2.3.4");

            var keys = MeasurementUriBuilder.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.AreEquivalent(new[] { "tid", "an", "av", "aip" }, keys);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Configuration_Returns_No_Aip_Value_When_False()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "AppName", "1.2.3.4") { AnonymizeIp = false };

            var keys = MeasurementUriBuilder.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.DoesNotContain(keys, "aip");
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Environment_Returns_Correct_Values()
        {
            var environment = new TestableEnvironment("en-gb")
                {
                    CharacterSet = "ISO-8550-1",
                    FlashVersion = "11.0.1b",
                    ScreenColorDepth = 32,
                    JavaEnabled = true,
                    ScreenHeight = 1050,
                    ScreenWidth = 1920,
                    ViewportHeight = 768,
                    ViewportWidth = 1024
                };

            var parameters = MeasurementUriBuilder.GetParameters(environment).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("ISO-8550-1", parameters["de"]);
            Assert.AreEqual("en-gb", parameters["ul"]);
            Assert.AreEqual("11.0.1b", parameters["fl"]);
            Assert.AreEqual("32-bit", parameters["sd"]);
            Assert.AreEqual("1", parameters["je"]);
            Assert.AreEqual("1024x768", parameters["vp"]);
            Assert.AreEqual("1920x1050", parameters["sr"]);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Environment_Returns_Correct_Je_Value()
        {
            var environment = new TestableEnvironment("en-gb") { JavaEnabled = false };

            var jeValue = MeasurementUriBuilder.GetParameters(environment).First(f => f.Key == "je").Value;

            Assert.AreEqual("0", jeValue);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Is_http_And_www_When_Not_Using_SSL()
        {
            var config = MeasurementTestHelpers.Configuration;
            config.UseSsl = false;
            var builder = new MeasurementUriBuilder(config, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment());

            var actual = builder.BuildUri(new ScreenViewActivity("Home"));

            Assert.AreEqual("http", actual.Scheme);
            Assert.AreEqual("www.google-analytics.com", actual.Host);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Is_https_And_ssl_When_Using_SSL()
        {
            var config = MeasurementTestHelpers.Configuration;
            config.UseSsl = true;
            var builder = new MeasurementUriBuilder(config, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment());

            var actual = builder.BuildUri(new ScreenViewActivity("Home"));

            Assert.AreEqual("https", actual.Scheme);
            Assert.AreEqual("ssl.google-analytics.com", actual.Host);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Carries_Forward_Cd_Parameter()
        {
            var config = MeasurementTestHelpers.Configuration;
            var builder = new MeasurementUriBuilder(config, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment());

            builder.BuildUri(new ScreenViewActivity("CarriedForward"));
            var actual = builder.BuildUri(new EventActivity("Action", "Category"));

            StringAssert.Contains(actual.Query, "cd=CarriedForward");
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Emits_SessionControl_start_At_Start()
        {
            var config = MeasurementTestHelpers.Configuration;
            var builder = new MeasurementUriBuilder(config, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment());

            var actual = builder.BuildUri(new ScreenViewActivity("Home"));

            StringAssert.Contains(actual.Query, "sc=start");
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Emits_No_SessionControl_After_Start()
        {
            var config = MeasurementTestHelpers.Configuration;
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var builder = new MeasurementUriBuilder(config, sessionManager, MeasurementTestHelpers.CreateEnvironment());

            sessionManager.Hit();
            var actual = builder.BuildUri(new ScreenViewActivity("Page2"));

            var parameters = actual.Query.Split('&').Select(p => p.Split('=')).ToDictionary(k => k[0], v => v.Length == 0 ? null : v[1]);
            CollectionAssert.DoesNotContain(parameters.Keys, "sc");
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_BuildUri_Emits_SessionControl_end_At_End()
        {
            var config = MeasurementTestHelpers.Configuration;
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var builder = new MeasurementUriBuilder(config, sessionManager, MeasurementTestHelpers.CreateEnvironment());

            sessionManager.End();
            var actual = builder.BuildUri(new EventActivity("Action", "Category"));

            StringAssert.Contains(actual.Query, "sc=end");
        }
    }
}