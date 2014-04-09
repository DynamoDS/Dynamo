using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class WallTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveAndHeight_ValidArgs()
        {
            var elevation = 0;
            var level = Level.ByElevation(elevation);
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));
            var wallType = WallType.ByName( "Curtain Wall 1" );

            var wall = Wall.ByCurveAndHeight(line, 10, level, wallType);

            Assert.NotNull(wall);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveAndHeight_NullArgs()
        {
            var elevation = 0;
            var level = Level.ByElevation(elevation);
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));
            var wallType = WallType.ByName("Curtain Wall 1");

            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndHeight(null, 10, level, wallType));
            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndHeight(line, 10, null, wallType));
            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndHeight(line, 10, level, null));        
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveAndLevels_ValidArgs()
        {
            // prevent rebinding when creating levels
            ElementBinder.IsEnabled = false;

            var elevation = 100;
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 100), Point.ByCoordinates(10, 10, 100));
            var level0 = Level.ByElevation(elevation);
            var level1 = Level.ByElevation(elevation + 100);
            var wallType = WallType.ByName("Curtain Wall 1");

            var wall = Wall.ByCurveAndLevels(line, level0, level1, wallType);

            Assert.NotNull(wall);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCurveAndLevels_NullArgs()
        {
            var elevation = 100;
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));
            var level0 = Level.ByElevation(elevation);
            var level1 = Level.ByElevation(elevation + 100);
            var wallType = WallType.ByName("Curtain Wall 1");

            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndLevels(null, level0, level1, wallType));
            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndLevels(line, null, level1, wallType));
            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndLevels(line, level0, null, wallType));
            Assert.Throws(typeof(ArgumentNullException), () => Wall.ByCurveAndLevels(line, level0, level1, null));
        }
    }
}

