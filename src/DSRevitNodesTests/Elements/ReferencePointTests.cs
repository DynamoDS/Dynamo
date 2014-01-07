using System.Linq;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    internal class ReferencePointTests 
    {
        [Test]
        public void ByCoordinates_ValidInput()
        {
            var pt = DSReferencePoint.ByCoordinates(0, -10, 23.1);
            Assert.NotNull(pt);
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }

        [Test]
        public void ByPoint_ValidInput()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var pt = DSReferencePoint.ByPoint(p);
            Assert.NotNull(pt);
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }

        [Test]
        public void ByPointVectorDistance_ValidInput()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var v = Vector.ByCoordinates(1,0,0);
            var d = 5;
            var pt = DSReferencePoint.ByPointVectorDistance(p, v, d);
            Assert.NotNull(pt);
            Assert.AreEqual(5, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }

        [Test]
        public void ByLengthOnCurveReference_ValidInput()
        {

            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = DSModelCurve.ByPlanarCurve(l);
            var pt = DSReferencePoint.ByLengthOnCurveReference(modelCurve.CurveReference, 0.5);

            Assert.NotNull(pt);
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);
        }

        [Test]
        public void ByParameterOnCurveReference_ValidInput()
        {

            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = DSModelCurve.ByPlanarCurve(l);
            var pt = DSReferencePoint.ByParameterOnCurveReference(modelCurve.CurveReference, 0.5);

            Assert.NotNull(pt);
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);
        }

        [Test]
        public void ByParametersOnFaceReference_ValidInput()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as DSForm;
            var faceRef = form.FaceReferences.First();

            var pt = DSReferencePoint.ByParametersOnFaceReference(faceRef, 0.5, 0.5);

            Assert.NotNull(pt);
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);
        }

        [Test]
        public void ByPointVectorDistance_NullInput1()
        {
            var v = Vector.ByCoordinates(1, 0, 0);
            Assert.Throws(typeof(System.ArgumentNullException), () => DSReferencePoint.ByPointVectorDistance(null, v, 0));
        }

        [Test]
        public void ByPointVectorDistance_NullInput2()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            Assert.Throws(typeof(System.ArgumentNullException), () => DSReferencePoint.ByPointVectorDistance(p, null, 0.5));
        }

        [Test]
        public void ByParametersOnFaceReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => DSReferencePoint.ByParametersOnFaceReference(null, 0.5, 0.5));
        }

        [Test]
        public void ByParameterOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => DSReferencePoint.ByParameterOnCurveReference(null, 0.5));
        }

        [Test]
        public void ByLengthOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => DSReferencePoint.ByLengthOnCurveReference(null, 0.5));
        }
    }
}
