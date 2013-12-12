using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes.Application;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class DocumentTests
    {

        [Test]
        public void Current()
        {
            var doc = DSDocument.Current;
            Assert.NotNull(doc);
            Assert.NotNull(doc.ActiveView);
            Assert.IsTrue(doc.IsFamilyDocument);
        }

    }
}
