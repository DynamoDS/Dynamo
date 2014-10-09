using System;
using System.Linq;
using Revit.Elements;
using NUnit.Framework;

using Revit.GeometryConversion;

using RTF.Framework;
using Revit.Elements.InternalUtilities;

namespace RevitTestServices.Elements
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
        public void ByElevationAndName_DuplicatedNames()
        {
            //Create a new level with the name of "Ham" and the
            //elevation of 100
            var elevation = 100;
            var name = "Old Level";
            var level = Level.ByElevationAndName(elevation, name);
            Assert.NotNull(level);

            Assert.AreEqual(elevation, level.Elevation);
            Assert.AreEqual(name, level.Name);

            //Create a new level with the same name and elevation
            //Ensure there is a exception thrown
            Assert.Throws(typeof(System.Exception), ()=>Level.ByElevationAndName(elevation, name));

            //Create a new level with a name of lowercase letters
            //and the same elevation
            var name3 = "old level";
            var level3 = Level.ByElevationAndName(elevation, name3);
            Assert.IsNotNull(level3);

            Assert.AreEqual(elevation, level3.Elevation);
            Assert.AreEqual(name3, level3.Name);

            //Create a new level with a totally different name
            var name4 = "New level";
            var level4 = Level.ByElevationAndName(elevation, name4);
            Assert.NotNull(level4);

            Assert.AreEqual(elevation, level4.Elevation);
            Assert.AreEqual(name4, level4.Name);
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

        [Test]
        [TestModel(@".\empty.rfa")]
        public void GetAllLevels()
        {
            var levels = ElementQueries.GetAllLevels();
            Assert.AreEqual(levels.Count(), 1.0);
        }
    }

}

