using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class FamilyInstanceTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByCoordinates_ValidInput()
        {
            var famSym = FamilySymbol.ByName("Box");
            var famInst = FamilyInstance.ByCoordinates(famSym, 0, 1, 2);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            Assert.AreEqual(0, position.X);
            Assert.AreEqual(1, position.Y);
            Assert.AreEqual(2, position.Z);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByPoint_ValidInput()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            Assert.AreEqual(0, position.X);
            Assert.AreEqual(1, position.Y);
            Assert.AreEqual(2, position.Z);
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
