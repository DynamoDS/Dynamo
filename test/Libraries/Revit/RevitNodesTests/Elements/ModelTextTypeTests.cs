using System;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelTextTypeTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var name = "24\" Arial";
            var modelTextType = ModelTextType.ByName(name);
            Assert.NotNull(modelTextType);
            Assert.AreEqual(name, modelTextType.Name);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                new ElementUUID( modelTextType.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => ModelTextType.ByName(null));
            Assert.Throws(typeof(Exception), () => ModelTextType.ByName("turtle type"));
        }

    }

}

