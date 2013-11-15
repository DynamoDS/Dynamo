using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelCurveTests
    {
        [Test]
        public void ByCurve_ValidArgs()
        {
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

            try
            {
                
            }
            var spline = DSNurbSpline.ByControlPointsAndWeights(pts, wts);
            Assert.NotNull(spline);

            var modelCurve = DSModelCurve.ByPlanarCurve(spline);
            Assert.NotNull(spline);

            var curveRef = modelCurve.CurveReference;
            Assert.NotNull(curveRef);
        }
    }
}
