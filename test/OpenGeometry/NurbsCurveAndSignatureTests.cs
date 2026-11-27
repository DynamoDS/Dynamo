using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture, Category("UnitTests")]
    public sealed class NurbsCurveAndSignatureTests
    {
        [Test]
        public void WhenNurbsCurveQueriedThenControlPointsWeightsAndKnotsMatchDefinition()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(2, 4, 0),
                Point.ByCoordinates(5, 4, 1),
                Point.ByCoordinates(8, 0, 2)
            };

            using var cubic = NurbsCurve.ByControlPoints(pts);
            var cp = cubic.ControlPoints();
            var wts = cubic.Weights();
            var knots = cubic.Knots();

            Assert.That(cubic.Degree, Is.EqualTo(3));
            Assert.That(cp.Length, Is.EqualTo(4));
            Assert.That(wts.Length, Is.EqualTo(4));
            Assert.That(knots.Length, Is.EqualTo(cp.Length + cubic.Degree + 1));
            for (var i = 0; i < pts.Length; i++)
            {
                Assert.That(cp[i].DistanceTo(pts[i]), Is.LessThan(1e-6));
                Assert.That(wts[i], Is.EqualTo(1.0).Within(1e-6));
            }

            var rationalWeights = new[] { 1.0, 2.5, 0.75, 1.0 };
            var rationalKnots = new[] { 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0 };
            using var rational = NurbsCurve.ByControlPointsWeightsKnots(pts, rationalWeights, rationalKnots, 3);
            var rCp = rational.ControlPoints();
            var rWts = rational.Weights();
            var rKnots = rational.Knots();

            Assert.That(rCp.Length, Is.EqualTo(4));
            Assert.That(rKnots.Length, Is.EqualTo(8));
            for (var i = 0; i < pts.Length; i++)
            {
                Assert.That(rCp[i].DistanceTo(pts[i]), Is.LessThan(1e-6));
                Assert.That(rWts[i], Is.EqualTo(rationalWeights[i]).Within(1e-6));
            }

            using var reversed = rational.Reverse();
            using var mirrored = rational.Mirror(Plane.XZ());
            Assert.That(reversed, Is.InstanceOf<NurbsCurve>());
            Assert.That(mirrored, Is.InstanceOf<NurbsCurve>());
        }

        [Test]
        public void WhenArcCreatedByCenterStartSweepThenStartPointSweepNormalAndRoundTripHold()
        {
            var center = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var start = Point.ByCoordinates(4, 6, 3);
            using var arc = Arc.ByCenterPointStartPointSweepAngle(center, start, 80.0, normal);

            Assert.That(arc.Radius, Is.EqualTo(5.0).Within(1e-6));
            Assert.That(arc.SweepAngle, Is.EqualTo(80.0).Within(1e-6));
            Assert.That(arc.StartPoint.DistanceTo(start), Is.LessThan(1e-5));
            Assert.That(arc.Normal.IsAlmostEqualTo(normal), Is.True);

            using var negArc = Arc.ByCenterPointStartPointSweepAngle(center, start, -45.0, normal);
            Assert.That(negArc.StartPoint.DistanceTo(start), Is.LessThan(1e-5));
            Assert.That(negArc.Length, Is.EqualTo(5.0 * (45.0 * Math.PI / 180.0)).Within(1e-4));

            using var reversed = (Arc)arc.Reverse();
            Assert.That(reversed.StartPoint.DistanceTo(arc.EndPoint), Is.LessThan(1e-5));
            Assert.That(reversed.EndPoint.DistanceTo(arc.StartPoint), Is.LessThan(1e-5));

            var cs1 = CoordinateSystem.ByOrigin(0, 0);
            var cs2 = CoordinateSystem.ByOrigin(2, 3, 4)
                .Rotate(Point.ByCoordinates(1, 0, 0), Vector.ByCoordinates(0, 1, 1), 35.0)
                .Scale(2.0);
            using var transformed = (Arc)arc.Transform(cs1, cs2);
            using var mirrored = (Arc)arc.Mirror(Plane.XZ());
            Assert.That(transformed.Radius, Is.EqualTo(10.0).Within(1e-5));
            Assert.That(mirrored.Radius, Is.EqualTo(5.0).Within(1e-5));

            using var rt = (Arc)Geometry.FromJson(transformed.ToJson());
            Assert.That(rt.IsAlmostEqualTo(transformed), Is.True);
            Assert.That(rt.StartAngle, Is.EqualTo(transformed.StartAngle).Within(1e-6));
            Assert.That(rt.SweepAngle, Is.EqualTo(transformed.SweepAngle).Within(1e-6));
            Assert.That(rt.Normal.IsAlmostEqualTo(transformed.Normal), Is.True);
        }

        [Test]
        public void WhenBoundingBoxCreatedByMinimumVolumeThenContextCoordinateSystemIsOrientedAndToJsonThrows()
        {
            var pts = new Geometry[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(4, 3, 1),
                Point.ByCoordinates(2, 6, 3),
                Point.ByCoordinates(6, 5, 2)
            };

            var aabb = BoundingBox.ByGeometry(pts);
            Assert.That(aabb.ContextCoordinateSystem.IsEqualTo(CoordinateSystem.Identity()), Is.True);
            var rtAabb = BoundingBox.FromJson(aabb.ToJson());
            Assert.That(rtAabb.MinPoint.IsAlmostEqualTo(aabb.MinPoint), Is.True);
            Assert.That(rtAabb.MaxPoint.IsAlmostEqualTo(aabb.MaxPoint), Is.True);
            Assert.That(rtAabb.ContextCoordinateSystem.IsEqualTo(aabb.ContextCoordinateSystem), Is.True);

            var obb = BoundingBox.ByMinimumVolume(pts);
            Assert.That(obb.ContextCoordinateSystem.IsEqualTo(CoordinateSystem.Identity()), Is.False);
            Assert.That(obb.ContextCoordinateSystem.IsUnscaledOrtho, Is.True);
            Assert.That(() => obb.ToJson(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void WhenCoordinateSystemQueriesCalledThenScaleFactorsXYPlaneAndEqualityMatch()
        {
            var cs2d = CoordinateSystem.ByOrigin(3, 4);
            Assert.That(cs2d.Origin.IsAlmostEqualTo(Point.ByCoordinates(3, 4, 0)), Is.True);

            var scaled = CoordinateSystem.ByOrigin(1, 2, 3)
                .Rotate(Point.Origin(), Vector.ZAxis(), 30.0)
                .Scale(2.0, 3.0, 4.0);
            Assert.That(scaled.XScaleFactor, Is.EqualTo(2.0).Within(1e-6));
            Assert.That(scaled.YScaleFactor, Is.EqualTo(3.0).Within(1e-6));
            Assert.That(scaled.ZScaleFactor, Is.EqualTo(4.0).Within(1e-6));
            Assert.That(scaled.XYPlane.Origin.IsAlmostEqualTo(scaled.Origin), Is.True);
            Assert.That(scaled.XYPlane.Normal.IsAlmostEqualTo(scaled.ZAxis.Normalized()), Is.True);

            var rt = CoordinateSystem.FromJson(scaled.ToJson());
            Assert.That(rt.IsEqualTo(scaled), Is.True);
            Assert.That(rt.IsEqualTo(cs2d), Is.False);
        }

        [Test]
        public void WhenDerivedGeometriesTransformedAndRoundTrippedThenTypesAndEqualityHold()
        {
            var cs1 = CoordinateSystem.ByOrigin(0, 0);
            var cs2 = CoordinateSystem.ByOrigin(2, 3, 4)
                .Rotate(Point.ByCoordinates(1, 2, 3), Vector.ByCoordinates(1, 1, 0), 25.0)
                .Scale(1.5);

            using var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(3, 4, 5));
            using var circle = Circle.ByCenterPointRadiusNormal(Point.ByCoordinates(1, 2, 3), 2.5, Vector.ByCoordinates(1, 1, 1));
            using var ellipse = Ellipse.ByCoordinateSystemRadii(cs2, 2.4, 4.0);
            using var ellipseArc = EllipseArc.ByPlaneRadiiAngles(cs2.XYPlane, 2.4, 4.0, 35.0, 120.0);
            using var helix = Helix.ByAxis(Point.Origin(), Vector.ZAxis(), Point.ByCoordinates(2, 0, 0), 1.5, 540.0);
            using var poly = Polygon.RegularPolygon(Circle.ByCenterPointRadius(Point.Origin(), 3.0), 5);
            using var rect = Rectangle.ByWidthLength(Plane.XY(), 3.0, 4.0);
            using var polyCurve = PolyCurve.ByPoints(new[] { Point.Origin(), Point.ByCoordinates(2, 0, 0), Point.ByCoordinates(2, 3, 0) }, false);
            using var cyl = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 0, 5), 2.0);
            using var plane = Plane.ByOriginNormal(Point.ByCoordinates(1, 2, 3), Vector.ByCoordinates(1, 2, 3));

            Assert.That(polyCurve.Points.Length, Is.EqualTo(3));
            Assert.That(poly.Points.Length, Is.EqualTo(5));

            var samples = new Geometry[]
            {
                line.Transform(cs1, cs2),
                circle.Transform(cs1, cs2),
                ellipse.Transform(cs1, cs2),
                ellipseArc.Transform(cs1, cs2),
                helix.Transform(cs1, cs2),
                poly.Transform(cs1, cs2),
                rect.Transform(cs1, cs2),
                polyCurve.Transform(cs1, cs2),
                cyl.Transform(cs1, cs2),
                plane.Transform(cs1, cs2),
                ellipseArc.Mirror(Plane.XZ()),
                cyl.Mirror(Plane.XZ())
            };

            try
            {
                Assert.That(samples[0], Is.InstanceOf<Line>());
                Assert.That(samples[1], Is.InstanceOf<Circle>());
                Assert.That(samples[2], Is.InstanceOf<Ellipse>());
                Assert.That(samples[3], Is.InstanceOf<EllipseArc>());
                Assert.That(samples[4], Is.InstanceOf<Helix>());
                Assert.That(samples[5], Is.InstanceOf<Polygon>());
                Assert.That(samples[6], Is.InstanceOf<Rectangle>());
                Assert.That(samples[7], Is.InstanceOf<PolyCurve>());
                Assert.That(samples[8], Is.InstanceOf<Cylinder>());
                Assert.That(samples[9], Is.InstanceOf<Plane>());
                Assert.That(samples[10], Is.InstanceOf<EllipseArc>());
                Assert.That(samples[11], Is.InstanceOf<Cylinder>());

                foreach (var g in samples)
                {
                    using var rt = Geometry.FromJson(g.ToJson());
                    Assert.That(rt.GetType(), Is.EqualTo(g.GetType()), $"JSON round-trip type mismatch for {g.GetType().Name}");
                    Assert.That(rt.IsAlmostEqualTo(g), Is.True, $"JSON round-trip IsAlmostEqualTo failed for {g.GetType().Name}");
                }
            }
            finally
            {
                foreach (var g in samples)
                    g.Dispose();
            }
        }
    }
}
