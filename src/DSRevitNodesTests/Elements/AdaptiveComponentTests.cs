using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
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

        [Test]
        public void ByPointsOnCurve_ValidInput()
        {

           // build the curve
            var pts = new Autodesk.DesignScript.Geometry.Point[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(3,0,0),
                Point.ByCoordinates(10,0,0)
            };

            var wts = new double[]
            {
                1,1,1,1
            };

            var crv = DSNurbSpline.ByControlPointsAndWeights(pts, wts);

            // obtain the family from the document
            var fs = DSFamilySymbol.ByName("3PointAC.3PointAC");

            // build the AC
            var parms = new double[]
            {
                0, 0.5, 1
            };

            var ac = DSAdaptiveComponent.ByPointsOnCurve(parms, crv, fs);
            Assert.NotNull(ac);

        }

        [Test]
        public void ByPointsOnFace_ValidInput()
        {
            Assert.Inconclusive();
        }

        // tests for properties, null input


    }
}
