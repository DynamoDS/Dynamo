using System;
using Revit.Elements;
using NUnit.Framework;

using Revit.GeometryConversion;

using RTF.Framework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class LevelTests : RevitNodeTestBase
    {
        public static double InternalElevation(Revit.Elements.Level level)
        {
            return (level.InternalElement as Autodesk.Revit.DB.Level).Elevation;
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByElevationAndName_ShouldProduceLevelAtCorrectElevation()
        {
            // construct the extrusion
            var elevation = 100;
            var name = "Ham";
            var level = Level.ByElevationAndName(elevation, name);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);
            Assert.AreEqual(name, level.Name);

            // without unit conversion
            InternalElevation(level)
                .ShouldBeApproximately(elevation * UnitConverter.DynamoToHostFactor);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByElevationAndName_NullArgument()
        {
            var elevation = 100;

            Assert.Throws(typeof(ArgumentNullException), () => Level.ByElevationAndName(elevation, null));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByElevation_ShouldProduceLevelAtCorrectElevation()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(elevation, level.ProjectElevation);

            // without unit conversion
            InternalElevation(level)
                .ShouldBeApproximately(elevation * UnitConverter.DynamoToHostFactor);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLevelAndOffset_ValidArgs()
        {
            var elevation = 100;
            var offset = 100;
            var level = Level.ByElevation(elevation);

            var level2 = Level.ByLevelAndOffset(level, offset);
            Assert.NotNull(level2);

            Assert.AreEqual(elevation + offset, level2.Elevation);
            Assert.AreEqual(elevation + offset, level2.ProjectElevation);

            // without unit conversion
            InternalElevation(level2)
                .ShouldBeApproximately((elevation + offset) * UnitConverter.DynamoToHostFactor);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLevelAndOffset_NullArgument()
        {
            var offset = 100;
            Assert.Throws(typeof(ArgumentNullException), () => Level.ByLevelAndOffset(null, offset));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLevelOffsetAndName_ShouldProduceLevelAtCorrectElevation()
        {
            var elevation = 100;
            var offset = 100;
            var name = "TortoiseTime";
            var level = Level.ByElevation(elevation);

            var level2 = Level.ByLevelOffsetAndName(level, offset, name);
            Assert.NotNull(level2);

            Assert.AreEqual(elevation + offset, level2.Elevation);
            Assert.AreEqual(elevation + offset, level2.ProjectElevation);

            // without unit conversion
            InternalElevation(level2)
                .ShouldBeApproximately((elevation + offset) * UnitConverter.DynamoToHostFactor);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
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

