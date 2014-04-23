using System.Net;
using DSCore;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    class WebTests
    {
        [Test]
        public void WebRequest_ValidArgs()
        {
            var result = Web.WebRequestByUrl("http://www.google.com");
            Assert.IsNotNullOrEmpty(result);
        }

        [Test]
        public void WebRequest_BadArgs()
        {
            Assert.Throws<WebException>(()=>Web.WebRequestByUrl("http://www.somesitethatdoesnotexist.com"));
        }
    }
}
