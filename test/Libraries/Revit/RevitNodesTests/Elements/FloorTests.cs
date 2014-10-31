using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class FloorTests : RevitNodeTestBase
    {

        private double BoundingBoxVolume(BoundingBox bb)
        {
            var val = bb.MaxPoint.Subtract(bb.MinPoint.AsVector());
            return Math.Abs( val.X * val.Y * val.Z );
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByOutlineTypeAndLevel_CurveArrayFloorTypeLevel_ProducesFloorWithCorrectArea()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);

            var outline = new[]
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(100, 0, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 0, 0), Point.ByCoordinates(100, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 100, 0), Point.ByCoordinates(0, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100, 0), Point.ByCoordinates(0, 0, 0))
            };

            var floorType = FloorType.ByName("Generic - 12\"");

            var floor = Floor.ByOutlineTypeAndLevel(outline, floorType, level);

            BoundingBoxVolume(floor.BoundingBox).ShouldBeApproximately(100 * 100 * 0.3048, 1e-3);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByOutlineTypeAndLevel_PolyCurveFloorTypeLevel_ProducesFloorWithCorrectArea()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);

            var outline = new[]
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(100, 0, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 0, 0), Point.ByCoordinates(100, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 100, 0), Point.ByCoordinates(0, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100, 0), Point.ByCoordinates(0, 0, 0))
            };

            var polyCurveOutline = PolyCurve.ByJoinedCurves(outline);

            var floorType = FloorType.ByName("Generic - 12\"");

            var floor = Floor.ByOutlineTypeAndLevel(polyCurveOutline, floorType, level);

            BoundingBoxVolume(floor.BoundingBox).ShouldBeApproximately(100 * 100 * 0.3048, 1e-3);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByOutlineTypeAndLevel_CurveArrayFloorTypeLevel_ThrowsExceptionWithNullArgument()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);

            var outline = new[]
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(100, 0, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 0, 0), Point.ByCoordinates(100, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(100, 100, 0), Point.ByCoordinates(0, 100, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100, 0), Point.ByCoordinates(0, 0, 0))
            };

            var floorType = FloorType.ByName("Generic - 12\"");

            Assert.Throws(typeof(ArgumentNullException), () => Floor.ByOutlineTypeAndLevel(outline, null, level));
            Assert.Throws(typeof(ArgumentNullException), () => Floor.ByOutlineTypeAndLevel(outline, floorType, null));
        }
    }
}

