using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements.Views;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitTestFramework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class DraftingViewTests : RevitNodeTestBase
    {
        [SetUp]
        public override void Setup()
        {
            HostFactory.Instance.StartUp();
            base.Setup();
        }

        [TearDown]
        public override void TearDown()
        {
            HostFactory.Instance.ShutDown();
            base.TearDown();
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var view = DraftingView.ByName("poodle");
            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
               new ElementUUID(view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_NullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => SectionView.ByBoundingBox(null));
        }
    }
}
