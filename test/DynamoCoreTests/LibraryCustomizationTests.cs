﻿using System.IO;

using Dynamo.Engine;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class LibraryCustomizationTests : UnitTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void CanLoadValidLibraryCustomization()
        {
            var fn = Path.Combine(TestDirectory, @"core/library/ProtoGeometry.dll");

            var c = LibraryCustomizationServices.GetForAssembly(fn, pathManager: null);
            Assert.NotNull(c);

            var cat = c.GetNamespaceCategory("Autodesk.DesignScript.Geometry");
            Assert.AreEqual("Geometry", cat);
        }
    }
}
