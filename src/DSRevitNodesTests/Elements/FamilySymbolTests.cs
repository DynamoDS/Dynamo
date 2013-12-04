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
        public void ByName_ValidInput()
        {
            var famSym = DSFamilySymbol.ByName("Box.Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }

        [Test]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => DSFamilySymbol.ByName("Turtle.BoxTurtle") );
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFamilySymbol.ByName(null));
        }
    }
}
