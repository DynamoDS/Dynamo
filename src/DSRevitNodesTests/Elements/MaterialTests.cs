using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
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
            var material = Material.ByName(name);
            Assert.NotNull(material);
            Assert.AreEqual(name, material.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Material.ByName(null) );
        }

        [Test]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => Material.ByName("Mayo"));
        }
    }
}

