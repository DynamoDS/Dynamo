using System;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ModelTextTypeTests : RevitNodeTestBase
    {
        [Test, Category("Failure")]
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

