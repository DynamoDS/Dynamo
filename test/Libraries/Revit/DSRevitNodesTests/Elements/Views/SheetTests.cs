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
    class SheetTests
    {
        [Test]
        public void ByNameNumberAndViews_ValidArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var pt2 = Point.ByCoordinates(100, 100, 0);
            var famInst2 = FamilyInstance.ByPoint(famSym, pt2);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            var view2 = SectionView.ByBoundingBox(famInst2.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            var ele = Sheet.ByNameNumberAndViews( sheetName, sheetNumber, new[] {view, view2});

            Assert.NotNull(ele);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(ele.InternalElement.Id));
        }

        [Test]
        public void ByNameNumberAndViews_BadArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var pt2 = Point.ByCoordinates(100, 100, 0);
            var famInst2 = FamilyInstance.ByPoint(famSym, pt2);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);
            var view2 = SectionView.ByBoundingBox(famInst2.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndViews(null, sheetNumber, new[] { view, view2 }));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndViews(sheetName, null, new[] { view, view2 }));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndViews(sheetName, sheetNumber, null));
        }
        
        [Test]
        public void ByNameNumberAndView_ValidArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            var ele = Sheet.ByNameNumberAndView(sheetName, sheetNumber, view);

            Assert.NotNull(ele);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(ele.InternalElement.Id));
        }

        [Test]
        public void ByNameNumberAndView_BadArgs()
        {
            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = SectionView.ByBoundingBox(famInst.BoundingBox);

            var sheetName = "Poodle";
            var sheetNumber = "A1";

            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndView(null, sheetNumber, view));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndView(sheetName, null, view));
            Assert.Throws(typeof(ArgumentNullException), () => Sheet.ByNameNumberAndView(sheetName, sheetNumber, null));
        }

    }
}

/*     
        public static DSSheet ByNameNumberAndViews(string sheetName, string sheetNumber, AbstractView[] views)
        {
            if (sheetName == null)
            {
                throw new ArgumentNullException("sheetName");
            }

            if (sheetNumber == null)
            {
                throw new ArgumentNullException("sheetNumber");
            }

            return new DSSheet(sheetName, sheetNumber, views.Select(x => x.InternalView));
        }

        public static DSSheet ByNameNumberAndView(string sheetName, string sheetNumber, AbstractView view)
        {
            return DSSheet.ByNameNumberAndViews(sheetName, sheetNumber, new[] { view });
        }

        public static DSSheet ByNameNumberTitleBlockAndViews(string sheetName, string sheetNumber, DSFamilySymbol titleBlockFamilySymbol, AbstractView[] views)
        {
            if (sheetName == null)
            {
                throw new ArgumentNullException("sheetName");
            }

            if (sheetNumber == null)
            {
                throw new ArgumentNullException("sheetNumber");
            }

            if (titleBlockFamilySymbol == null)
            {
                throw new ArgumentNullException("titleBlockFamilySymbol");
            }

            return new DSSheet(sheetName, sheetNumber, titleBlockFamilySymbol.InternalFamilySymbol, views.Select(x => x.InternalView));
        }

        public static DSSheet ByNameNumberTitleBlockAndView(string sheetName, string sheetNumber, DSFamilySymbol titleBlockFamilySymbol, AbstractView view)
        {
            return DSSheet.ByNameNumberTitleBlockAndViews(sheetName, sheetNumber, titleBlockFamilySymbol, new[] { view });
        } */