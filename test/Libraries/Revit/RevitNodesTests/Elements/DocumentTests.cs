using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revit.Application;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class DocumentTests
    {

        [Test]
        public void Current()
        {
            var doc = Document.Current;
            Assert.NotNull(doc);
            Assert.NotNull(doc.ActiveView);
            Assert.IsTrue(doc.IsFamilyDocument);
        }

    }
}
