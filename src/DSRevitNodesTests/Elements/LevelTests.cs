using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
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
            var level = Level.ByElevationAndName(elevation, name);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);
            Assert.AreEqual(name, level.Name);
        }

        [Test]
        public void ByElevationAndName_NullArgument()
        {
            var elevation = 100;

            Assert.Throws(typeof(ArgumentNullException), () => Level.ByElevationAndName(elevation, null));
        }

        [Test]
        public void ByElevation_ValidArgs()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);
        }

        [Test]
        public void ByLevelAndOffset_ValidArgs()
        {
            var elevation = 100;
            var offset = 100;
            var level = Level.ByElevation(elevation);

            var level2 = Level.ByLevelAndOffset(level, offset);
            Assert.NotNull(level2);

            Assert.AreEqual(elevation + offset, level2.Elevation);
            Assert.AreEqual(elevation + offset, level2.ProjectElevation);
        }

        [Test]
        public void ByLevelAndOffset_NullArgument()
        {
            var offset = 100;
            Assert.Throws(typeof(ArgumentNullException), () => Level.ByLevelAndOffset(null, offset));
        }

        [Test]
        public void ByLevelOffsetAndName_ValidArgs()
        {
            var elevation = 100;
            var offset = 100;
            var name = "Ham";
            var level = Level.ByElevation(elevation);

            var level2 = Level.ByLevelOffsetAndName(level, offset, name);
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
            var level = Level.ByElevation(elevation);

            Assert.Throws(typeof(ArgumentNullException), () => Level.ByLevelOffsetAndName(null, offset, name));
            Assert.Throws(typeof(ArgumentNullException), () => Level.ByLevelOffsetAndName(level, offset, null));
        }
    }

}

