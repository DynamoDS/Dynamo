using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using Revit.Elements;
using NUnit.Framework;

using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

using Form = Revit.Elements.Form;
using Line = Autodesk.DesignScript.Geometry.Line;
using ModelCurve = Revit.Elements.ModelCurve;
using Point = Autodesk.DesignScript.Geometry.Point;
using ReferencePoint = Revit.Elements.ReferencePoint;

namespace RevitNodesTests
{
    [TestFixture]
    internal class ReferencePointTests : RevitNodeTestBase
    {

        internal XYZ InternalPosition(Revit.Elements.ReferencePoint point)
        {
            return (point.InternalElement as Autodesk.Revit.DB.ReferencePoint).Position;
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCoordinates_ShouldPlaceReferencePointCorrectly()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var rp = ReferencePoint.ByCoordinates(p.X, p.Y, p.Z);

            rp.Point.ShouldBeApproximately(p);

            InternalPosition(rp).ShouldBeApproximately(p.InHostUnits());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByPoint_ShouldPlaceReferencePointCorrectly()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var rp = ReferencePoint.ByPoint(p);

            rp.Point.ShouldBeApproximately(p);

            InternalPosition(rp).ShouldBeApproximately(p.InHostUnits());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByPointVectorDistance_ShouldPlaceReferencePointCorrectly()
        {
            var p = Point.ByCoordinates(0, -10, 23.1);
            var v = Vector.ByCoordinates(1,0,0);
            var d = 5;
            var rp = ReferencePoint.ByPointVectorDistance(p, v, d);

            var pt = p.Add(v.Scale(5));

            rp.Point.ShouldBeApproximately(pt);
            InternalPosition(rp).ShouldBeApproximately(pt.InHostUnits());
        }

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void ByLengthOnCurveReference_ShouldPlaceReferencePointCorrectly()
        {
            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = ModelCurve.ByCurve(l);
            var rp = ReferencePoint.ByLengthOnCurveReference(modelCurve.ElementCurveReference, 0.5);

            var pt = Point.ByCoordinates(0.5, 0, 0);

            rp.Point.ShouldBeApproximately(pt);
            InternalPosition(rp).ShouldBeApproximately(pt.InHostUnits());
        }

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void ByParameterOnCurveReference_ShouldPlaceReferencePointCorrectly()
        {
            var l = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            var modelCurve = ModelCurve.ByCurve(l);
            var rp = ReferencePoint.ByParameterOnCurveReference(modelCurve.ElementCurveReference, 0.5);

            var pt = Point.ByCoordinates(0.5, 0, 0);

            rp.Point.ShouldBeApproximately(pt);
            InternalPosition(rp).ShouldBeApproximately(pt.InHostUnits());
        }

        [Test, Category("Failure")]
        [TestModel(@".\block.rfa")]
        public void ByParametersOnFaceReference_ShouldPlaceReferencePointCorrectly()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var faceRef = form.ElementFaceReferences.First();

            var pt = ReferencePoint.ByParametersOnFaceReference(faceRef, 0.5, 0.5);

            Assert.NotNull(pt);
            pt.X.ShouldBeApproximately(-18.19622727891606);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByPointVectorDistance_NullInput1()
        {
            var v = Vector.ByCoordinates(1, 0, 0);
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByPointVectorDistance(null, v, 0));
            var p = Point.ByCoordinates(0, -10, 23.1);
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByPointVectorDistance(p, null, 0.5));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByParametersOnFaceReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByParametersOnFaceReference(null, 0.5, 0.5));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByParameterOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByParameterOnCurveReference(null, 0.5));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLengthOnCurveReference_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePoint.ByLengthOnCurveReference(null, 0.5));
        }
    }
}
