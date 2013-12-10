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
    public class LevelTests
    {

        [Test]
        public void ByElevationAndName_ValidArgs()
        {
            // construct the extrusion
            var elevation = 100;
            var name = "Ham";
            var level = DSLevel.ByElevationAndName(elevation, name);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);
            Assert.AreEqual(name, level.Name);
        }

        [Test]
        public void ByElevationAndName_NullArgument()
        {
            var elevation = 100;

            Assert.Throws(typeof(ArgumentNullException), () => DSLevel.ByElevationAndName(elevation, null));
        }

        [Test]
        public void ByElevation_ValidArgs()
        {
            var elevation = 100;
            var level = DSLevel.ByElevation(elevation);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);
        }

        [Test]
        public void ByLevelAndOffset_ValidArgs()
        {
            var elevation = 100;
            var offset = 100;
            var level = DSLevel.ByElevation(elevation);

            var level2 = DSLevel.ByLevelAndOffset(level, offset);
            Assert.NotNull(level2);

            Assert.AreEqual(elevation + offset, level2.Elevation);
            Assert.AreEqual(elevation + offset, level2.ProjectElevation);
        }

        [Test]
        public void ByLevelAndOffset_NullArgument()
        {
            var offset = 100;
            Assert.Throws(typeof(ArgumentNullException), () => DSLevel.ByLevelAndOffset(null, offset));
        }

        [Test]
        public void ByLevelOffsetAndName_ValidArgs()
        {
            var elevation = 100;
            var offset = 100;
            var name = "Ham";
            var level = DSLevel.ByElevation(elevation);

            var level2 = DSLevel.ByLevelOffsetAndName(level, offset, name);
            Assert.NotNull(level2);

            Assert.AreEqual(elevation + offset, level2.Elevation);
            Assert.AreEqual(elevation + offset, level2.ProjectElevation);
        }

        [Test]
        public void ByLevelOffsetAndName_NullArgument()
        {
            var elevation = 100;
            var offset = 100;
            var name = "Ham";
            var level = DSLevel.ByElevation(elevation);

            Assert.Throws(typeof(ArgumentNullException), () => DSLevel.ByLevelOffsetAndName(null, offset, name));
            Assert.Throws(typeof(ArgumentNullException), () => DSLevel.ByLevelOffsetAndName(level, offset, null));
        }
    }

}

