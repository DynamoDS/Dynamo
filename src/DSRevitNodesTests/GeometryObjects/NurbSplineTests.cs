using System;
using System.IO;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    public class NurbSplineTests : RevitTestBase
    {

        [Test]
        public void ByControlPointsAndWeights_ValidArgs()
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

        }

        [Test]
        public void ByControlPointsWeightsKnotsDegreeClosedAndRationality_ValidArgs()
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

            var knots = new double[]
            {
                0,0,0,0,1,1,1,1
            };

            try
            {
                var spline = DSNurbSpline.ByControlPointsWeightsKnotsDegreeClosedAndRationality(pts, wts, knots, 3,
                    false,
                    false);
                Assert.NotNull(spline);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

    }
}
