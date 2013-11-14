using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class DividedPathTests : RevitTestBase
    {
        [Test]
        public void ByCurveAndEqualDivisions_CurveIsNotElementReference()
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

            var spline = DSNurbSpline.ByControlPointsAndWeights(pts, wts);
            Assert.NotNull(spline);

            Assert.Throws(typeof (Exception), () => DSDividedPath.ByCurveAndDivisions(spline, 5));
        }
    }
}
