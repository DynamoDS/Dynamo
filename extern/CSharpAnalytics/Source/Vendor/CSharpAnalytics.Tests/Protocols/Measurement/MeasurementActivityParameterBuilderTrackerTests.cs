using System;
using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementActivityParameterBuilderTrackerTests
    {
        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_ScreenViewActivity_Returns_Correct_Values()
        {
            var activity = new ScreenViewActivity("page");

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("screenview", parameters["t"]);
            Assert.AreEqual("page", parameters["cd"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_ContentViewActivity_Returns_Correct_Values()
        {
            var location = new Uri("http://unittest.csharpanalytics.com/some/path");
            var activity = new ContentViewActivity(location, "Document Title", "A content description.", "/document/path", "hostname.csharpanalytics.com");

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(6, parameters.Keys.Count);
            Assert.AreEqual("pageview", parameters["t"]);
            Assert.AreEqual(location.AbsoluteUri, parameters["dl"]);
            Assert.AreEqual("Document Title", parameters["dt"]);
            Assert.AreEqual("A content description.", parameters["cd"]);
            Assert.AreEqual("/document/path", parameters["dp"]);
            Assert.AreEqual("hostname.csharpanalytics.com", parameters["dh"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_EventActivity_Returns_Correct_Values()
        {
            var activity = new EventActivity("action", "category", "label", 123, true);

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(6, parameters.Keys.Count);
            Assert.AreEqual("event", parameters["t"]);
            Assert.AreEqual("action", parameters["ea"]);
            Assert.AreEqual("category", parameters["ec"]);
            Assert.AreEqual("label", parameters["el"]);
            Assert.AreEqual("123", parameters["ev"]);
            Assert.AreEqual("1", parameters["ni"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_ExceptionActivity_Returns_Correct_Values()
        {
            var activity = new ExceptionActivity("Something wonderful has happened.", false);

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(3, parameters.Keys.Count);
            Assert.AreEqual("Something wonderful has happened.", parameters["exd"]);
            Assert.AreEqual("0", parameters["exf"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_SocialActivity_Returns_Correct_Values()
        {
            var activity = new SocialActivity("action", "network", target: "target");

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(4, parameters.Keys.Count);
            Assert.AreEqual("social", parameters["t"]);
            Assert.AreEqual("network", parameters["sn"]);
            Assert.AreEqual("action", parameters["sa"]);
            Assert.AreEqual("target", parameters["st"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_TimedEventActivity_Returns_Correct_Values()
        {
            var activity = new TimedEventActivity("cateogry", "variable", TimeSpan.FromMilliseconds(12345), "label");

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(5, parameters.Keys.Count);
            Assert.AreEqual("timing", parameters["t"]);
            Assert.AreEqual("variable", parameters["utv"]);
            Assert.AreEqual("12345", parameters["utt"]);
            Assert.AreEqual("label", parameters["utl"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_TransactionActivity_Returns_Correct_Values()
        {
            var activity = new TransactionActivity
            {
                OrderId = "12345",
                Currency = "USD",
                OrderTotal = 109.76m,
                ShippingCost = 11.27m,
                StoreName = "My Store",
                TaxCost = 8.18m
            };

            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(7, parameters.Keys.Count);
            Assert.AreEqual("transaction", parameters["t"]);
            Assert.AreEqual("12345", parameters["ti"]);
            Assert.AreEqual("USD", parameters["cu"]);
            Assert.AreEqual("109.76", parameters["tr"]);
            Assert.AreEqual("11.27", parameters["ts"]);
            Assert.AreEqual("My Store", parameters["ta"]);
            Assert.AreEqual("8.18", parameters["tt"]);
        }

        [TestMethod]
        public void MeasurementActivityParameterBuilder_GetParameter_For_TransactionItemActivity_Returns_Correct_Values()
        {
            var transaction = new TransactionActivity { OrderId = "567", Currency = "GBP" };
            var activity = new TransactionItemActivity("code", "name", 1.23m, 4096, "variation") { Transaction = transaction };
            var parameters = MeasurementActivityParameterBuilder.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual(8, parameters.Keys.Count);
            Assert.AreEqual("item", parameters["t"]);

            Assert.AreEqual("567", parameters["ti"]);
            Assert.AreEqual("GBP", parameters["cu"]);

            Assert.AreEqual("code", parameters["ic"]);
            Assert.AreEqual("name", parameters["in"]);
            Assert.AreEqual("1.23", parameters["ip"]);
            Assert.AreEqual("4096", parameters["iq"]);
            Assert.AreEqual("variation", parameters["iv"]);
        }
    }
}