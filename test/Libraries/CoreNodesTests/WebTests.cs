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
        public void WebRequest_ValidArgs()
        {
            var result = Web.WebRequestByUrl("http://www.google.com");
            Assert.IsNotNullOrEmpty(result);
        }

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
    }
}
