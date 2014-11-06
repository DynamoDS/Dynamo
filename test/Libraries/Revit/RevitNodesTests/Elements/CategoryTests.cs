using System;

using NUnit.Framework;

using Revit.Elements;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class CategoryTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rvt")]
        public void CategoryByName_ValidArgs()
        {
            var cat = Category.ByName("OST_PointClouds");
            Assert.NotNull(cat);
            Assert.AreEqual(cat.Id, cat.InternalCategory.Id.IntegerValue);
        }

        [Test]
        [TestModel(@".\empty.rvt")]
        public void CategoryByName_BadArgs()
        {
            Assert.Throws<ArgumentException>(()=>Category.ByName("foo"));
            Assert.Throws<ArgumentNullException>(()=>Category.ByName(null));
        }
    }
}
