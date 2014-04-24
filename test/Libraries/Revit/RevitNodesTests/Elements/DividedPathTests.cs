using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class DividedPathTests : RevitNodeTestBase
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
            base.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
            base.TearDown();
        }

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
            var divPath = DividedPath.ByCurveAndDivisions(modCurve.CurveReference, 5);
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
            Assert.Throws(typeof(Exception), () => DividedPath.ByCurveAndDivisions(modCurve.CurveReference, 0));

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
            var divPath = DividedPath.ByCurveAndDivisions(modCurve.CurveReference, 5);
            Assert.NotNull(divPath);

            foreach (var pt in divPath.Points)
            {
                Assert.IsTrue(pts.Any(x => x.ShouldBeApproximately(pt)));
            }
        }
    }
}
