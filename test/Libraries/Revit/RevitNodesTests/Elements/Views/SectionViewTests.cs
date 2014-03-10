using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revit;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryObjects;
using NUnit.Framework;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests
{
    [TestFixture]
    class SectionViewTests
    {
        [Test]
        public void ByBoundingBox_ValidArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            Assert.NotNull(view);

            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByBoundingBox_NullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => SectionView.ByBoundingBox(null));
        }
    }
}
