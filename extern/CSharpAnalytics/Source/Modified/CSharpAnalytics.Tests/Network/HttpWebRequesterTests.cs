using System;
using CSharpAnalytics.Network;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class HttpWebRequesterTests
    {
        private const string BaseUriString = "http://localhost/csharpanalytics";

        [TestMethod]
        public void HttpWebRequester_CreateRequest_Uses_Get_Under_2000_Bytes()
        {
            var shortUri = new Uri(BaseUriString);
            var request = HttpWebRequester.CreateRequest(shortUri);

            Assert.AreEqual("GET", request.Method);
            Assert.AreEqual(shortUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(-1, request.ContentLength);
        }

        [TestMethod]
        public void HttpWebRequester_CreateRequest_Uses_Get_When_2000_Bytes()
        {
            var longUri = new Uri(BaseUriString + "?" + TestHelpers.RandomChars(2000));
            var borderlineUri = new Uri(longUri.AbsoluteUri.Substring(0, 2000));

            var request = HttpWebRequester.CreateRequest(borderlineUri);

            Assert.AreEqual("GET", request.Method);
            Assert.AreEqual(borderlineUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(-1, request.ContentLength);
        }

        [TestMethod]
        public void HttpWebRequester_CreateMessage_Uses_Post_Over_2000_Bytes()
        {
            var baseUri = new Uri(BaseUriString);
            var longUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(2000));

            var request = HttpWebRequester.CreateRequest(longUri, false);

            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.IsTrue(request.ContentLength > 100);
        }
    }
}