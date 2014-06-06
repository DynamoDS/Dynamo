﻿using System;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitTestFramework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class FamilyTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByName_ValidInput()
        {
            var fam = Family.ByName("Cone");
            Assert.NotNull(fam);
            Assert.AreEqual("Cone",fam.Name);
            Assert.AreEqual(1, fam.Symbols.Length);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByName_NonexistentName()
        {
            Assert.Throws(typeof(Exception), () => Family.ByName("Turtle"));
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void ByName_NullInput()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Family.ByName(null));
        }
    }
}
