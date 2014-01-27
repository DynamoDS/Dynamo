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
    public class WallTypeTests
    {

        [Test]
        public void ByName_ValidArgs()
        {
            var wallTypeName = "Curtain Wall 1";
            var wallType = DSWallType.ByName(wallTypeName);
            Assert.NotNull(wallType);
            Assert.AreEqual(wallTypeName, wallType.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSElementType.ByName(null));
        }

    }

}

