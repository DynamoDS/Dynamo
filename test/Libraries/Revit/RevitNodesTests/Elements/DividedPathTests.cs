using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RevitServices.Persistence;

using RTF.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class DividedPathTests : GeometricRevitNodeTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCurveAndEqualDivisions_ValidArgs()
        {
            // create spline
            var pts = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(3,0,0),
                Point.ByCoordinates(10,0,0),
                Point.ByCoordinates(12,0,0)
            };

            var spline = NurbsCurve.ByControlPoints( pts, 3 );
            Assert.NotNull(spline);

            // build model curve from spline
            var modCurve = ModelCurve.ByCurve(spline);
            Assert.NotNull(modCurve);

            // build dividedPath
            var divPath = DividedPath.ByCurveAndDivisions(modCurve.ElementCurveReference, 5);
            Assert.NotNull(divPath);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCurveAndEqualDivisions_NullArgument()
        {
            // build dividedPath
            Assert.Throws(typeof (ArgumentNullException), () => DividedPath.ByCurveAndDivisions(null, 5));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCurveAndEqualDivisions_InvalidDivisions()
        {
            // create spline
            var pts = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(3,0,0),
                Point.ByCoordinates(10,0,0),
                Point.ByCoordinates(12,0,0)
            };

            var spline = NurbsCurve.ByControlPoints(pts, 3);
            Assert.NotNull(spline);

            // build model curve from spline
            var modCurve = ModelCurve.ByCurve(spline);
            Assert.NotNull(modCurve);

            // build dividedPath
            Assert.Throws(typeof(Exception), () => DividedPath.ByCurveAndDivisions(modCurve.ElementCurveReference, 0));

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Points()
        {
            // create spline
            var pts = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(3,0,0),
                Point.ByCoordinates(10,0,0),
                Point.ByCoordinates(12,0,0)
            };

            var spline = NurbsCurve.ByControlPoints(pts, 3);
            Assert.NotNull(spline);

            // build model curve from spline
            var modCurve = ModelCurve.ByCurve(spline);
            Assert.NotNull(modCurve);

            // build dividedPath
            var divPath = DividedPath.ByCurveAndDivisions(modCurve.ElementCurveReference, 5);
            Assert.NotNull(divPath);

            var divPathPts = divPath.Points;
            Assert.AreEqual(5, divPathPts.Count());

            foreach (var pt in divPathPts)
            {
                // all of the points should be along the curve
                spline.DistanceTo(pt).ShouldBeApproximately(0);
            }
        }
    }
}
