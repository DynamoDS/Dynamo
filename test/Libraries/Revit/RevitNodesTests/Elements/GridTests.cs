using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    class GridTests : RevitNodeTestBase
    {
        [Test, Category("Failure")]
        [TestModel(@".\Empty.rvt")]
        public void ByLine_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));

            var grid = Grid.ByLine(line);

            Assert.NotNull(grid);
            Assert.NotNull(grid.Curve);
            Assert.NotNull(grid.ElementCurveReference);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByLine_NullArgs()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => Grid.ByLine(null));
        }

        [Test, Category("Failure")]
        [TestModel(@".\Empty.rvt")]
        public void ByStartPointEndPoint_ValidArgs()
        {
            var grid = Grid.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));

            Assert.NotNull(grid);
            Assert.NotNull(grid.Curve);
            Assert.NotNull(grid.ElementCurveReference);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByStartPointEndPoint_NullArgs()
        {
            var p = Point.ByCoordinates(0, 0, 0);

            Assert.Throws(typeof(System.ArgumentNullException), () => Grid.ByStartPointEndPoint(p, null));
            Assert.Throws(typeof(System.ArgumentNullException), () => Grid.ByStartPointEndPoint(null, p));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByArc_ValidArgs()
        {
            var arc = Arc.ByCenterPointRadiusAngle(Point.ByCoordinates(0, 0, 0), 1000, 0, 90,
                Vector.ByCoordinates(0, 0, 1));
            var grid = Grid.ByArc(arc);

            Assert.NotNull(grid);
            Assert.NotNull(grid.Curve);
            Assert.NotNull(grid.ElementCurveReference);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByArc_NullArgs()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => Grid.ByArc(null));
        }

    }
}
