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
    class PerspectiveViewTests
    {
        [Test]
        public void ByEyePointTargetAndBoundingBox_ValidArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = DSFamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);

            var view = DSPerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, famInst.BoundingBox, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByEyePointTargetAndBoundingBox_BadArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = DSFamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);

            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndBoundingBox(null, target, famInst.BoundingBox, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndBoundingBox(eye, null, famInst.BoundingBox, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, null, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndBoundingBox(eye, target, famInst.BoundingBox, null, false));
        }

        [Test]
        public void ByEyePointTargetAndElement_ValidArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = DSFamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);

            var view = DSPerspectiveView.ByEyePointTargetAndElement(eye, target, famInst, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(view.InternalElement.Id));
        }

        [Test]
        public void ByEyePointTargetAndElement_BadArgs()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = DSFamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = DSFamilyInstance.ByPoint(famSym, pt);

            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndElement(null, target, famInst, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndElement(eye, null, famInst, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndElement(eye, target, null, name, false));
            Assert.Throws(typeof(ArgumentNullException), () => DSPerspectiveView.ByEyePointTargetAndElement(eye, target, famInst, null, false));
        }
    }
}
