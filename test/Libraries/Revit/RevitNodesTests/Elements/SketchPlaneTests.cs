using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class SketchPlaneTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByPlane_ValidArgs()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);

            var sketchPlane = SketchPlane.ByPlane(plane);

            Assert.NotNull(sketchPlane);
            Assert.NotNull(sketchPlane.Plane);
            Assert.AreEqual(normal, sketchPlane.Plane.Normal);
            Assert.AreEqual(origin, sketchPlane.Plane.Origin);
            Assert.NotNull(sketchPlane.PlaneReference);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByPlane_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => SketchPlane.ByPlane(null));
        }
    }
}
