using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class FamilyTests
    {
        [Test]
        public void ByName_ValidInput()
        {
            var fam = DSFamily.ByName("Cone");
            Assert.NotNull(fam);
            Assert.AreEqual("Cone",fam.Name);
            Assert.AreEqual(1, fam.Symbols.Length);
        }
    }
}
