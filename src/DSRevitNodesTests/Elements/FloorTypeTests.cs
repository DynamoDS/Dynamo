using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
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
            var floorType = DSFloorType.ByName(floorTypeName);
            Assert.NotNull(floorType);
            Assert.AreEqual(floorTypeName, floorType.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSFloorType.ByName(null));
        }

    }

}

