using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements.Views;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class DraftingViewTests : GeometricRevitNodeTest
    {
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
