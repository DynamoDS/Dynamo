using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Revit.Elements;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    class TopographyTests
    {
        [Test]
        public void ByPoints_ValidArgs()
        {
            //ElementBinder.IsEnabled = false;

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

            //p1.ShouldBeApproximately(topoSurf.Points[0]);
            //p2.ShouldBeApproximately(topoSurf.Points[1]);
            //p3.ShouldBeApproximately(topoSurf.Points[2]);
            //p4.ShouldBeApproximately(topoSurf.Points[3]);
            //p5.ShouldBeApproximately(topoSurf.Points[4]);
        }

        [Test]
        public void ByPoints_InvalidArgs()
        {

        }
    }
}
