using Autodesk.DesignScript.Geometry;
using Revit.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    internal class CurveLoopTests
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
        public void RectangleByCurves_ValidArgs()
        {
            // construct a unit rectangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,0,0),
                Point.ByCoordinates(0.8,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,0.4,0),
                Point.ByCoordinates(1,0.8,0),
                Point.ByCoordinates(1,1,0)
            };

            var pts3 = new[]
            {
                Point.ByCoordinates(1,1,0),
                Point.ByCoordinates(0.8,1,0),
                Point.ByCoordinates(0.4,1,0),
                Point.ByCoordinates(0.0,1,0)
            };

            var pts4 = new[]
            {
                Point.ByCoordinates(0,1,0),
                Point.ByCoordinates(0,0.8,0),
                Point.ByCoordinates(0,0.4,0),
                Point.ByCoordinates(0,0,0)
            };

            var crvs = new[]
            {
                NurbsCurve.ByPoints(pts1),
                NurbsCurve.ByPoints(pts2),
                NurbsCurve.ByPoints(pts3),
                NurbsCurve.ByPoints(pts4)
            };

            // construct the curveloop
            var curveloop = CurveLoop.ByCurves(crvs);
            Assert.NotNull(curveloop);
            Assert.IsTrue(curveloop.IsPlanar);
            Assert.IsTrue(curveloop.IsClosed);
        }

        [Test]
        public void TriangleByCurves_ValidArgs()
        {
            // construct a triangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,0,0),
                Point.ByCoordinates(0.8,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,0.4,0),
                Point.ByCoordinates(1,0.8,0),
                Point.ByCoordinates(1,1,0)
            };

            var pts3 = new[]
            {
                Point.ByCoordinates(1,1,0),
                Point.ByCoordinates(0.8,0.8,0),
                Point.ByCoordinates(0.4,0.4,0),
                Point.ByCoordinates(0.0,0,0)
            };

            var crvs = new[]
            {
                NurbsCurve.ByPoints(pts1),
                NurbsCurve.ByPoints(pts2),
                NurbsCurve.ByPoints(pts3)
            };

            // construct the curveloop
            var curveloop = CurveLoop.ByCurves(crvs);
            Assert.NotNull(curveloop);
            Assert.IsTrue(curveloop.IsPlanar);
            Assert.IsTrue(curveloop.IsClosed);
        }

        [Test]
        public void OpenCurveByCurves_ValidArgs()
        {
            // construct a unit rectangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,0,0),
                Point.ByCoordinates(0.8,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,0.4,0),
                Point.ByCoordinates(1,0.8,0),
                Point.ByCoordinates(1,1,0)
            };

            var crvs = new[]
            {
                NurbsCurve.ByPoints(pts1),
                NurbsCurve.ByPoints(pts2)
            };

            // construct the curveloop
            var curveloop = CurveLoop.ByCurves(crvs);
            Assert.NotNull(curveloop);
            Assert.IsTrue(curveloop.IsPlanar);
            Assert.IsFalse(curveloop.IsClosed);
        }

        [Test]
        public void NonPlanarOpenCurveByCurves_ValidArgs()
        {
            // construct a unit rectangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,0,0),
                Point.ByCoordinates(0.8,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,0.4,0.4),
                Point.ByCoordinates(1,0.7,1.5),
                Point.ByCoordinates(1,1,1)
            };

            var crvs = new[]
            {
                NurbsCurve.ByPoints(pts1),
                NurbsCurve.ByPoints(pts2)
            };

            // construct the curveloop
            var curveloop = CurveLoop.ByCurves(crvs);
            Assert.NotNull(curveloop);
            Assert.IsFalse(curveloop.IsPlanar);
            Assert.IsFalse(curveloop.IsClosed);
        }
    }
}
