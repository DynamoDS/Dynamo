using System;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture, Category("UnitTests")]
    public sealed class SliceHSerializationTests
    {
        private const double Tolerance = 1e-5;

        [Test]
        public void WhenVectorCoordinateSystemAndBoundingBoxRoundTripJsonThenValuesMatch()
        {
            using var vec = Vector.ByCoordinates(3.5, -2.25, 7.125);
            var vecJson = vec.ToJson();
            using var vec2 = Vector.FromJson(vecJson);

            using var cs = CoordinateSystem.ByOriginVectors(
                Point.ByCoordinates(10, 20, 30),
                Vector.ByCoordinates(0, 1, 0),
                Vector.ByCoordinates(-1, 0, 0),
                Vector.ByCoordinates(0, 0, 2));
            var csJson = cs.ToJson();
            using var cs2 = CoordinateSystem.FromJson(csJson);

            using var bbox = BoundingBox.ByCorners(
                Point.ByCoordinates(-4, -5, -6),
                Point.ByCoordinates(7, 8, 9));
            var bboxJson = bbox.ToJson();
            using var bbox2 = BoundingBox.FromJson(bboxJson);

            Assert.Multiple(() =>
            {
                Assert.That(vecJson, Does.Contain("autodesk.math:vector3d-1.0.0"));
                Assert.That(vec2.X, Is.EqualTo(vec.X).Within(Tolerance));
                Assert.That(vec2.Y, Is.EqualTo(vec.Y).Within(Tolerance));
                Assert.That(vec2.Z, Is.EqualTo(vec.Z).Within(Tolerance));

                Assert.That(csJson, Does.Contain("autodesk.math:matrix44d-1.0.0"));
                Assert.That(cs2.Origin.DistanceTo(cs.Origin), Is.LessThan(Tolerance));
                Assert.That(cs2.XAxis.Subtract(cs.XAxis).Length, Is.LessThan(Tolerance));
                Assert.That(cs2.YAxis.Subtract(cs.YAxis).Length, Is.LessThan(Tolerance));
                Assert.That(cs2.ZAxis.Subtract(cs.ZAxis).Length, Is.LessThan(Tolerance));

                Assert.That(bboxJson, Does.Contain("autodesk.geometry:boundingbox3d-1.0.0"));
                Assert.That(bbox2.MinPoint.DistanceTo(bbox.MinPoint), Is.LessThan(Tolerance));
                Assert.That(bbox2.MaxPoint.DistanceTo(bbox.MaxPoint), Is.LessThan(Tolerance));
            });
        }

        [Test]
        public void WhenBooleanCutSolidRoundTripsJsonThenExactBrepVolumeAndTopologySurvive()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var cyl = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, -6), Point.ByCoordinates(0, 0, 6), 2);
            using var cut = box.Difference(cyl);
            var expectedVolume = 1000.0 - 40.0 * Math.PI;

            var json = cut.ToJson();
            using var restored = Geometry.FromJson(json) as Solid;

            Assert.That(restored, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("dynamo.geometry:solid-1.0.0"));
                Assert.That(restored.Volume, Is.EqualTo(expectedVolume).Within(expectedVolume * 1e-5));
                Assert.That(restored.Faces.Length, Is.EqualTo(cut.Faces.Length));
                Assert.That(restored.Edges.Length, Is.EqualTo(cut.Edges.Length));
            });

            // Further boolean operation on the deserialized BRep must succeed cleanly.
            using var halfCut = Cuboid.ByLengths(Point.ByCoordinates(5, 0, 0), 10, 20, 20);
            using var sliced = restored.Difference(halfCut);
            Assert.That(sliced.Volume, Is.EqualTo(expectedVolume * 0.5).Within(expectedVolume * 1e-4));
        }

        [Test]
        public void WhenPrimitivesSurfacesAndCurvesRoundTripJsonThenExactTypesAndMetricsSurvive()
        {
            using var pt = Point.ByCoordinates(1.5, -2.5, 3.5);
            using var pl = Plane.ByOriginNormalXAxis(Point.ByCoordinates(1, 2, 3), Vector.ZAxis(), Vector.YAxis());
            using var line = Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(3, 4, 0));
            using var circle = Circle.ByCenterPointRadiusNormal(Point.ByCoordinates(1, 2, 3), 5.0, Vector.ZAxis());
            using var arc = Arc.ByCenterPointRadiusAngle(Point.Origin(), 4.0, 15.0, 135.0, Vector.ZAxis());
            using var ellipse = Ellipse.ByOriginRadii(Point.ByCoordinates(2, 3, 0), 6.0, 3.0);
            using var ellipseArc = EllipseArc.ByPlaneRadiiAngles(Plane.XY(), 5.0, 2.5, 0.0, 180.0);
            using var helix = Helix.ByAxis(Point.Origin(), Vector.ZAxis(), Point.ByCoordinates(3, 0, 0), 4.0, 720.0);
            using var nurbsCurve = NurbsCurve.ByPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(2, 3, 1),
                Point.ByCoordinates(5, 1, 2),
                Point.ByCoordinates(8, 4, 0)
            });
            using var rect = Rectangle.ByWidthLength(6, 4);
            using var polyCurve = PolyCurve.ByPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(4, 0, 0),
                Point.ByCoordinates(4, 3, 0)
            }, connectLastToFirst: true);
            using var cuboid = Cuboid.ByLengths(4, 5, 6);
            using var sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(1, 2, 3), 3.0);
            using var cone = Cone.ByPointsRadii(Point.Origin(), Point.ByCoordinates(0, 0, 6), 3.0, 1.0);
            using var surf = rect.Patch();
            using var nurbsSurf = NurbsSurface.ByControlPoints(new[]
            {
                new[] { Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 5, 1) },
                new[] { Point.ByCoordinates(5, 0, 1), Point.ByCoordinates(5, 5, 0) }
            }, 1, 1);
            using var polySurf = PolySurface.BySolid(cuboid);
            using var panelSurf = PanelSurface.ByQuads(surf, 2, 3);
            using var mesh = Mesh.ByGeometry(cuboid);

            using var pt2 = Geometry.FromJson(pt.ToJson()) as Point;
            using var pl2 = Geometry.FromJson(pl.ToJson()) as Plane;
            using var line2 = Geometry.FromJson(line.ToJson()) as Line;
            using var circle2 = Geometry.FromJson(circle.ToJson()) as Circle;
            using var arc2 = Geometry.FromJson(arc.ToJson()) as Arc;
            using var ellipse2 = Geometry.FromJson(ellipse.ToJson()) as Ellipse;
            using var ellipseArc2 = Geometry.FromJson(ellipseArc.ToJson()) as EllipseArc;
            using var helix2 = Geometry.FromJson(helix.ToJson()) as Helix;
            using var nurbsCurve2 = Geometry.FromJson(nurbsCurve.ToJson()) as NurbsCurve;
            using var rect2 = Geometry.FromJson(rect.ToJson()) as Rectangle;
            using var polyCurve2 = Geometry.FromJson(polyCurve.ToJson()) as PolyCurve;
            using var cuboid2 = Geometry.FromJson(cuboid.ToJson()) as Cuboid;
            using var sphere2 = Geometry.FromJson(sphere.ToJson()) as Sphere;
            using var cone2 = Geometry.FromJson(cone.ToJson()) as Cone;
            using var surf2 = Geometry.FromJson(surf.ToJson()) as Surface;
            using var nurbsSurf2 = Geometry.FromJson(nurbsSurf.ToJson()) as NurbsSurface;
            using var polySurf2 = Geometry.FromJson(polySurf.ToJson()) as PolySurface;
            using var panelSurf2 = Geometry.FromJson(panelSurf.ToJson()) as PanelSurface;
            using var mesh2 = Mesh.FromJson(mesh.ToJson());

            Assert.Multiple(() =>
            {
                Assert.That(pt2, Is.Not.Null);
                Assert.That(pt2.DistanceTo(pt), Is.LessThan(Tolerance));

                Assert.That(pl2, Is.Not.Null);
                Assert.That(pl2.Origin.DistanceTo(pl.Origin), Is.LessThan(Tolerance));
                Assert.That(pl2.XAxis.Subtract(pl.XAxis).Length, Is.LessThan(Tolerance));

                Assert.That(line2, Is.Not.Null);
                Assert.That(line2.Length, Is.EqualTo(5.0).Within(Tolerance));

                Assert.That(circle2, Is.Not.Null);
                Assert.That(circle2.Radius, Is.EqualTo(5.0).Within(Tolerance));
                Assert.That(circle2.CenterPoint.DistanceTo(circle.CenterPoint), Is.LessThan(Tolerance));

                Assert.That(arc2, Is.Not.Null);
                Assert.That(arc2.Length, Is.EqualTo(arc.Length).Within(1e-3));
                Assert.That(arc2.StartPoint.DistanceTo(arc.StartPoint), Is.LessThan(1e-3));
                Assert.That(arc2.EndPoint.DistanceTo(arc.EndPoint), Is.LessThan(1e-3));

                Assert.That(ellipse2, Is.Not.Null);
                Assert.That(ellipse2.Length, Is.EqualTo(ellipse.Length).Within(1e-2));

                Assert.That(ellipseArc2, Is.Not.Null);
                Assert.That(ellipseArc2.Length, Is.EqualTo(ellipseArc.Length).Within(1e-2));

                Assert.That(helix2, Is.Not.Null);
                Assert.That(helix2.Length, Is.EqualTo(helix.Length).Within(1e-2));
                Assert.That(helix2.Pitch, Is.EqualTo(4.0).Within(Tolerance));

                Assert.That(nurbsCurve2, Is.Not.Null);
                Assert.That(nurbsCurve2.Degree, Is.EqualTo(nurbsCurve.Degree));
                Assert.That(nurbsCurve2.Length, Is.EqualTo(nurbsCurve.Length).Within(1e-3));
                Assert.That(nurbsCurve2.PointAtParameter(0.5).DistanceTo(nurbsCurve.PointAtParameter(0.5)), Is.LessThan(1e-3));

                Assert.That(rect2, Is.Not.Null);
                Assert.That(rect2.Width, Is.EqualTo(6.0).Within(Tolerance));
                Assert.That(rect2.Height, Is.EqualTo(4.0).Within(Tolerance));

                Assert.That(polyCurve2, Is.Not.Null);
                Assert.That(polyCurve2.NumberOfCurves, Is.EqualTo(3));
                Assert.That(polyCurve2.Length, Is.EqualTo(12.0).Within(Tolerance));

                Assert.That(cuboid2, Is.Not.Null);
                Assert.That(cuboid2.Volume, Is.EqualTo(120.0).Within(1e-3));

                Assert.That(sphere2, Is.Not.Null);
                Assert.That(sphere2.Volume, Is.EqualTo(sphere.Volume).Within(1e-2));

                Assert.That(cone2, Is.Not.Null);
                Assert.That(cone2.Volume, Is.EqualTo(cone.Volume).Within(1e-2));

                Assert.That(surf2, Is.Not.Null);
                Assert.That(surf2.Area, Is.EqualTo(24.0).Within(1e-3));

                Assert.That(nurbsSurf2, Is.Not.Null);
                Assert.That(nurbsSurf2.Area, Is.EqualTo(nurbsSurf.Area).Within(1e-3));

                Assert.That(polySurf2, Is.Not.Null);
                Assert.That(polySurf2.SurfaceCount, Is.EqualTo(6));

                Assert.That(panelSurf2, Is.Not.Null);
                Assert.That(panelSurf2.NumPanels, Is.EqualTo(6));

                Assert.That(mesh2, Is.Not.Null);
                Assert.That(mesh2.VertexCount, Is.EqualTo(mesh.VertexCount));
                Assert.That(mesh2.TriangleCount, Is.EqualTo(mesh.TriangleCount));
                Assert.That(mesh2.Volume, Is.EqualTo(120.0).Within(1e-2));
            });
        }

        [Test]
        public void WhenUnsupportedProprietaryJsonFormatsAreGivenThenExplicitNotSupportedExceptionIsThrown()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Geometry.FromSolidDef("{}"), Throws.TypeOf<NotSupportedException>());
                Assert.That(() => Geometry.FromJson("""{"$typeid":"dynamo.geometry:sab-1.0.0"}"""), Throws.TypeOf<NotSupportedException>());
                Assert.That(() => Geometry.FromJson("""{"$typeid":"dynamo.geometry:tsm-1.0.0"}"""), Throws.TypeOf<NotSupportedException>());
            });
        }
    }
}
