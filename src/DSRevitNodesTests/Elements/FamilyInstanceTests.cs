using DSRevitNodes;
using NUnit.Framework;

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
    }
}
