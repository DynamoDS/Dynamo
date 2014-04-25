using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using NUnit.Framework;
using Revit.Elements;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    class TopographyTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rvt")]
        public void ByPoints_ValidArgs()
        {   
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p2 = Point.ByCoordinates(1, 3, 4);
            var p3 = Point.ByCoordinates(27, 5, 12);
            var p4 = Point.ByCoordinates(-10, 8, -3);
            var p5 = Point.ByCoordinates(.005, 0.01, -10);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);

            var topoSurf = Topography.ByPoints(new List<Point> { p1,p2,p3,p4,p5 });
            Assert.NotNull(topoSurf);

            topoSurf.Points[0].AssertShouldBeApproximately(p1);
            topoSurf.Points[1].AssertShouldBeApproximately(p2);
            topoSurf.Points[2].AssertShouldBeApproximately(p3);
            topoSurf.Points[3].AssertShouldBeApproximately(p4);
            topoSurf.Points[4].AssertShouldBeApproximately(p5);
        }

        [Test]
        [TestModel(@".\empty.rvt")]
        public void ByPoints_InvalidArgs()
        {
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p2 = Point.ByCoordinates(1, 3, 4);

            Assert.NotNull(p1);
            Assert.NotNull(p2);

            Assert.Throws<System.Exception>(()=>Topography.ByPoints(new List<Point> { p1, p2 }));
        }

        [Test]
        [TestModel(@".\empty.rvt")]
        public void ByPoints_Mutation()
        {
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p2 = Point.ByCoordinates(1, 3, 4);
            var p3 = Point.ByCoordinates(27, 5, 12);
            var p4 = Point.ByCoordinates(-10, 8, -3);
            var p5 = Point.ByCoordinates(.005, 0.01, -10);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);

            var topoSurf = Topography.ByPoints(new List<Point> { p1, p2, p3, p4, p5 });
            Assert.NotNull(topoSurf);

            var p6 = Point.ByCoordinates(10, 42, -3.3);
            topoSurf = Topography.ByPoints(new List<Point> {p1, p2, p3, p4, p6});
            Assert.NotNull(topoSurf);

            topoSurf.Points[4].AssertShouldBeApproximately(p6);
        }
    }
}
