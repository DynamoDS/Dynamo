using System;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class FamilySymbolTests 
    {
        [Test]
        public void ByName_GoodArgs()
        {
            var famSym = DSFamilySymbol.ByName("Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }

        [Test]
        public void ByName_BadArgs()
        {
            Assert.Throws(typeof(Exception), () => DSFamilySymbol.ByName("Turtle.BoxTurtle") );
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilySymbol.ByName(null));
        }

        [Test]
        public void ByFamilyAndName_GoodArgs()
        {
            var fam = DSFamily.ByName("Box");
            var famSym = DSFamilySymbol.ByFamilyAndName(fam, "Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }

        [Test]
        public void ByFamilyAndName_BadArgs()
        {
            var fam = DSFamily.ByName("Box");
            Assert.Throws(typeof(Exception), () => DSFamilySymbol.ByFamilyAndName(fam, "Turtle"));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilySymbol.ByFamilyAndName(fam, null));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilySymbol.ByFamilyAndName(null, "Turtle"));
        }
    }
}
