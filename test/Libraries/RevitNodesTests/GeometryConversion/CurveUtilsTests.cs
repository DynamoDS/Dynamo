using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

using Arc = Autodesk.DesignScript.Geometry.Arc;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    class CurveUtilsTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesLine()
        {
            var line = Line.ByStartPointEndPoint(
                Point.Origin(),
                Point.ByCoordinates(12, 3, 2));

            var revitCurve = line.ToRevitType(false);

            Assert.True(CurveUtils.IsLineLike(revitCurve));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesStraightNurbsCurve()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, 0));

            var nurbsCurve = NurbsCurve.ByPoints(points, 3);
            var revitCurve = nurbsCurve.ToRevitType(false);

            Assert.True(CurveUtils.IsLineLike(revitCurve));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesNonStraightNurbsCurve()
        {
            var points = new[]
            {
                Point.ByCoordinates(5,5,0),
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(-5,5,0),
                Point.ByCoordinates(-10,5,0)
            };

            var nurbsCurve = NurbsCurve.ByPoints(points, 3);

            Assert.False(CurveUtils.IsLineLike(nurbsCurve.ToRevitType(false)));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesStraightHermiteSpline()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => new XYZ(x, 0, 0));

            var hs = HermiteSpline.Create(
                points.ToList(),
                false,
                new HermiteSplineTangents()
                {
                    StartTangent = new XYZ(1, 0, 0),
                    EndTangent = new XYZ(1, 0, 0)
                });

            Assert.True(CurveUtils.IsLineLike(hs));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesNonStraightHermiteWithStraightControlPoints()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => new XYZ(x, 0, 0));

            var hs = HermiteSpline.Create(
                points.ToList(),
                false,
                new HermiteSplineTangents()
                {
                    StartTangent = new XYZ(0, 0, 1),
                    EndTangent = new XYZ(1, 0, 0)
                });

            Assert.False(CurveUtils.IsLineLike(hs));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IsLineLike_Curve_CorrectlyIdentifiesArc()
        {
            var arc = Arc.ByThreePoints(
                Point.Origin(),
                Point.ByCoordinates(1, 1),
                Point.ByCoordinates(0, 1));

            Assert.False(CurveUtils.IsLineLike(arc.ToRevitType(false)));
        }

    }
}
