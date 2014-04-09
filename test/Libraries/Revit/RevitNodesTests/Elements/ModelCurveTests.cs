using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelCurveTests : RevitNodeTestBase
    {
        [Test]
        public void ByCurve_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 1, 1));
            Assert.NotNull(line);

            var modelCurve = ModelCurve.ByCurve(line);
            Assert.NotNull(line);

            var curveRef = modelCurve.CurveReference;
            Assert.NotNull(curveRef);
        }
    }
}
