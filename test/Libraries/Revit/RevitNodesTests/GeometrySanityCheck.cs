using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using RevitTestServices;

namespace RevitNodesTests
{
    [TestFixture]
    class GeometrySanityCheck : RevitNodeTestBase
    {
        /// <summary>
        /// Example of calling ProtoGeometry methods from C#
        /// This can be removed but is a good test to assert that ProtoGeometry
        /// is working.  
        /// </summary>
        [Test]
        public void NurbsCurve()
        {
            // create spline
            var pts = new Autodesk.DesignScript.Geometry.Point[]
            {
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(1,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(3,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(10,0,0)
            };

            var spline = Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(pts, 3);

            Assert.NotNull(spline);
            Assert.AreEqual(3, spline.Degree);
            var eval0 = spline.PointAtParameter(0);
            var eval1 = spline.PointAtParameter(1);

            var expectedPoint0 = Point.ByCoordinates(0, 0, 0);
            var expectedPoint1 = Point.ByCoordinates(10, 0, 0);

            Assert.AreEqual(0, expectedPoint0.DistanceTo(eval0), 1e-6);
            Assert.AreEqual(0, expectedPoint1.DistanceTo(eval1), 1e-6);

            var closestPoint0 = spline.ClosestPointTo(expectedPoint0);
            Assert.AreEqual(0, expectedPoint0.DistanceTo(closestPoint0), 1e-6);
        }

    }
}
