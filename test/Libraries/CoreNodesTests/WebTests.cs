using System;
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
            Assert.Throws<UriFormatException>(()=>Web.WebRequestByUrl("ThisIsNotAUrl"));
        }
    }
}
