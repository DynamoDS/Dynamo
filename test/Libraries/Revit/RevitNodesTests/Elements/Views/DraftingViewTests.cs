using System;

using NUnit.Framework;

using Revit.Elements.Views;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements.Views
{
    [TestFixture]
    class DraftingViewTests : RevitNodeTestBase
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
