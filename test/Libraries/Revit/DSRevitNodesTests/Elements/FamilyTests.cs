using System;
using Revit;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class FamilyTests 
    {
        [Test]
        public void ByName_ValidInput()
        {
            var fam = Family.ByName("Cone");
            Assert.NotNull(fam);
            Assert.AreEqual("Cone",fam.Name);
            Assert.AreEqual(1, fam.Symbols.Length);
        }

        [Test]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => Family.ByName("Turtle"));
        }

        [Test]
        public void ByName_NullInput()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Family.ByName(null));
        }
    }
}
