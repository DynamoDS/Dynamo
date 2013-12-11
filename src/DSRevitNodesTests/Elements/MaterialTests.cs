using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class MaterialTests
    {
        [Test]
        public void ByName_ValidArgs()
        {
            var name = "Cherry";
            var material = DSMaterial.ByName(name);
            Assert.NotNull(material);
            Assert.AreEqual(name, material.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSMaterial.ByName(null) );
        }

        [Test]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => DSMaterial.ByName("Mayo"));
        }
    }
}

