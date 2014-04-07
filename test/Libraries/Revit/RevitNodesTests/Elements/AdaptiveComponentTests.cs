using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests
{
    [TestFixture]
    class AdaptiveComponentTests : ProtoGeometryTest
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
            var fs = FamilySymbol.ByName("3PointAC");
            var ac = AdaptiveComponent.ByPoints(pts, fs);

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
            var fs = FamilySymbol.ByName("3PointAC");

            Assert.Throws(typeof (Exception), () => AdaptiveComponent.ByPoints(pts, fs));
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

            Assert.Throws(typeof(ArgumentNullException), () => AdaptiveComponent.ByPoints(pts, null));
        }

        [Test]
        public void ByPoints_NullPts()
        {
            var fs = FamilySymbol.ByName("3PointAC");

            Assert.Throws(typeof(ArgumentNullException), () => AdaptiveComponent.ByPoints(null, fs));
        }

        [Test]
        public void ByPointsOnCurve_ValidInput()
        {
            // create spline
            var pts = new Autodesk.DesignScript.Geometry.Point[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(3,0,0),
                Point.ByCoordinates(10,0,0),
                Point.ByCoordinates(12,0,0),
            };

            var spline = NurbsCurve.ByControlPoints(pts, 3);
            Assert.NotNull(spline);

            // build model curve from spline
            var modCurve = ModelCurve.ByCurve(spline);
            Assert.NotNull(modCurve);

            // obtain the family from the document
            var fs = FamilySymbol.ByName("3PointAC");

            // build the AC
            var parms = new double[]
            {
                0, 0.5, 1
            };

            var ac = AdaptiveComponent.ByParametersOnCurveReference(parms, modCurve.CurveReference, fs);
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
