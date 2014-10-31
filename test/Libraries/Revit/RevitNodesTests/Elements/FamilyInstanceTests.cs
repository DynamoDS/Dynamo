using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

using FamilyInstance = Revit.Elements.FamilyInstance;
using FamilySymbol = Revit.Elements.FamilySymbol;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class FamilyInstanceTests : RevitNodeTestBase
    {
        public Autodesk.Revit.DB.XYZ InternalLocation(Autodesk.Revit.DB.FamilyInstance instance)
        {
            return (instance.Location as LocationPoint).Point;
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByCoordinates_ProducesValidFamilyInstanceWithCorrectLocation()
        {
            var famSym = FamilySymbol.ByName("Box");
            var famInst = FamilyInstance.ByCoordinates(famSym, 0, 1, 2);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            position.ShouldBeApproximately(Point.ByCoordinates(0, 1, 2));
    
            // no unit conversion
            var internalPos =
                InternalLocation(famInst.InternalElement as Autodesk.Revit.DB.FamilyInstance);

            (internalPos * UnitConverter.HostToDynamoFactor).ShouldBeApproximately(
                Point.ByCoordinates(0, 1, 2));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByPoint_ProducesValidFamilyInstanceWithCorrectLocation()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            position.ShouldBeApproximately(Point.ByCoordinates(0, 1, 2));

            // no unit conversion
            var internalPos =
                InternalLocation(famInst.InternalElement as Autodesk.Revit.DB.FamilyInstance);

            (internalPos * UnitConverter.HostToDynamoFactor).ShouldBeApproximately(
                Point.ByCoordinates(0, 1, 2));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByPoint_NullFamilySymbol()
        {
            var pt = Point.ByCoordinates(0, 1, 2);
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilyInstance.ByPoint(null, pt));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByPoint_NullPoint()
        {
            var famSym = FamilySymbol.ByName("Box");
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilyInstance.ByPoint(famSym, null));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByCoordinates_NullFamilySymbol()
        {
            var pt = Point.ByCoordinates(0, 1, 2);
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilyInstance.ByCoordinates(null, 0, 1, 2));
        }

    }
}
