using System;

using NUnit.Framework;

using Revit.Elements;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class FamilySymbolTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByName_GoodArgs()
        {
            var famSym = FamilySymbol.ByName("Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }

        [Test, Category("Failure")]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByName_BadArgs()
        {
            Assert.Throws(typeof(Exception), () => FamilySymbol.ByName("Turtle.BoxTurtle") );
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilySymbol.ByName(null));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByFamilyAndName_GoodArgs()
        {
            var fam = Family.ByName("Box");
            var famSym = FamilySymbol.ByFamilyAndName(fam, "Box");
            Assert.NotNull(famSym);
            Assert.AreEqual("Box", famSym.Name);
            Assert.AreEqual("Box", famSym.Family.Name);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByFamilyAndName_BadArgs()
        {
            var fam = Family.ByName("Box");
            Assert.Throws(typeof(Exception), () => FamilySymbol.ByFamilyAndName(fam, "Turtle"));
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilySymbol.ByFamilyAndName(fam, null));
            Assert.Throws(typeof(System.ArgumentNullException), () => FamilySymbol.ByFamilyAndName(null, "Turtle"));
        }
    }
}
