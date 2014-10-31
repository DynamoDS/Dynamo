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
    class PerspectiveViewTests : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByEyePointAndTarget_ValidBoundingBox()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            object element = famInst.BoundingBox;
            var view = PerspectiveView.ByEyePointAndTarget(eye, target, element, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByEyePointAndTarget_ValidAbstractElement()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            var famSym = FamilySymbol.ByName("Kousa Dogwood - 10'");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            object element = famInst;
            var view = PerspectiveView.ByEyePointAndTarget(eye, target, element, name, false);

            Assert.NotNull(view);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByEyePointAndTarget_BadArgs0()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                PerspectiveView.ByEyePointAndTarget(eye, target, null, name, false);
            });
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByEyePointAndTarget_BadArgs1()
        {
            var eye = Point.ByCoordinates(100, 100, 100);
            var target = Point.ByCoordinates(0, 1, 2);
            var name = "treeView";

            Assert.Throws(typeof(ArgumentException), () =>
            {
                PerspectiveView.ByEyePointAndTarget(eye, target, eye, name, false);
            });
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
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
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
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
        [TestModel(@".\Empty.rvt")]
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
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID(view.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
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
