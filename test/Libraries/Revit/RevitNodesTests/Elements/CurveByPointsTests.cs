using System.Collections.Generic;
using NUnit.Framework;
using Revit.Elements;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class CurveByPointsTests
    {
        [Test]
        public void ByReferencePoints_ValidArgs()
        {
            ElementBinder.IsEnabled = false;

            var p1 = ReferencePoint.ByCoordinates(0, 0, 0);
            var p2 = ReferencePoint.ByCoordinates(1, 1, 1);
            var p3 = ReferencePoint.ByCoordinates(2, 2, 2);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);

            var curveByPoints = CurveByPoints.ByReferencePoints(new List<ReferencePoint>{p1,p2,p3});
            Assert.NotNull(curveByPoints);

            var curveRef = curveByPoints.CurveReference;
            Assert.NotNull(curveRef);
        }
    }
}
