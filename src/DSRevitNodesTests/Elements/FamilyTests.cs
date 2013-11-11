using System;
using DSRevitNodes;
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

        [Test]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => DSFamily.ByName("Turtle"));
        }

        [Test]
        public void ByName_NullInput()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSFamily.ByName(null));
        }
    }
}
