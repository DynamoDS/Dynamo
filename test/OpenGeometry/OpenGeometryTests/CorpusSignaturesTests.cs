using System;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture]
    public sealed class CorpusSignaturesTests
    {
        [Test]
        public void WhenLineByBestFitThroughPointsRunsThenDirectionAndEndpointsMatchPrincipalAxis()
        {
            // Collinear points along (1, 1, 0) from (0,0,0) to (10,10,0) with symmetric tiny transverse noise.
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(2.5, 2.6, 0),
                Point.ByCoordinates(5.0, 4.9, 0),
                Point.ByCoordinates(7.5, 7.6, 0),
                Point.ByCoordinates(10.0, 9.9, 0)
            };

            using var line = Line.ByBestFitThroughPoints(pts);
            var expectedLen = Math.Sqrt(200.0);
            Assert.Multiple(() =>
            {
                Assert.That(line.Length, Is.EqualTo(expectedLen).Within(0.2));
                Assert.That(line.Direction.Normalized().Dot(Vector.ByCoordinates(1, 1, 0).Normalized()), Is.GreaterThan(0.999));
                Assert.That(line.StartPoint.DistanceTo(Point.Origin()), Is.LessThan(0.2));
                Assert.That(line.EndPoint.DistanceTo(Point.ByCoordinates(10, 10, 0)), Is.LessThan(0.2));
            });
        }

        [Test]
        public void WhenNurbsCurveByControlPointsClosedThenStartAndEndCoincide()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(4, 0, 0),
                Point.ByCoordinates(4, 4, 0),
                Point.ByCoordinates(0, 4, 0)
            };
            using var closed = NurbsCurve.ByControlPoints(pts, 3, closeCurve: true);
            Assert.Multiple(() =>
            {
                Assert.That(closed.IsClosed, Is.True);
                Assert.That(closed.Length, Is.GreaterThan(8.0));
            });
        }

        [Test]
        public void WhenSolidByLoftWithGuideCurveRunsThenValidSolidIsProduced()
        {
            using var bottom = Circle.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 2.0);
            using var top = Circle.ByCenterPointRadius(Point.ByCoordinates(0, 0, 10), 2.0);
            using var guide = NurbsCurve.ByPoints(new[]
            {
                Point.ByCoordinates(2, 0, 0),
                Point.ByCoordinates(3.5, 0, 5),
                Point.ByCoordinates(2, 0, 10)
            });

            using var solid = Solid.ByLoft(new Curve[] { bottom, top }, new Curve[] { guide });
            Assert.That(solid.Volume, Is.GreaterThan(100.0));
        }
    }
}
