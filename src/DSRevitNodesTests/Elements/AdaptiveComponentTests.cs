using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests
{
    [TestFixture]
    class AdaptiveComponentTests
    {
        [Test]
        public void ByPoints_ValidInput()
        {
            var pts = new Point[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 10),
                Point.ByCoordinates(20, 0, 0)
            };
            var fs = DSFamilySymbol.ByName("3PointAC.3PointAC");
            var ac = DSAdaptiveComponent.ByPoints(pts, fs);

            Assert.NotNull(ac);
        }

        [Test]
        public void ByPoints_NonMatchingNumberOfPoints()
        {
            var pts = new Point[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 10)
            };
            var fs = DSFamilySymbol.ByName("3PointAC.3PointAC");

            Assert.Throws(typeof (Exception), () => DSAdaptiveComponent.ByPoints(pts, fs));
        }

        [Test]
        public void ByPoints_NullFamilySymbol()
        {
            var pts = new Point[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 10),
                Point.ByCoordinates(20, 0, 0)
            };

            Assert.Throws(typeof(ArgumentNullException), () => DSAdaptiveComponent.ByPoints(pts, null));
        }

        [Test]
        public void ByPoints_NullPts()
        {
            var fs = DSFamilySymbol.ByName("3PointAC.3PointAC");

            Assert.Throws(typeof(ArgumentNullException), () => DSAdaptiveComponent.ByPoints(null, fs));
        }

        // tests for properties, null input

        [Test]
        public void ByPointsOnFace_ValidInput()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void ByPointsOnCurve_ValidInput()
        {
            Assert.Inconclusive();
        }
    }
}
