using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class SketchPlaneTests : RevitNodeTestBase
    {
        [Test, Category("Failure")]
        [TestModel(@".\Empty.rvt")]
        public void ByPlane_CanBeUsedToCreateSketchPlaneInProjectDocument()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);

            var sketchPlane = SketchPlane.ByPlane(plane);

            Assert.NotNull(sketchPlane);
            Assert.NotNull(sketchPlane.Plane);
            Assert.AreEqual(normal, sketchPlane.Plane.Normal);
            Assert.AreEqual(origin, sketchPlane.Plane.Origin);
            Assert.NotNull(sketchPlane.ElementPlaneReference);
        }

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void ByPlane_CanBeUsedToCreateSketchPlaneInFamilyDocument()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);

            var sketchPlane = SketchPlane.ByPlane(plane);

            Assert.NotNull(sketchPlane);
            Assert.NotNull(sketchPlane.Plane);
            Assert.AreEqual(normal, sketchPlane.Plane.Normal);
            Assert.AreEqual(origin, sketchPlane.Plane.Origin);
            Assert.NotNull(sketchPlane.ElementPlaneReference);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByPlane_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => SketchPlane.ByPlane(null));
        }
    }
}
