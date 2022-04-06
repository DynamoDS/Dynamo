using NUnit.Framework;

namespace DSCoreNodesTests
{
    static class LogicTests
    {
        [Test]
        [Category("UnitTests")]
        public static void Xor()
        {
            Assert.AreEqual(false, DSCore.Math.Xor(true, true));
            Assert.AreEqual(true, DSCore.Math.Xor(true, false));
            Assert.AreEqual(true, DSCore.Math.Xor(false, true));
            Assert.AreEqual(false, DSCore.Math.Xor(false, false));
        }
    }
}
