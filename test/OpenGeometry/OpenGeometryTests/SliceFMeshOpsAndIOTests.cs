using System;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture]
    public sealed class SliceFMeshOpsAndIOTests
    {
        [Test]
        public void WhenMeshPlaneCutAndSectionedThenVolumeAndSectionPerimeterMatchAnalyticValues()
        {
            using var box = Mesh.Cuboid(Point.Origin(), 10, 10, 10);
            using var cutPlane = Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 2), Vector.ZAxis());

            // Removes +Z side above z=2, keeping z in [-5, 2] -> height 7, volume 700.
            using var cutSolid = box.PlaneCut(cutPlane, makeSolid: true);
            using var cutOpen = box.PlaneCut(cutPlane, makeSolid: false);

            Assert.Multiple(() =>
            {
                Assert.That(cutSolid.Volume, Is.EqualTo(700.0).Within(1.0));
                Assert.That(cutSolid.BoundingBox.MaxPoint.Z, Is.EqualTo(2.0).Within(1e-3));
                Assert.That(cutSolid.BoundingBox.MinPoint.Z, Is.EqualTo(-5.0).Within(1e-3));
                // Open shell drops the 10x10 top cap face (area 480 - 100 = 380).
                Assert.That(cutOpen.Area, Is.EqualTo(380.0).Within(1.0));
            });

            var sections = box.Intersect(cutPlane);
            try
            {
                Assert.That(sections.Length, Is.GreaterThanOrEqualTo(1));
                var totalLen = sections.Sum(s => s.Length);
                Assert.That(totalLen, Is.EqualTo(40.0).Within(1e-2));
                Assert.That(sections[0].IsClosed, Is.True);
            }
            finally
            {
                foreach (var s in sections) s.Dispose();
            }
        }

        [Test]
        public void WhenOpenBoxMeshMadeWatertightThenMissingFaceIsFilled()
        {
            // 10x10x10 cube missing its top (z=10) face -> 5 faces (10 triangles), area 500.
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 10, 0),
                Point.ByCoordinates(0, 10, 0),
                Point.ByCoordinates(0, 0, 10),
                Point.ByCoordinates(10, 0, 10),
                Point.ByCoordinates(10, 10, 10),
                Point.ByCoordinates(0, 10, 10)
            };
            var openFaces = new[]
            {
                // bottom (z=0, normal -Z)
                IndexGroup.ByIndices(0, 3, 2), IndexGroup.ByIndices(0, 2, 1),
                // front (y=0, normal -Y)
                IndexGroup.ByIndices(0, 1, 5), IndexGroup.ByIndices(0, 5, 4),
                // right (x=10, normal +X)
                IndexGroup.ByIndices(1, 2, 6), IndexGroup.ByIndices(1, 6, 5),
                // back (y=10, normal +Y)
                IndexGroup.ByIndices(2, 3, 7), IndexGroup.ByIndices(2, 7, 6),
                // left (x=0, normal -X)
                IndexGroup.ByIndices(3, 0, 4), IndexGroup.ByIndices(3, 4, 7)
            };

            using var openBox = Mesh.ByPointsFaceIndices(pts, openFaces);
            Assert.That(openBox.Area, Is.EqualTo(500.0).Within(1e-3));

            using var closed = openBox.MakeWatertight();
            Assert.Multiple(() =>
            {
                Assert.That(closed.Area, Is.EqualTo(600.0).Within(1e-2));
                Assert.That(closed.Volume, Is.EqualTo(1000.0).Within(1e-1));
            });
        }

        [Test]
        public void WhenMeshNearestMirrorEdgesExplodeAndSmoothExecuteThenInvariantsHold()
        {
            using var box = Mesh.Cuboid(Point.ByCoordinates(10, 0, 0), 4, 4, 4);

            var nearest = box.Nearest(Point.ByCoordinates(20, 0, 0));
            Assert.Multiple(() =>
            {
                Assert.That(nearest.X, Is.EqualTo(12.0).Within(1e-6));
                Assert.That(nearest.Y, Is.EqualTo(0.0).Within(1e-6));
                Assert.That(nearest.Z, Is.EqualTo(0.0).Within(1e-6));
            });

            using var mirrored = box.Mirror(Plane.YZ());
            Assert.Multiple(() =>
            {
                Assert.That(mirrored.Volume, Is.EqualTo(64.0).Within(1e-3));
                Assert.That(mirrored.BoundingBox.MinPoint.X, Is.EqualTo(-12.0).Within(1e-6));
                Assert.That(mirrored.BoundingBox.MaxPoint.X, Is.EqualTo(-8.0).Within(1e-6));
            });

            var edges = box.Edges();
            var exploded = box.Explode();
            using var smoothed = box.Smooth(1.0);
            try
            {
                Assert.That(edges.Length, Is.EqualTo(box.EdgeCount));
                Assert.That(exploded.Length, Is.EqualTo(box.TriangleCount));
                Assert.That(smoothed.VertexCount, Is.EqualTo(box.VertexCount));
                Assert.That(smoothed.TriangleCount, Is.EqualTo(box.TriangleCount));
                Assert.That(smoothed.Volume, Is.LessThan(box.Volume));
            }
            finally
            {
                foreach (var e in edges) e.Dispose();
                foreach (var m in exploded) m.Dispose();
            }
        }

        [Test]
        public void WhenMeshesExportedAndImportedAsObjStlAndPlyThenGeometryRoundTripsAccurately()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "OpenGeometryMeshIO_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                using var boxA = Mesh.Cuboid(Point.Origin(), 10, 10, 10);
                using var boxB = Mesh.Cuboid(Point.ByCoordinates(20, 0, 0), 4, 4, 4);

                // OBJ multi-mesh round-trip
                var objPath = Path.Combine(tempDir, "pair.obj");
                Assert.That(Mesh.ExportMeshes(objPath, new[] { boxA, boxB }), Is.EqualTo(objPath));
                var objMeshes = Mesh.ImportFile(objPath);
                try
                {
                    Assert.That(objMeshes.Length, Is.EqualTo(2));
                    Assert.That(objMeshes[0].Volume, Is.EqualTo(1000.0).Within(1e-3));
                    Assert.That(objMeshes[1].Volume, Is.EqualTo(64.0).Within(1e-3));
                }
                finally
                {
                    foreach (var m in objMeshes) m.Dispose();
                }

                // Binary STL round-trip
                var stlPath = Path.Combine(tempDir, "box.stl");
                Mesh.ExportMeshes(stlPath, new[] { boxA });
                var stlMeshes = Mesh.ImportFile(stlPath);
                try
                {
                    Assert.That(stlMeshes.Length, Is.EqualTo(1));
                    Assert.That(stlMeshes[0].VertexCount, Is.EqualTo(8));
                    Assert.That(stlMeshes[0].TriangleCount, Is.EqualTo(12));
                    Assert.That(stlMeshes[0].Volume, Is.EqualTo(1000.0).Within(1e-2));
                }
                finally
                {
                    foreach (var m in stlMeshes) m.Dispose();
                }

                // ASCII PLY round-trip
                var plyPath = Path.Combine(tempDir, "box.ply");
                Mesh.ExportMeshes(plyPath, new[] { boxA });
                var plyMeshes = Mesh.ImportFile(plyPath);
                try
                {
                    Assert.That(plyMeshes.Length, Is.EqualTo(1));
                    Assert.That(plyMeshes[0].VertexCount, Is.EqualTo(8));
                    Assert.That(plyMeshes[0].TriangleCount, Is.EqualTo(12));
                    Assert.That(plyMeshes[0].Volume, Is.EqualTo(1000.0).Within(1e-2));
                }
                finally
                {
                    foreach (var m in plyMeshes) m.Dispose();
                }

                // Unsupported proprietary format throws explicit NotSupportedException
                Assert.Throws<NotSupportedException>(() =>
                    Mesh.ExportMeshes(Path.Combine(tempDir, "unsupported.mix"), new[] { boxA }));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
