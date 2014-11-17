using System;
using System.Linq;

using Autodesk.DesignScript.Geometry;

using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ModelCurveTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCurve_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 1, 1));
            Assert.NotNull(line);

            var modelCurve = ModelCurve.ByCurve(line);
            Assert.NotNull(line);

            var curveRef = modelCurve.ElementCurveReference;
            Assert.NotNull(curveRef);

            var curve = modelCurve.Curve;

            curve.Length.ShouldBeApproximately(Math.Sqrt(3.0));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByCurve_Curve_AcceptsStraightDegree3NurbsCurve()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, 0));

            var nurbsCurve = NurbsCurve.ByPoints(points, 3);

            var modelCurve = ModelCurve.ByCurve(nurbsCurve);
            Assert.NotNull(nurbsCurve);

            modelCurve.Curve.Length.ShouldBeApproximately(9);
            modelCurve.Curve.StartPoint.ShouldBeApproximately(Point.Origin());
            modelCurve.Curve.EndPoint.ShouldBeApproximately(Point.ByCoordinates(9,0,0));

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ReferenceCurveByCurve_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 1, 1));
            Assert.NotNull(line);

            var modelCurve = ModelCurve.ReferenceCurveByCurve(line);
            Assert.NotNull(line);

            var curveRef = modelCurve.ElementCurveReference;
            Assert.NotNull(curveRef);

            var curve = modelCurve.Curve;

            curve.Length.ShouldBeApproximately(Math.Sqrt(3.0));
        }
    }
}
