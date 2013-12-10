using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    public class SolidTests
    {
        [Test]
        public void ByExtrusion_ValidArgs()
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
                Point.ByCoordinates(0.8,0.8,0),
                Point.ByCoordinates(0.4,0.4,0),
                Point.ByCoordinates(0.0,0,0)
            };

            var wts = new double[]
            {
                1,1,1,1
            };

            var crvs = new[]
            {
                DSNurbSpline.ByControlPointsAndWeights(pts1, wts),
                DSNurbSpline.ByControlPointsAndWeights(pts2, wts),
                DSNurbSpline.ByControlPointsAndWeights(pts3, wts)
            };

            // construct the curveloop
            var curveloop = DSCurveLoop.ByCurves(crvs);

            var dir = Vector.ByCoordinates(0, 0, 1);
            var dist = 5;
            var extrusion = DSSolid.ByExtrusion(curveloop, dir, dist);

            Assert.NotNull(extrusion);
            Assert.AreEqual(2.5, extrusion.Volume, 0.01);
            Assert.AreEqual(5 + 5 + 0.5 + 0.5 + Math.Sqrt(2) * 5, extrusion.SurfaceArea, 0.01);

        }

        [Test]
        public void CanCreateARevolveWithValidArguments()
        {
            //create a curve curves
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,2,0),
                Point.ByCoordinates(0.8,4,0),
                Point.ByCoordinates(1,2,0),
            };
            var wts = new double[]
            {
                1,1,1,1
            };
            var crv1 = DSNurbSpline.ByControlPointsAndWeights(pts1, wts);

            var origin = Point.ByCoordinates(0, 0, 0);
            var axis = Vector.ByCoordinateArrayN(new double[] {.5, .5, .5});
            
            var revolve = DSSolid.ByRevolve(new List<DSCurve>{crv1}, origin, axis, 0, 180);
            Assert.NotNull(revolve);
        }
    }
}
