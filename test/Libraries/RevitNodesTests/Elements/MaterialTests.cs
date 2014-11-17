using System;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class MaterialTests : RevitNodeTestBase
    {
        [Test, Category("Failure")]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var name = "Cherry";
            var material = Material.ByName(name);
            Assert.NotNull(material);
            Assert.AreEqual(name, material.Name);
        }

        [Test, Category("Failure")]
        [TestModel(@".\Empty.rvt")]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Material.ByName(null) );
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => Material.ByName("Mayo"));
        }
    }
}

