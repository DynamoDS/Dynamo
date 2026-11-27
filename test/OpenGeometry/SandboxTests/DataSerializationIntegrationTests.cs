using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class DataSerializationIntegrationTests
    {
        private const double Tolerance = 1e-4;

        [Test]
        public void WhenCoreNodesStringifyAndParseJsonRunOnOpenGeometryThenObjectsRoundTrip()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var cyl = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, -6), Point.ByCoordinates(0, 0, 6), 2);
            using var cut = box.Difference(cyl);
            using var pt = Point.ByCoordinates(2, 4, 6);
            using var vec = Vector.ByCoordinates(1, -2, 3);
            using var cs = CoordinateSystem.ByOrigin(5, 6, 7);
            using var bbox = cut.BoundingBox;
            using var mesh = Mesh.ByGeometry(box);

            var payload = new Dictionary<string, object>
            {
                ["solid"] = cut,
                ["point"] = pt,
                ["vector"] = vec,
                ["cs"] = cs,
                ["bbox"] = bbox,
                ["mesh"] = mesh
            };

            var json = Data.StringifyJSON(payload);
            var parsed = Data.ParseJSON(json) as Dictionary<string, object>;

            Assert.That(parsed, Is.Not.Null);
            using var cut2 = parsed["solid"] as Solid;
            using var pt2 = parsed["point"] as Point;
            using var vec2 = parsed["vector"] as Vector;
            using var cs2 = parsed["cs"] as CoordinateSystem;
            using var bbox2 = parsed["bbox"] as BoundingBox;
            using var mesh2 = parsed["mesh"] as Mesh;

            var expectedVolume = 1000.0 - 40.0 * System.Math.PI;
            Assert.Multiple(() =>
            {
                Assert.That(cut2, Is.Not.Null);
                Assert.That(cut2.Volume, Is.EqualTo(expectedVolume).Within(expectedVolume * Tolerance));
                Assert.That(pt2, Is.Not.Null);
                Assert.That(pt2.DistanceTo(pt), Is.LessThan(Tolerance));
                Assert.That(vec2, Is.Not.Null);
                Assert.That(vec2.Subtract(vec).Length, Is.LessThan(Tolerance));
                Assert.That(cs2, Is.Not.Null);
                Assert.That(cs2.Origin.DistanceTo(cs.Origin), Is.LessThan(Tolerance));
                Assert.That(bbox2, Is.Not.Null);
                Assert.That(bbox2.MinPoint.DistanceTo(bbox.MinPoint), Is.LessThan(Tolerance));
                Assert.That(bbox2.MaxPoint.DistanceTo(bbox.MaxPoint), Is.LessThan(Tolerance));
                Assert.That(mesh2, Is.Not.Null);
                Assert.That(mesh2.Volume, Is.EqualTo(1000.0).Within(1e-2));
            });
        }

        [Test]
        public void WhenDataRememberCachesOpenGeometrySolidThenNullInputRestoresCachedSolid()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var cyl = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, -6), Point.ByCoordinates(0, 0, 6), 2);
            using var cut = box.Difference(cyl);
            var expectedVolume = 1000.0 - 40.0 * System.Math.PI;

            var firstRun = Data.Remember(cut, string.Empty);
            var cachedJson = firstRun["Cache"] as string;
            Assert.That(cachedJson, Is.Not.Null.And.Not.Empty);

            // Simulate reopening the graph without upstream input: Data.Remember must deserialize the cached solid.
            var restoredRun = Data.Remember(null, cachedJson);
            using var restoredSolid = restoredRun[">"] as Solid;

            Assert.That(restoredSolid, Is.Not.Null);
            Assert.That(restoredSolid.Volume, Is.EqualTo(expectedVolume).Within(expectedVolume * Tolerance));
        }
    }
}
