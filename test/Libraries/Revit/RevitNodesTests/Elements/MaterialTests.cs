using System;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class MaterialTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var name = "Cherry";
            var material = Material.ByName(name);
            Assert.NotNull(material);
            Assert.AreEqual(name, material.Name);
        }

        [Test]
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

