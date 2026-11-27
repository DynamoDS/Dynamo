using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture]
    public sealed class SliceDCurvesSurfacesIntersectionsTests
    {
        [Test]
        public void WhenEllipseAndEllipseArcCreatedThenDimensionsAndEndpointsMatchAnalyticValues()
        {
            const double a = 5.0;
            const double b = 3.0;
            using var ellipse = Ellipse.ByOriginRadii(Point.ByCoordinates(1, 2, 3), a, b);

            // Ramanujan approximation for ellipse perimeter: pi*(3(a+b) - sqrt((3a+b)(a+3b))).
            var expectedPerimeter = Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
            Assert.Multiple(() =>
            {
                Assert.That(ellipse.IsClosed, Is.True);
                Assert.That(ellipse.Length, Is.EqualTo(expectedPerimeter).Within(1e-2));
                Assert.That(ellipse.MajorAxis.Length, Is.EqualTo(a).Within(1e-9));
                Assert.That(ellipse.MinorAxis.Length, Is.EqualTo(b).Within(1e-9));
                Assert.That(ellipse.CenterPoint.X, Is.EqualTo(1.0).Within(1e-9));
            });

            using var arc = EllipseArc.ByPlaneRadiiAngles(Plane.XY(), a, b, 0.0, 90.0);
            Assert.Multiple(() =>
            {
                Assert.That(arc.IsClosed, Is.False);
                Assert.That(arc.StartPoint.X, Is.EqualTo(a).Within(1e-5));
                Assert.That(arc.StartPoint.Y, Is.EqualTo(0.0).Within(1e-5));
                Assert.That(arc.EndPoint.X, Is.EqualTo(0.0).Within(1e-5));
                Assert.That(arc.EndPoint.Y, Is.EqualTo(b).Within(1e-5));
                Assert.That(arc.Length, Is.EqualTo(expectedPerimeter / 4.0).Within(1e-2));
            });
        }

        [Test]
        public void WhenHelixCreatedByAxisThenLengthAndHeightMatchExactFormula()
        {
            const double radius = 3.0;
            const double pitch = 4.0;
            const double turns = 2.0;
            using var helix = Helix.ByAxis(
                Point.Origin(), Vector.ZAxis(), Point.ByCoordinates(radius, 0, 0), pitch, turns * 360.0);

            var expectedLength = turns * Math.Sqrt(Math.Pow(2.0 * Math.PI * radius, 2) + pitch * pitch);
            Assert.Multiple(() =>
            {
                Assert.That(helix.Radius, Is.EqualTo(radius).Within(1e-9));
                Assert.That(helix.Pitch, Is.EqualTo(pitch).Within(1e-9));
                Assert.That(helix.Angle, Is.EqualTo(720.0).Within(1e-9));
                Assert.That(helix.StartPoint.X, Is.EqualTo(radius).Within(1e-4));
                Assert.That(helix.StartPoint.Z, Is.EqualTo(0.0).Within(1e-4));
                Assert.That(helix.EndPoint.X, Is.EqualTo(radius).Within(1e-3));
                Assert.That(helix.EndPoint.Y, Is.EqualTo(0.0).Within(1e-3));
                Assert.That(helix.EndPoint.Z, Is.EqualTo(pitch * turns).Within(1e-3));
                Assert.That(helix.Length, Is.EqualTo(expectedLength).Within(1e-2));
            });
        }

        [Test]
        public void WhenCurveOffsetAndProjectedThenResultingGeometryMatchesExpectedLocation()
        {
            using var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 0, 0));
            using var offset = line.Offset(2.5);
            Assert.Multiple(() =>
            {
                Assert.That(offset.Length, Is.EqualTo(10.0).Within(1e-4));
                Assert.That(Math.Abs(offset.StartPoint.Y), Is.EqualTo(2.5).Within(1e-4));
                Assert.That(Math.Abs(offset.EndPoint.Y), Is.EqualTo(2.5).Within(1e-4));
            });

            using var elevated = Line.ByStartPointEndPoint(Point.ByCoordinates(1, 2, 5), Point.ByCoordinates(9, 2, 8));
            using var planeSheet = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 10, 0),
                Point.ByCoordinates(0, 10, 0)
            });

            var projected = elevated.Project(planeSheet, Vector.ByCoordinates(0, 0, -1));
            try
            {
                Assert.That(projected.Length, Is.EqualTo(1));
                var curve = (Curve)projected[0];
                Assert.That(curve.Length, Is.EqualTo(8.0).Within(1e-4));
                Assert.That(curve.StartPoint.Z, Is.EqualTo(0.0).Within(1e-4));
                Assert.That(curve.EndPoint.Z, Is.EqualTo(0.0).Within(1e-4));
            }
            finally
            {
                foreach (var g in projected) g.Dispose();
            }
        }

        [Test]
        public void WhenNurbsSurfaceCreatedByPointsAndControlPointsThenSurfaceInterpolatesAndReportsMetadata()
        {
            var grid = new Point[4][];
            for (var u = 0; u < 4; u++)
            {
                grid[u] = new Point[4];
                for (var v = 0; v < 4; v++)
                {
                    var x = u * 2.0;
                    var y = v * 2.0;
                    var z = Math.Sin(u * 0.5) + Math.Cos(v * 0.5);
                    grid[u][v] = Point.ByCoordinates(x, y, z);
                }
            }

            using var interp = NurbsSurface.ByPoints(grid, 3, 3);
            using var ctrl = NurbsSurface.ByControlPoints(grid, 3, 3);

            var midPt = interp.PointAtParameter(0.5, 0.5);
            Assert.Multiple(() =>
            {
                Assert.That(interp.DegreeU, Is.EqualTo(3));
                Assert.That(interp.DegreeV, Is.EqualTo(3));
                Assert.That(interp.NumControlPointsU, Is.GreaterThanOrEqualTo(4));
                Assert.That(interp.NumControlPointsV, Is.GreaterThanOrEqualTo(4));
                Assert.That(interp.Area, Is.GreaterThan(36.0));
                Assert.That(interp.PointAtParameter(0, 0).DistanceTo(grid[0][0]), Is.LessThan(1e-4));
                Assert.That(interp.PointAtParameter(1, 1).DistanceTo(grid[3][3]), Is.LessThan(1e-4));
                Assert.That(midPt.X, Is.EqualTo(3.0).Within(0.2));

                Assert.That(ctrl.DegreeU, Is.EqualTo(3));
                Assert.That(ctrl.DegreeV, Is.EqualTo(3));
                Assert.That(ctrl.NumControlPointsU, Is.EqualTo(4));
                Assert.That(ctrl.NumControlPointsV, Is.EqualTo(4));
                Assert.That(ctrl.ControlPoints().Length, Is.EqualTo(4));
                Assert.That(ctrl.ControlPoints()[0].Length, Is.EqualTo(4));
                Assert.That(ctrl.UKnots().Length, Is.EqualTo(8));
                Assert.That(ctrl.VKnots().Length, Is.EqualTo(8));
                Assert.That(ctrl.Weights()[0][0], Is.EqualTo(1.0).Within(1e-9));
            });
        }

        [Test]
        public void WhenSurfaceSweptRevolvedAndTrimmedThenAreasMatchAnalyticValues()
        {
            // Surface.BySweep: length-4 line swept along length-10 perpendicular line -> area 40.
            using var profile = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(4, 0, 0));
            using var path = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 10, 0));
            using var swept = Surface.BySweep(profile, path);
            Assert.That(swept.Area, Is.EqualTo(40.0).Within(1e-2));

            // Surface.BySweep2Rails: two rails diverging from width 4 to width 8 over length 10 -> trapezoid area 60.
            using var rail1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 10, 0));
            using var rail2 = Line.ByStartPointEndPoint(Point.ByCoordinates(4, 0, 0), Point.ByCoordinates(8, 10, 0));
            using var swept2 = Surface.BySweep2Rails(rail1, rail2, profile);
            Assert.That(swept2.Area, Is.EqualTo(60.0).Within(1.0));

            // Surface.ByRevolve: vertical line at x=3, z in [0, 5] revolved 360 around Z -> cylinder lateral area 2*pi*3*5 = 30*pi.
            using var gen = Line.ByStartPointEndPoint(Point.ByCoordinates(3, 0, 0), Point.ByCoordinates(3, 0, 5));
            using var rev = Surface.ByRevolve(gen, Point.Origin(), Vector.ZAxis(), 0.0, 360.0);
            Assert.That(rev.Area, Is.EqualTo(30.0 * Math.PI).Within(1e-1));

            // Surface.TrimWithEdgeLoops: 10x10 square trimmed with 4x4 inner square loop -> trimmed area 16.
            using var baseSheet = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 10, 0),
                Point.ByCoordinates(0, 10, 0)
            });
            using var trimLoop = Rectangle.ByCornerPoints(
                Point.ByCoordinates(2, 2, 0),
                Point.ByCoordinates(6, 2, 0),
                Point.ByCoordinates(6, 6, 0),
                Point.ByCoordinates(2, 6, 0));
            using var trimmed = baseSheet.TrimWithEdgeLoops(new PolyCurve[] { trimLoop });
            Assert.That(trimmed.Area, Is.EqualTo(16.0).Within(1e-2));
        }

        [Test]
        public void WhenGeometriesIntersectAndTrimThenResultsAreExact()
        {
            // Curve / Curve intersection
            using var l1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 10, 0));
            using var l2 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 10, 0), Point.ByCoordinates(10, 0, 0));
            using var l3 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 5), Point.ByCoordinates(10, 0, 5));

            var hits = l1.Intersect(l2);
            try
            {
                Assert.That(hits.Length, Is.EqualTo(1));
                var hit = (Point)hits[0];
                Assert.That(hit.X, Is.EqualTo(5.0).Within(1e-4));
                Assert.That(hit.Y, Is.EqualTo(5.0).Within(1e-4));
                Assert.That(l1.DoesIntersect(l2), Is.True);
                Assert.That(l1.DoesIntersect(l3), Is.False);
            }
            finally
            {
                foreach (var h in hits) h.Dispose();
            }

            // Solid / Plane section intersection & Geometry.Trim
            using var box = Cuboid.ByLengths(Point.Origin(), 10, 10, 10);
            using var cutPlane = Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 2), Vector.ZAxis());
            var section = ((Geometry)box).Intersect(cutPlane);
            try
            {
                Assert.That(section.Length, Is.GreaterThanOrEqualTo(1));
                var totalLength = section.OfType<Curve>().Sum(c => c.Length);
                Assert.That(totalLength, Is.EqualTo(40.0).Within(1e-2));
            }
            finally
            {
                foreach (var s in section) s.Dispose();
            }

            // Geometry.Trim removes the side closest to pick=(0,0,5) -> keeps z in [-5, 2], volume = 10*10*7 = 700.
            var trimmed = box.Trim(cutPlane, Point.ByCoordinates(0, 0, 5));
            try
            {
                Assert.That(trimmed.Length, Is.EqualTo(1));
                Assert.That(((Solid)trimmed[0]).Volume, Is.EqualTo(700.0).Within(1e-2));
            }
            finally
            {
                foreach (var t in trimmed) t.Dispose();
            }
        }
    }
}
