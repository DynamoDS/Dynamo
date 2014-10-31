using System.Linq;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

using Point = Autodesk.DesignScript.Geometry.Point;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ImportInstanceTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByGeometry_ProvidesExpectedConversion()
        {
            // construct the cuboid in meters
            var c0 = Point.ByCoordinates(-1, -1, -1);
            var c1 = Point.ByCoordinates(1, 1, 1);
            var cuboid = Cuboid.ByCorners(c0, c1);

            // import
            var import = Revit.Elements.ImportInstance.ByGeometry(cuboid);

            // extract geometry
            var importGeometry = import.Geometry().First();

            Assert.IsAssignableFrom(typeof(Autodesk.DesignScript.Geometry.Solid), importGeometry);

            var solidImport = (Autodesk.DesignScript.Geometry.Solid)importGeometry;

            cuboid.Volume.ShouldBeApproximately(solidImport.Volume);
            cuboid.BoundingBox.MinPoint.ShouldBeApproximately(solidImport.BoundingBox.MinPoint);
            cuboid.BoundingBox.MaxPoint.ShouldBeApproximately(solidImport.BoundingBox.MaxPoint);
        }
    }
}
