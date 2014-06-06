﻿using System;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitTestFramework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class FloorTypeTests : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var floorTypeName = "Generic - 12\"";
            var floorType = FloorType.ByName(floorTypeName);
            Assert.NotNull(floorType);
            Assert.AreEqual(floorTypeName, floorType.Name);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => FloorType.ByName(null));
        }
    }
}

