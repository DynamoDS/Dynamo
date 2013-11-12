using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    class CurveLoopTests
    {
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

            var wts = new double[]
            {
                1,1,1,1
            };

            var crvs = new[]
            {
                DSNurbSpline.ByControlPointsAndWeights(pts1, wts),
                DSNurbSpline.ByControlPointsAndWeights(pts2, wts),
                DSNurbSpline.ByControlPointsAndWeights(pts3, wts),
                DSNurbSpline.ByControlPointsAndWeights(pts4, wts)
            };

            // construct the curveloop
            var curveloop = DSCurveLoop.ByCurves(crvs);
            Assert.NotNull(curveloop);
            Assert.IsTrue(curveloop.IsPlanar);
            Assert.IsTrue(curveloop.IsClosed);
        }
    }
}
