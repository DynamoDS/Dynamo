using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture, Category("UnitTests")]
    public sealed class PrimitivesAndTransformsTests
    {
        private const double Tol = 1e-5;

        [Test]
        public void WhenSphereIsCreatedThenVolumeAreaCentroidAndBoundsMatchAnalytic()
        {
            const double r = 3.0;
            using var center = Point.ByCoordinates(1, -2, 4);
            using var sphere = Sphere.ByCenterPointRadius(center, r);
            using var centroid = sphere.Centroid();
            using var bbox = sphere.BoundingBox;

            double expectedVolume = (4.0 / 3.0) * Math.PI * r * r * r;
            double expectedArea = 4.0 * Math.PI * r * r;

            Assert.Multiple(() =>
            {
                Assert.That(sphere.Volume, Is.EqualTo(expectedVolume).Within(expectedVolume * Tol));
                Assert.That(sphere.Area, Is.EqualTo(expectedArea).Within(expectedArea * Tol));
                Assert.That(centroid.X, Is.EqualTo(1.0).Within(Tol));
                Assert.That(centroid.Y, Is.EqualTo(-2.0).Within(Tol));
                Assert.That(centroid.Z, Is.EqualTo(4.0).Within(Tol));
                Assert.That(bbox.MinPoint.X, Is.EqualTo(-2.0).Within(1e-3));
                Assert.That(bbox.MaxPoint.X, Is.EqualTo(4.0).Within(1e-3));
                Assert.That(bbox.MinPoint.Z, Is.EqualTo(1.0).Within(1e-3));
                Assert.That(bbox.MaxPoint.Z, Is.EqualTo(7.0).Within(1e-3));
            });
        }

        [Test]
        public void WhenConeAndFrustumAreCreatedThenVolumesMatchAnalytic()
        {
            using var start = Point.ByCoordinates(0, 0, 0);
            using var end = Point.ByCoordinates(0, 0, 6);
            using var pointed = Cone.ByPointsRadius(start, end, 3.0);
            using var frustum = Cone.ByPointsRadii(start, end, 3.0, 1.5);
            using var inverted = Cone.ByPointsRadii(start, end, 0.0, 3.0);

            double expectedPointed = Math.PI * 3.0 * 3.0 * 6.0 / 3.0;
            double expectedFrustum = (Math.PI * 6.0 / 3.0) * (9.0 + 4.5 + 2.25);

            Assert.Multiple(() =>
            {
                Assert.That(pointed.Volume, Is.EqualTo(expectedPointed).Within(expectedPointed * Tol));
                Assert.That(frustum.Volume, Is.EqualTo(expectedFrustum).Within(expectedFrustum * Tol));
                Assert.That(inverted.Volume, Is.EqualTo(expectedPointed).Within(expectedPointed * Tol));
            });
        }

        [Test]
        public void WhenSolidsUnionAndIntersectThenAnalyticVolumesAndInputImmutabilityHold()
        {
            using var a = Cuboid.ByCorners(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(4, 4, 4));
            using var b = Cuboid.ByCorners(Point.ByCoordinates(2, 0, 0), Point.ByCoordinates(6, 4, 4));

            using var union = a.Union(b);
            using var inter = a.Intersect(b);

            Assert.Multiple(() =>
            {
                Assert.That(union.Volume, Is.EqualTo(96.0).Within(96.0 * Tol));
                Assert.That(inter.Volume, Is.EqualTo(32.0).Within(32.0 * Tol));
                Assert.That(a.Volume, Is.EqualTo(64.0).Within(64.0 * Tol));
                Assert.That(b.Volume, Is.EqualTo(64.0).Within(64.0 * Tol));
            });
        }

        [Test]
        public void WhenSolidIsTranslatedRotatedScaledAndTransformedThenOriginalsRemainUnchanged()
        {
            using var box = Cuboid.ByLengths(2, 4, 6);
            using var moved = (Solid)box.Translate(10, -5, 3);
            using var rotated = (Solid)box.Rotate(Point.Origin(), Vector.ZAxis(), 90);
            using var scaled = (Solid)box.Scale(2, 3, 0.5);
            using var cs = CoordinateSystem.ByOriginVectors(
                Point.ByCoordinates(5, 0, 0),
                Vector.ByCoordinates(0, 1, 0),
                Vector.ByCoordinates(-1, 0, 0));
            using var transformed = (Solid)box.Transform(cs);

            using var movedCenter = moved.Centroid();
            using var origCenter = box.Centroid();
            using var rotBounds = rotated.BoundingBox;
            using var xformCenter = transformed.Centroid();

            Assert.Multiple(() =>
            {
                Assert.That(origCenter.X, Is.EqualTo(0).Within(Tol));
                Assert.That(movedCenter.X, Is.EqualTo(10).Within(Tol));
                Assert.That(movedCenter.Y, Is.EqualTo(-5).Within(Tol));
                Assert.That(movedCenter.Z, Is.EqualTo(3).Within(Tol));
                Assert.That(rotated.Volume, Is.EqualTo(48.0).Within(48.0 * Tol));
                Assert.That(rotBounds.MaxPoint.X - rotBounds.MinPoint.X, Is.EqualTo(4.0).Within(1e-3));
                Assert.That(rotBounds.MaxPoint.Y - rotBounds.MinPoint.Y, Is.EqualTo(2.0).Within(1e-3));
                Assert.That(scaled.Volume, Is.EqualTo(48.0 * 2 * 3 * 0.5).Within(1e-3));
                Assert.That(xformCenter.X, Is.EqualTo(5.0).Within(Tol));
                Assert.That(transformed.Volume, Is.EqualTo(48.0).Within(48.0 * Tol));
            });
        }

        [Test]
        public void WhenDistancesAreMeasuredBetweenPointsAndSolidsThenResultsMatchExactClearance()
        {
            using var a = Cuboid.ByCorners(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(2, 2, 2));
            using var b = Cuboid.ByCorners(Point.ByCoordinates(5, 0, 0), Point.ByCoordinates(7, 2, 2));
            using var p = Point.ByCoordinates(1, 1, 6);

            Assert.Multiple(() =>
            {
                Assert.That(a.DistanceTo(b), Is.EqualTo(3.0).Within(Tol));
                Assert.That(a.DistanceTo(p), Is.EqualTo(4.0).Within(Tol));
                Assert.That(p.DistanceTo(a), Is.EqualTo(4.0).Within(Tol));
            });
        }

        [Test]
        public void WhenVectorsPlanesAndCoordinateSystemsComposeThenAlgebraIsConsistent()
        {
            using var plane = Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 5), Vector.ZAxis());
            using var offset = plane.Offset(2.5);
            using var cs = CoordinateSystem.ByPlane(offset);
            using var inv = cs.Inverse();
            using var local = Point.ByCoordinates(3, 4, 0);
            using var world = (Point)local.Transform(cs);
            using var roundTrip = (Point)world.Transform(inv);

            var package = new DefaultRenderPackage();
            ((IGraphicItem)offset).Tessellate(package, new TessellationParameters());

            Assert.Multiple(() =>
            {
                Assert.That(cs.IsUnscaledOrtho, Is.True);
                Assert.That(world.X, Is.EqualTo(3.0).Within(Tol));
                Assert.That(world.Y, Is.EqualTo(4.0).Within(Tol));
                Assert.That(world.Z, Is.EqualTo(7.5).Within(Tol));
                Assert.That(roundTrip.DistanceTo(local), Is.LessThan(Tol));
                Assert.That(Vector.XAxis().AngleWithVector(Vector.YAxis()), Is.EqualTo(90.0).Within(Tol));
                Assert.That(package.MeshVertexCount, Is.EqualTo(6));
            });
        }

        [Test]
        public void WhenUnsupportedAsmPersistenceMethodsAreCalledThenExplicitNotSupportedDiagnosticIsThrown()
        {
            using var box = Cuboid.ByLengths(2, 2, 2);
            Assert.Multiple(() =>
            {
                Assert.That(() => box.ExportToSAT("test.sat"), Throws.TypeOf<NotSupportedException>().With.Message.Contains("OpenGeometry"));
                Assert.That(() => box.SerializeAsSAB(), Throws.TypeOf<NotSupportedException>().With.Message.Contains("OpenGeometry"));
                Assert.That(() => box.ToSolidDef(), Throws.TypeOf<NotSupportedException>().With.Message.Contains("OpenGeometry"));
            });
        }
    }
}
