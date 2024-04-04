using System;
using DSCore;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    class WebTests
    {
        [Test]
        [Category("UnitTests")]
        public void WebRequest_BadArgs()
        {
            Assert.Throws<UriFormatException>(()=>Web.WebRequestByUrl("ThisIsNotAUrl"));
        }

        [Test]
        [Category("UnitTests")]
        public void WebRequest_EmptyUrl()
        {
            Assert.Throws<ArgumentException>(() => Web.WebRequestByUrl(""));
        }

        [Test]
        [Category("UnitTests")]
        public void WebRequest_GitHubAPI()
        {
            string url = "https://api.github.com/rate_limit";
            string result = Web.WebRequestByUrl(url);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }
    }
}
