using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    internal class ReferencePointTests 
    {
        [Test]
        public void ByCoordinates_ValidInput()
        {
            var pt = ReferencePoint.ByCoordinates(0, -10, 23.1);
            Assert.NotNull(pt);
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }

        [Test]
        public void ByPoint_ValidInput()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var pt = ReferencePoint.ByPoint(p);
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
            var pt = ReferencePoint.ByPointVectorDistance(p, v, d);
            Assert.NotNull(pt);
            Assert.AreEqual(5, pt.X);
            Assert.AreEqual(-10, pt.Y);
            Assert.AreEqual(23.1, pt.Z);
        }

        [Test]
        public void ByLengthOnCurveReference_ValidInput()
        {

            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = ModelCurve.ByCurve(l);
            var pt = ReferencePoint.ByLengthOnCurveReference(modelCurve.CurveReference, 0.5);

            Assert.NotNull(pt);
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);
        }

        [Test]
        public void ByParameterOnCurveReference_ValidInput()
        {

            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = ModelCurve.ByCurve(l);
            var pt = ReferencePoint.ByParameterOnCurveReference(modelCurve.CurveReference, 0.5);

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

            var form = ele as Form;
            var faceRef = form.FaceReferences.First();

            var pt = ReferencePoint.ByParametersOnFaceReference(faceRef, 0.5, 0.5);

            Assert.NotNull(pt);
            pt.X.ShouldBeApproximately(-18.19622727891606);
        }

        [Test]
        public void ByPointVectorDistance_NullInput1()
        {
            var v = Vector.ByCoordinates(1, 0, 0);
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByPointVectorDistance(null, v, 0));
        }

        [Test]
        public void ByPointVectorDistance_NullInput2()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByPointVectorDistance(p, null, 0.5));
        }

        [Test]
        public void ByParametersOnFaceReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByParametersOnFaceReference(null, 0.5, 0.5));
        }

        [Test]
        public void ByParameterOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByParameterOnCurveReference(null, 0.5));
        }

        [Test]
        public void ByLengthOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByLengthOnCurveReference(null, 0.5));
        }
    }
}
