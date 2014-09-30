using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.Elements.Views;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace RevitTestServices
{
    [TestFixture]
    class SectionViewTests : GeometricRevitNodeTest
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByBoundingBox_ValidArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            Assert.NotNull(view);

            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID( view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByBoundingBox_NullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => SectionView.ByBoundingBox(null));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByCoordinateSystemMinPointMaxPoint_ValidArgs()
        {
            var cs = CoordinateSystem.Identity();
            var minPoint = Point.ByCoordinates(-2, -2, -2);
            var maxPoint = Point.ByCoordinates(2, 2, 2);

            var view = SectionView.ByCoordinateSystemMinPointMaxPoint(cs, minPoint, maxPoint);
            Assert.NotNull(view);

            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(view.InternalElement.UniqueId)));
        }
    }
}
