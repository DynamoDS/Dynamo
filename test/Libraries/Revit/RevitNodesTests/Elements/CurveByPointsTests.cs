using System.Collections.Generic;
using NUnit.Framework;
using Revit.Elements;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class CurveByPointsTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByReferencePoints_ValidArgs()
        {
            ElementBinder.IsEnabled = false;

            var p1 = ReferencePoint.ByCoordinates(0, 0, 0);
            var p2 = ReferencePoint.ByCoordinates(1, 1, 1);
            var p3 = ReferencePoint.ByCoordinates(2, 2, 2);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);

            var curveByPoints = CurveByPoints.ByReferencePoints(new List<ReferencePoint>{p1,p2,p3}.ToArray());
            Assert.NotNull(curveByPoints);

            var curveRef = curveByPoints.ElementCurveReference;
            Assert.NotNull(curveRef);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByReferencePoints_DuplicatePoints()
        {
            ElementBinder.IsEnabled = false;

            var p1 = ReferencePoint.ByCoordinates(0, 0, 0);
            var p2 = ReferencePoint.ByCoordinates(1, 1, 1);

            Assert.NotNull(p1);
            Assert.NotNull(p2);

            //ensure that the call to create a curve by points with 
            //duplicate points is handled and a system exception is thrown
            Assert.Throws<Autodesk.Revit.Exceptions.ArgumentException>(
                () => CurveByPoints.ByReferencePoints(new List<ReferencePoint> {p1, p2, p2}.ToArray()));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByReferencePoints_Mutation()
        {
            ElementBinder.IsEnabled = false;

            var p1 = ReferencePoint.ByCoordinates(0, 0, 0);
            var p2 = ReferencePoint.ByCoordinates(1, 1, 1);
            var p3 = ReferencePoint.ByCoordinates(2, 2, 2);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);

            ElementBinder.IsEnabled = true;

            var curveByPoints = CurveByPoints.ByReferencePoints(new List<ReferencePoint> { p1, p2, p3 }.ToArray());
            Assert.NotNull(curveByPoints);

            var curveRef = curveByPoints.ElementCurveReference;
            Assert.NotNull(curveRef);

            var p4 = ReferencePoint.ByCoordinates(3, 3, 3);
            curveByPoints = CurveByPoints.ByReferencePoints(new List<ReferencePoint> { p1, p2, p4 }.ToArray());
            Assert.NotNull(curveByPoints);
        }
    }
}
