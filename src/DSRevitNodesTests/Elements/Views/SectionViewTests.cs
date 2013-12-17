using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
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
            var famSym = DSFamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);

            var view = DSSectionView.ByBoundingBox(famInst.BoundingBox);
            Assert.NotNull(view);

            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByBoundingBox_NullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSSectionView.ByBoundingBox(null));
        }
    }
}
