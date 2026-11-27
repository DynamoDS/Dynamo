using System;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture]
    public sealed class SliceEMeshAndPanelSurfaceTests
    {
        [Test]
        public void WhenMeshCreatedFromGeometryAndPrimitivesThenMetricsMatchExactValues()
        {
            using var box = Cuboid.ByLengths(Point.Origin(), 10, 10, 10);
            using var mesh = Mesh.ByGeometry(box);

            Assert.Multiple(() =>
            {
                Assert.That(mesh.VertexCount, Is.EqualTo(8));
                Assert.That(mesh.TriangleCount, Is.EqualTo(12));
                Assert.That(mesh.EdgeCount, Is.EqualTo(18));
                Assert.That(mesh.Area, Is.EqualTo(600.0).Within(1e-3));
                Assert.That(mesh.Volume, Is.EqualTo(1000.0).Within(1e-3));
                Assert.That(mesh.VerticesAsThreeNumbers.Length, Is.EqualTo(24));
                Assert.That(mesh.TrianglesAsNineNumbers.Length, Is.EqualTo(12 * 9));
                Assert.That(mesh.EdgesAsSixNumbers.Length, Is.EqualTo(18 * 6));
                Assert.That(mesh.TriangleNormals().Length, Is.EqualTo(12));
                Assert.That(mesh.TriangleCentroids().Length, Is.EqualTo(12));
            });

            var tris = mesh.Triangles();
            try
            {
                Assert.That(tris.Length, Is.EqualTo(12));
            }
            finally
            {
                foreach (var t in tris) t.Dispose();
            }

            using var planeMesh = Mesh.Plane(Point.Origin(), 4, 6, 2, 3);
            Assert.Multiple(() =>
            {
                Assert.That(planeMesh.VertexCount, Is.EqualTo(12));
                Assert.That(planeMesh.TriangleCount, Is.EqualTo(12));
                Assert.That(planeMesh.Area, Is.EqualTo(24.0).Within(1e-6));
            });
        }

        [Test]
        public void WhenMeshBooleansExecuteViaSmPolyBrepThenVolumesAreExactAndInputsSurvive()
        {
            using var meshA = Mesh.Cuboid(Point.Origin(), 10, 10, 10);
            using var meshB = Mesh.Cuboid(Point.ByCoordinates(5, 0, 0), 10, 10, 10);

            // Overlap is a 5 x 10 x 10 = 500 volume region.
            using var unionMesh = meshA.BooleanUnion(meshB);
            using var interMesh = meshA.BooleanIntersection(meshB);
            using var diffMesh = meshA.BooleanDifference(meshB);

            Assert.Multiple(() =>
            {
                Assert.That(unionMesh.Volume, Is.EqualTo(1500.0).Within(1e-1));
                Assert.That(interMesh.Volume, Is.EqualTo(500.0).Within(1e-1));
                Assert.That(diffMesh.Volume, Is.EqualTo(500.0).Within(1e-1));
                // Original operands remain completely intact after all three operations.
                Assert.That(meshA.Volume, Is.EqualTo(1000.0).Within(1e-3));
                Assert.That(meshB.Volume, Is.EqualTo(1000.0).Within(1e-3));
            });
        }

        [Test]
        public void WhenPanelSurfaceSubdividesSurfaceThenCountsAndPolygonsMatchGrid()
        {
            using var srf = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 8, 0),
                Point.ByCoordinates(0, 8, 0)
            });

            using var quads = PanelSurface.ByQuads(srf, 5, 4, PanelSurfaceBoundaryCondition.Keep);
            using var diag = PanelSurface.ByDiagonallySplitSquares(srf, 5, 4, false, PanelSurfaceBoundaryCondition.Trim);
            using var cross = PanelSurface.ByCrossSplitSquares(srf, 2, 2, PanelSurfaceBoundaryCondition.Remove);

            Assert.Multiple(() =>
            {
                Assert.That(quads.NumPanels, Is.EqualTo(20));
                Assert.That(quads.NumVertices, Is.EqualTo(30));
                Assert.That(quads.GetNumPanelVertices()[0], Is.EqualTo(4));
                Assert.That(diag.NumPanels, Is.EqualTo(40));
                Assert.That(cross.NumPanels, Is.EqualTo(16));
            });

            var polys = quads.GetPanelPolygon(new[] { 0, 19 });
            try
            {
                Assert.That(polys.Length, Is.EqualTo(2));
                Assert.That(polys[0].Points.Length, Is.EqualTo(4));
            }
            finally
            {
                foreach (var p in polys) p.Dispose();
            }
        }
    }
}
