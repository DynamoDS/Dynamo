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
    public class BoundingBoxTests
    {
        [Test]
        public void ByExtrusion_ValidArgs()
        {


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
    }
}
