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
    class DividedPathTests
    {
        [Test]
        public void ByCurveAndEqualDivisions_ValidArgs()
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
            var divPath = DSDividedPath.ByCurveAndEqualDivisions(spline, 5);
            Assert.NotNull(divPath);

        }
    }
}
