using System;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class FloorTests
    {
        [Test]
        public void ByOutlineTypeAndLevel_ValidArgs()
        {
            var elevation = 100;
            var level = DSLevel.ByElevation(elevation);

            var outline = new[]
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(100, 0, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 0, 0), Point.ByCoordinates(100, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 100, 0), Point.ByCoordinates(0, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100, 0), Point.ByCoordinates(0, 0, 0))
            };

            var floorType = DSFloorType.ByName("Generic - 12\"");

            var floor = DSFloor.ByOutlineTypeAndLevel(outline, floorType, level);
            Assert.NotNull(floor);
        }

        [Test]
        public void ByOutlineTypeAndLevel_NullArgument()
        {
            var elevation = 100;
            var level = DSLevel.ByElevation(elevation);

            var outline = new[]
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(100, 0, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 0, 0), Point.ByCoordinates(100, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 100, 0), Point.ByCoordinates(0, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100, 0), Point.ByCoordinates(0, 0, 0))
            };

            var floorType = DSFloorType.ByName("Generic - 12\"");

            Assert.Throws(typeof(ArgumentNullException), () => DSFloor.ByOutlineTypeAndLevel(null, floorType, level));
            Assert.Throws(typeof(ArgumentNullException), () => DSFloor.ByOutlineTypeAndLevel(outline, null, level));
            Assert.Throws(typeof(ArgumentNullException), () => DSFloor.ByOutlineTypeAndLevel(outline, floorType, null));
        }
    }
}

