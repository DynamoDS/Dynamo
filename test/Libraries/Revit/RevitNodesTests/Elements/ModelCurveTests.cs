using System;

using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RTF.Framework;

namespace DSRevitNodesTests.Elements
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
            curve.Length.ShouldBeApproximately(Math.Sqrt(3) * (1 / 0.3042));
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
            curve.Length.ShouldBeApproximately(Math.Sqrt(3) * (1 / 0.3042));
        }
    }
}
