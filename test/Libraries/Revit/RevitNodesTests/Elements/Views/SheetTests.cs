using System;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.Elements;
using Revit.Elements.Views;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements.Views
{
    [TestFixture]
    class SheetTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByNameNumberTitleBlockAndViews_ValidArgs()
        {
            ElementBinder.IsEnabled = false;

            var famSymName = "E1 30x42 Horizontal";
            var famName = "E1 30 x 42 Horizontal";
            var titleBlock = FamilySymbol.ByFamilyAndName(Family.ByName(famName), famSymName);

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var pt2 = Point.ByCoordinates(100, 100, 0);
            var famInst2 = FamilyInstance.ByPoint(famSym, pt2);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            var view2 = SectionView.ByBoundingBox(famInst2.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            var ele = Sheet.ByNameNumberTitleBlockAndViews( sheetName, sheetNumber, titleBlock, new[] {view, view2});

            Assert.NotNull(ele);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByNameNumberTitleBlockAndViews_BadArgs()
        {

            ElementBinder.IsEnabled = false;

            var famSymName = "E1 30x42 Horizontal";
            var famName = "E1 30 x 42 Horizontal";
            var titleBlock = FamilySymbol.ByFamilyAndName(Family.ByName(famName), famSymName);

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var pt2 = Point.ByCoordinates(100, 100, 0);
            var famInst2 = FamilyInstance.ByPoint(famSym, pt2);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            var view2 = SectionView.ByBoundingBox(famInst2.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndViews(null, sheetNumber, titleBlock, new[] { view, view2 }));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndViews(sheetName, null, titleBlock, new[] { view, view2 }));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndViews(sheetName, sheetNumber, null, new[] { view, view2 }));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndViews(sheetName, sheetNumber, titleBlock, null));
        }
        
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByNameNumberTitleBlockAndView_ValidArgs()
        {
            ElementBinder.IsEnabled = false;

            var famSymName = "E1 30x42 Horizontal";
            var famName = "E1 30 x 42 Horizontal";
            var titleBlock = FamilySymbol.ByFamilyAndName(Family.ByName(famName), famSymName);

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            var ele = Sheet.ByNameNumberTitleBlockAndView(sheetName, sheetNumber, titleBlock, view);

            Assert.NotNull(ele);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByNameNumberTitleBlockAndView_BadArgs()
        {
            ElementBinder.IsEnabled = false;

            var famSymName = "E1 30x42 Horizontal";
            var famName = "E1 30 x 42 Horizontal";
            var titleBlock = FamilySymbol.ByFamilyAndName(Family.ByName(famName), famSymName);

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndView(null, sheetNumber, titleBlock, view));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndView(sheetName, null, titleBlock, view));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndView(sheetName, sheetNumber, null, view));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberTitleBlockAndView(sheetName, sheetNumber, titleBlock, null));
        }

    }
}