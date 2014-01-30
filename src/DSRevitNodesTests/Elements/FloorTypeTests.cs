using System;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class FloorTypeTests
    {

        [Test]
        public void ByName_ValidArgs()
        {
            var floorTypeName = "Generic - 12\"";
            var floorType = FloorType.ByName(floorTypeName);
            Assert.NotNull(floorType);
            Assert.AreEqual(floorTypeName, floorType.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => FloorType.ByName(null));
        }

    }

}

