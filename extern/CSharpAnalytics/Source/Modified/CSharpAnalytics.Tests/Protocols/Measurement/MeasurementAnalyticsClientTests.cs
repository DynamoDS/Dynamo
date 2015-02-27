using System;
using System.Collections.Generic;
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
    public class MeasurementAnalyticsClientTests
    {
        [TestMethod]
        public void MeasurementAnalyticsClient_Replays_Tracked_Activities_After_Configured()
        {
            var actual = new List<Uri>();

            var client = new MeasurementAnalyticsClient();
            client.Track(new ScreenViewActivity("The Big Screen"));
            client.Track(new ScreenViewActivity("Silk Screen"));

            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            Assert.AreEqual(2, actual.Count);
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_Track_Ends_AutoTimedEventActivity()
        {
            var client = new MeasurementAnalyticsClient();
            var autoTimedEvent = new AutoTimedEventActivity("Category", "Variable");

            client.Track(autoTimedEvent);

            Assert.IsNotNull(autoTimedEvent.EndedAt);
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomDimension_By_Int_Is_Sent()
        {
            var actual = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            client.SetCustomDimension(5, "DimensionFive");
            client.TrackScreenView("Test View");

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].Query, "cd5=DimensionFive");
        }

        private enum CustomDimensions
        {
            Eight = 8
        };

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomDimension_By_Enum_Is_Sent()
        {
            var actual = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            client.SetCustomDimension(CustomDimensions.Eight, "DimensionEight");
            client.TrackScreenView("Test View");

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].Query, "cd8=DimensionEight");
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomMetric_Int_Is_Sent()
        {
            var actual = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            client.SetCustomMetric(6, 6060);
            client.TrackScreenView("Test View");

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].Query, "cm6=6060");
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomMetric_Timespan_Is_Sent()
        {
            var actual = new List<Uri>();
            var actualTimespan = new TimeSpan(4, 1, 2, 3);
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            client.SetCustomMetric(7, actualTimespan);
            client.TrackScreenView("Test View");

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].Query, "cm7=" + (int)actualTimespan.TotalSeconds);
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomMetric_Decimal_Is_Sent()
        {
            var actual = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, actual.Add);

            client.SetCustomMetric(8, 123456.78m);
            client.TrackScreenView("Test View");

            Assert.AreEqual(1, actual.Count);
            StringAssert.Contains(actual[0].Query, "cm8=123456.78");
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_AdjustUriBeforeRequest_Adds_Qt_Parameter()
        {
            var originalUri = new Uri("http://anything.really.com/something#" + DateTime.UtcNow.ToString("o"));

            var actual = new MeasurementAnalyticsClient().AdjustUriBeforeRequest(originalUri);

            StringAssert.Contains(actual.Query, "qt=");
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_AdjustUriBeforeRequest_Does_Not_Add_Qt_Parameter_If_Sc_Parameter_Present()
        {
            var originalUri = new Uri("http://anything.really.com/something?sc=start#" + DateTime.UtcNow.ToString("o"));

            var actual = new MeasurementAnalyticsClient().AdjustUriBeforeRequest(originalUri);

            Assert.IsFalse(actual.Query.Contains("qt="));
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_AdjustUriBeforeRequest_Clears_Fragment()
        {
            var originalUri = new Uri("http://anything.really.com/something#" + DateTime.UtcNow.ToString("o"));

            var actual = new MeasurementAnalyticsClient().AdjustUriBeforeRequest(originalUri);

            Assert.AreEqual(actual.Fragment, "");
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_OnTrack_Fires_When_Tracked()
        {
            var fired = false;
            var analyticsClient = new MeasurementAnalyticsClient();
            
            analyticsClient.OnTrack += (s, e) => fired = true;
            analyticsClient.Track(new ScreenViewActivity("Testing"));

            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_OnTrack_Fires_After_AutoTimedEvent_Ended()
        {
            DateTimeOffset? endedAt = null;
            var analyticsClient = new MeasurementAnalyticsClient();
            var autoTimedEvent = new AutoTimedEventActivity("Category", "Variable");

            analyticsClient.OnTrack += (s, e) => { endedAt = ((AutoTimedEventActivity) e).EndedAt; };
            analyticsClient.Track(autoTimedEvent);

            Assert.IsNotNull(endedAt);
        }

        [TestMethod]
        public void TrackScreenView_Tracks_ScreenView()
        {
            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackScreenView("SomeScreenName");

            Assert.AreEqual(1, list.Count);
            StringAssert.Contains(list[0].OriginalString, "t=screenview");
        }

        [TestMethod]
        public void TrackContentView_Tracks_ContentView()
        {
            var url = new Uri("http://csharpanalytics.com/doc");
            const string title = "CSharpAnalytics docs";
            const string description = "Documentation for CSharpAnalaytics";
            const string path = "/docs";
            const string hostName = "docs.csharpanalytics.com";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackContentView(url, title, description, path, hostName);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=pageview");
            CollectionAssert.Contains(parameters, "dl=" + url);
            CollectionAssert.Contains(parameters, "dt=" + title);
            CollectionAssert.Contains(parameters, "cd=" + description);
            CollectionAssert.Contains(parameters, "dp=" + path);
            CollectionAssert.Contains(parameters, "dh=" + hostName);
        }

        [TestMethod]
        public void TrackEvent_Tracks_Event()
        {
            const string action = "Some action";
            const string category = "Category Z";
            const string label = "I am a label";
            const int value = 55;

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackEvent(action, category, label, value, true);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=event");
            CollectionAssert.Contains(parameters, "ea=" + action);
            CollectionAssert.Contains(parameters, "ec=" + category); 
            CollectionAssert.Contains(parameters, "el=" + label);
            CollectionAssert.Contains(parameters, "ev=" + value);
            CollectionAssert.Contains(parameters, "ni=1");
        }

        [TestMethod]
        public void TrackException_Tracks_Exception()
        {
            const string description = "Some action";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackException(description);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=exception");
            CollectionAssert.Contains(parameters, "exd=" + description);
            CollectionAssert.Contains(parameters, "exf=0");
        }

        [TestMethod]
        public void TrackSocial_Tracks_Social()
        {
            const string action = "Poke";
            const string network = "FriendFace";
            const string target = "Clown";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackSocial(action, network, target);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=social");
            CollectionAssert.Contains(parameters, "sa=" + action);
            CollectionAssert.Contains(parameters, "sn=" + network);
            CollectionAssert.Contains(parameters, "st=" + target);
        }

        [TestMethod]
        public void TrackTimedEvent_Tracks_TimedEvent()
        {
            const string category = "A category";
            const string variable = "Some variable";
            var time = TimeSpan.FromMilliseconds(12345);
            const string label = "Blue";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackTimedEvent(category, variable, time, label);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=timing");
            CollectionAssert.Contains(parameters, "utc=" + category);
            CollectionAssert.Contains(parameters, "utv=" + variable);
            CollectionAssert.Contains(parameters, "utt=" + time.TotalMilliseconds);
            CollectionAssert.Contains(parameters, "utl=" + label);
        }

        private enum NotIntBacked : long
        {
            SomeValue
        }

#if WINDOWS_STORE || WINDOWS_PHONE
        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomDimension_Throws_ArgumentException_If_Enum_Not_Underlying_Int_Type()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementAnalyticsClient().SetCustomDimension(NotIntBacked.SomeValue, "OneTwoThree"));
        }

        [TestMethod]
        public void MeasurementAnalyticsClient_SetCustomDimension_Throws_ArgumentOutOfRangeException_If_Enum_Not_Defined()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MeasurementAnalyticsClient().SetCustomDimension((CustomDimensions) 99, "Ninety-Nine"));
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MeasurementAnalyticsClient_SetCustomDimension_Throws_ArgumentException_If_Enum_Not_Underlying_Int_Type()
        {
            new MeasurementAnalyticsClient().SetCustomDimension(NotIntBacked.SomeValue, "OneTwoThree");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MeasurementAnalyticsClient_SetCustomDimension_Throws_ArgumentOutOfRangeException_If_Enum_Not_Defined()
        {
            new MeasurementAnalyticsClient().SetCustomDimension((CustomDimensions)99, "Ninety-Nine");
        }
#endif
    }
}