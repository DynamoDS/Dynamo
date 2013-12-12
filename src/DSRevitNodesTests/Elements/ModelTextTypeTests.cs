using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
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
            var modelTextType = DSModelTextType.ByName(name);
            Assert.NotNull(modelTextType);
            Assert.AreEqual(name, modelTextType.Name);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(modelTextType.InternalElement.Id));
        }

        [Test]
        public void ByName_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSModelTextType.ByName(null));
            Assert.Throws(typeof(Exception), () => DSModelTextType.ByName("turtle type"));
        }

    }

}

