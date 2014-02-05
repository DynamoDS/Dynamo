using Autodesk.DesignScript.Geometry;
using DSCoreNodes;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSCoreNodesTests
{
    [TestFixture]
    class GeometryTests
    {
        /// <summary>
        /// Example of calling ProtoGeometry methods from C#
        /// This can be removed but is a good test to assert that ProtoGeometry
        /// is working.  
        /// </summary>
        [Test]
        public void NurbsCurve()
        {
            HostFactory.Instance.StartUp();

            // create spline
            var pts = new Autodesk.DesignScript.Geometry.Point[]
            {
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(1,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(3,0,0),
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(10,0,0)
            };

            var spline = Autodesk.DesignScript.Geometry.NurbsCurve.ByControlVertices(pts, 3);

            Assert.NotNull(spline);
            Assert.AreEqual(3, spline.Degree);
            var eval0 = spline.PointAtParameter(0);
            var eval1 = spline.PointAtParameter(1);

            var expectedPoint0 = Point.ByCoordinates(0, 0, 0);
            var expectedPoint1 = Point.ByCoordinates(10, 0, 0);

            Assert.AreEqual(0, expectedPoint0.DistanceTo(eval0), 1e-6);
            Assert.AreEqual(0, expectedPoint1.DistanceTo(eval1), 1e-6);

            var closestPoint0 = spline.GetClosestPoint(expectedPoint0);
            Assert.AreEqual(0, expectedPoint0.DistanceTo(closestPoint0), 1e-6);

            HostFactory.Instance.ShutDown();
        }

        [Test]
        public void Domain2D()
        {

            //create a domain object
            var dom = new Domain2D(Vector.ByCoordinates(0, 0, 0), Vector.ByCoordinates(1, 1, 0));

            //assert the min and max of the domain
            Assert.IsTrue(dom.Min.X == 0);
            Assert.IsTrue(dom.Min.Y == 0);
            Assert.IsTrue(dom.Max.X == 1);
            Assert.IsTrue(dom.Max.Y == 1);

            //assert the spans of the domain
            Assert.IsTrue(dom.USpan == 1);
            Assert.IsTrue(dom.VSpan == 1);
        }

        [Test]
        public void Domain()
        {
            //create a domain object
            var dom = new Domain(0, 1);

            //assert the min and max of the domain
            Assert.IsTrue(dom.Min == 0);
            Assert.IsTrue(dom.Max == 1);

            //assert the spans of the domain
            Assert.IsTrue(dom.Span == 1);
        }

    }
}
