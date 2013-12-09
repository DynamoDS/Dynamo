using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    internal class ReferencePointTests 
    {
        [Test]
        public void ByCoordinates_ValidInput()
        {
            var pt = DSReferencePoint.ByCoordinates(0, -10, 23.1);
            Assert.NotNull(pt);
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }
    }
}
