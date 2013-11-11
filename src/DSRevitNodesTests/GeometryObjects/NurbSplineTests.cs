using Autodesk.DesignScript.Geometry;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    public class NurbSplineTests
    {

        [Test]
        public void ByControlPointsAndWeights_ValidArgs()
        {
            var pts = new Autodesk.DesignScript.Geometry.Point[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0)
            };

            var wts = new double[]
            {
                1,1
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
                Point.ByCoordinates(1,0,0)
            };

            var wts = new double[]
            {
                1,1
            };

            var knots = new double[]
            {
                0,1
            };

            var spline = DSNurbSpline.ByControlPointsWeightsKnotsDegreeClosedAndRationality(pts, wts, knots, 1, false,
                false);
            Assert.NotNull(spline);

        }

    }
}
