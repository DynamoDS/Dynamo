using System;

using NUnit.Framework;

using Object = DSCore.Object;

namespace DSCoreNodesTests
{
    [TestFixture]
    internal static class ObjectTests
    {
        [Test]
        [Category("UnitTests")]
        public static void Type()
        {
            Assert.Throws<ArgumentException>(() => Object.Type(null));

            var result = Object.Type(string.Empty);
            Assert.AreEqual("System.String", result);

            result = Object.Type(0);
            Assert.AreEqual("System.Int32", result);

            result = Object.Type(0.0);
            Assert.AreEqual("System.Double", result);
        }
        [Test]
        [Category("UnitTests")]
        public static void Test_Identity()
        {
            Assert.AreEqual(string.Empty,Object.Identity(string.Empty));
        }
    }
}
