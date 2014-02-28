using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelTextTypeTests
    {
        [Test]
        public void ByName_ValidArgs()
        {
            var name = "24\" Arial";
            var modelTextType = ModelTextType.ByName(name);
            Assert.NotNull(modelTextType);
            Assert.AreEqual(name, modelTextType.Name);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(modelTextType.InternalElement.Id));
        }

        [Test]
        public void ByName_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => ModelTextType.ByName(null));
            Assert.Throws(typeof(Exception), () => ModelTextType.ByName("turtle type"));
        }

    }

}

