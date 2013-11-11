using DSRevitNodes;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class FamilySymbolTests
    {
        [Test]
        public void ByName_ValidInput()
        {
            var famSym = DSFamilySymbol.ByName("Box.Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }
    }
}
