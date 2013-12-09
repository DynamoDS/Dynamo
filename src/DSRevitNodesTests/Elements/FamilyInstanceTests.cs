using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.Exceptions;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class FamilyInstanceTests 
    {
        [Test]
        public void ByCoordinates_ValidInput()
        {
            var famSym = DSFamilySymbol.ByName("Box.Box");
            var famInst = DSFamilyInstance.ByCoordinates(famSym, 0, 1, 2);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            Assert.AreEqual(0, position.X);
            Assert.AreEqual(1, position.Y);
            Assert.AreEqual(2, position.Z);
        }

        [Test]
        public void ByPoint_ValidInput()
        {
            var famSym = DSFamilySymbol.ByName("Box.Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);
            Assert.NotNull(famInst);

            var position = famInst.Location;

            Assert.AreEqual(0, position.X);
            Assert.AreEqual(1, position.Y);
            Assert.AreEqual(2, position.Z);
        }

        [Test]
        public void ByPoint_NullFamilySymbol()
        {
            var pt = Point.ByCoordinates(0, 1, 2);
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilyInstance.ByPoint(null, pt));
        }

        [Test]
        public void ByPoint_NullPoint()
        {
            var famSym = DSFamilySymbol.ByName("Box.Box");
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilyInstance.ByPoint(famSym, null));
        }

        [Test]
        public void ByCoordinates_NullFamilySymbol()
        {
            var pt = Point.ByCoordinates(0, 1, 2);
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilyInstance.ByCoordinates(null, 0, 1, 2));
        }

    }
}
