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
    class PerspectiveViewTests
    {
        [Test]
        public void ByEyePointTargetAndBoundingBox_ValidArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = PerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, famInst.BoundingBox, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByEyePointTargetAndBoundingBox_BadArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndBoundingBox(null, target, famInst.BoundingBox, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndBoundingBox(eye, null, famInst.BoundingBox, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, null, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, famInst.BoundingBox, null, false));
        }

        [Test]
        public void ByEyePointTargetAndElement_ValidArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var view = PerspectiveView.ByEyePointTargetAndElement(eye, target, famInst, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByEyePointTargetAndElement_BadArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndElement(null, target, famInst, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndElement(eye, null, famInst, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndElement(eye, target, null, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => PerspectiveView.ByEyePointTargetAndElement(eye, target, famInst, null, false));
        }
    }
}
