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
    public class WallTests
    {
        [Test]
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
        public void ByCurveAndLevels_ValidArgs()
        {
            var elevation = 100;
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));
            var level0 = Level.ByElevation(elevation);
            var level1 = Level.ByElevation(elevation + 100);
            var wallType = WallType.ByName("Curtain Wall 1");

            var wall = Wall.ByCurveAndLevels(line, level0, level1, wallType);

            Assert.NotNull(wall);
        }

        [Test]
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

