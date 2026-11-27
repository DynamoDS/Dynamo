using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using NUnit.Framework;
using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace OpenGeometryTests
{
    [TestFixture]
    public sealed class ModelingOperationsAndMeshTests
    {
        [Test]
        public void WhenCurvesAreExtrudedThenSurfaceAndSolidHaveAnalyticAreaAndVolume()
        {
            using var circle = Circle.ByCenterPointRadius(Point.Origin(), 3.0);
            using var wall = circle.Extrude(10.0);
            Assert.That(wall.Area, Is.EqualTo(2.0 * Math.PI * 3.0 * 10.0).Within(1e-2));

            using var solidCyl = circle.ExtrudeAsSolid(10.0);
            Assert.That(solidCyl.Volume, Is.EqualTo(Math.PI * 9.0 * 10.0).Within(1e-2));

            using var rect = Rectangle.ByWidthLength(4.0, 5.0);
            using var prism = rect.ExtrudeAsSolid(Vector.ZAxis(), 6.0);
            Assert.That(prism.Volume, Is.EqualTo(120.0).Within(1e-3));
            Assert.That(prism.Faces.Length, Is.EqualTo(6));

            using var patch = rect.Patch();
            Assert.That(patch.Area, Is.EqualTo(20.0).Within(1e-3));
        }

        [Test]
        public void WhenSolidIsCreatedByLoftSweepAndRevolveThenAnalyticVolumesHold()
        {
            // Frustum loft from 4x4 square at z=0 to 2x2 square at z=6:
            // V = h/3 * (A1 + A2 + sqrt(A1*A2)) = 2 * (16 + 4 + 8) = 56.
            using var bottom = Rectangle.ByWidthLength(Plane.XY(), 4.0, 4.0);
            using var top = Rectangle.ByWidthLength(Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 6), Vector.ZAxis()), 2.0, 2.0);
            using var frustum = Solid.ByLoft(new Curve[] { bottom, top });
            Assert.That(frustum.Volume, Is.EqualTo(56.0).Within(1e-2));

            // Sweep a radius-2 circle in XY along a vertical line of length 8: V = pi * 4 * 8 = 32*pi.
            using var profile = Circle.ByCenterPointRadius(Point.Origin(), 2.0);
            using var path = Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(0, 0, 8));
            using var swept = Solid.BySweep(profile, path);
            Assert.That(swept.Volume, Is.EqualTo(32.0 * Math.PI).Within(1e-1));

            // Revolve a 2x4 rectangle centered at x=5 (from x=4..6, z=0..4) 360 deg around Z axis.
            // By Pappus's centroid theorem: V = Area * (2 * pi * R_centroid) = 8 * 2 * pi * 5 = 80*pi.
            using var revProfile = Polygon.ByPoints(new[]
            {
                Point.ByCoordinates(4, 0, 0),
                Point.ByCoordinates(6, 0, 0),
                Point.ByCoordinates(6, 0, 4),
                Point.ByCoordinates(4, 0, 4)
            });
            using var ring = Solid.ByRevolve(revProfile, Point.Origin(), Vector.ZAxis(), 0, 360);
            Assert.That(ring.Volume, Is.EqualTo(80.0 * Math.PI).Within(1e-1));
        }

        [Test]
        public void WhenSurfaceIsOffsetAndThickenedThenOffsetPositionAndSolidVolumeAreExact()
        {
            using var plate = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 5, 0),
                Point.ByCoordinates(0, 5, 0)
            });

            using var offset = plate.Offset(3.0);
            Assert.That(offset.Area, Is.EqualTo(50.0).Within(1e-3));
            Assert.That(Math.Abs(offset.PointAtParameter(0.5, 0.5).Z), Is.EqualTo(3.0).Within(1e-4));

            using var thickOneSide = plate.Thicken(2.0, both_sides: false);
            Assert.That(thickOneSide.Volume, Is.EqualTo(100.0).Within(1e-2));

            using var thickBothSides = plate.Thicken(2.0, both_sides: true);
            Assert.That(thickBothSides.Volume, Is.EqualTo(100.0).Within(1e-2));
            var bb = thickBothSides.BoundingBox;
            Assert.That(bb.MinPoint.Z, Is.EqualTo(-1.0).Within(1e-3));
            Assert.That(bb.MaxPoint.Z, Is.EqualTo(1.0).Within(1e-3));
        }

        [Test]
        public void WhenCuboidEdgesAreFilletedAndChamferedThenAnalyticVolumeReductionsMatch()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            var singleEdge = new[] { box.Edges[0] };

            // Single 90-deg edge of length 10 filleted with radius r=1 removes (1 - pi/4) * r^2 * 10.
            var expectedFilletVolume = 1000.0 - (1.0 - Math.PI / 4.0) * 1.0 * 10.0;
            using var filleted = box.Fillet(singleEdge, 1.0);
            Assert.That(filleted.Volume, Is.EqualTo(expectedFilletVolume).Within(1e-2));
            Assert.That(filleted.Faces.Length, Is.EqualTo(7));

            // Single 90-deg edge of length 10 chamfered with setback c=1 removes 0.5 * c^2 * 10 = 5.
            using var chamfered = box.Chamfer(singleEdge, 1.0);
            Assert.That(chamfered.Volume, Is.EqualTo(995.0).Within(1e-2));
            Assert.That(chamfered.Faces.Length, Is.EqualTo(7));

            // Original box must be untouched.
            Assert.That(box.Volume, Is.EqualTo(1000.0).Within(1e-4));
        }

        [Test]
        public void WhenSolidIsThinShelledAndSplitByPlaneThenVolumesAreExact()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);

            // Inward shell thickness 1 leaves hollow interior 8x8x8 = 512 => shell volume = 1000 - 512 = 488.
            using var shelledIn = box.ThinShell(1.0, 0.0);
            Assert.That(shelledIn.Volume, Is.EqualTo(488.0).Within(1e-1));

            // Split centered 10x10x10 box with Z=2 plane => two halves of height 3 (vol 300) and height 7 (vol 700).
            using var cutPlane = Plane.ByOriginNormal(Point.ByCoordinates(0, 0, 2), Vector.ZAxis());
            var halves = box.Split(cutPlane);
            try
            {
                Assert.That(halves.Length, Is.EqualTo(2));
                var volumes = halves.Cast<Solid>().Select(s => s.Volume).OrderBy(v => v).ToArray();
                Assert.That(volumes[0], Is.EqualTo(300.0).Within(1e-2));
                Assert.That(volumes[1], Is.EqualTo(700.0).Within(1e-2));
            }
            finally
            {
                foreach (var h in halves) h.Dispose();
            }
        }

        [Test]
        public void WhenMeshIsCreatedFromPointsAndIndexGroupsThenAreaVolumeTransformsAndPreviewAreExact()
        {
            // Unit cube from 8 vertices and 6 outward-oriented quad faces.
            var verts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(2, 0, 0),
                Point.ByCoordinates(2, 2, 0),
                Point.ByCoordinates(0, 2, 0),
                Point.ByCoordinates(0, 0, 2),
                Point.ByCoordinates(2, 0, 2),
                Point.ByCoordinates(2, 2, 2),
                Point.ByCoordinates(0, 2, 2)
            };
            var faces = new[]
            {
                IndexGroup.ByIndices(0, 3, 2, 1), // bottom (-Z)
                IndexGroup.ByIndices(4, 5, 6, 7), // top (+Z)
                IndexGroup.ByIndices(0, 1, 5, 4), // front (-Y)
                IndexGroup.ByIndices(2, 3, 7, 6), // back (+Y)
                IndexGroup.ByIndices(0, 4, 7, 3), // left (-X)
                IndexGroup.ByIndices(1, 2, 6, 5)  // right (+X)
            };

            using var mesh = Mesh.ByPointsFaceIndices(verts, faces);
            Assert.That(mesh.VertexCount, Is.EqualTo(8));
            Assert.That(mesh.TriangleCount, Is.EqualTo(12));
            Assert.That(mesh.Area, Is.EqualTo(24.0).Within(1e-6));
            Assert.That(mesh.Volume, Is.EqualTo(8.0).Within(1e-6));
            Assert.That(mesh.VertexNormals.Length, Is.EqualTo(8));

            using var scaled = (Mesh)mesh.Scale(2.0);
            Assert.That(scaled.Volume, Is.EqualTo(64.0).Within(1e-6));
            Assert.That(scaled.Area, Is.EqualTo(96.0).Within(1e-6));

            var pkg = new MeshCapturePackage();
            mesh.Tessellate(pkg, new TessellationParameters());
            Assert.That(pkg.MeshVertexCount, Is.EqualTo(36)); // 12 triangles * 3 vertices
        }

        [Test]
        public void WhenDesignScriptExecutesSliceCModelingAndMeshThenResultsAreExact()
        {
            var assemblyPath = typeof(Solid).Assembly.Location.Replace("\\", "\\\\");
            var builtinPath = typeof(DesignScript.Builtin.Dictionary).Assembly.Location.Replace("\\", "\\\\");
            var script = $@"
import(""{builtinPath}"");
import(""{assemblyPath}"");
r = Rectangle.ByWidthLength(4.0, 5.0);
prism = r.ExtrudeAsSolid(10.0);
edges = prism.Edges;
e0 = edges[0];
chamfered = prism.Chamfer([e0], 1.0);
vol = chamfered.Volume;
tri = Mesh.ByPointsFaceIndices(
    [Point.ByCoordinates(0,0,0), Point.ByCoordinates(4,0,0), Point.ByCoordinates(0,3,0)],
    [IndexGroup.ByIndices(0, 1, 2)]);
triArea = tri.Area;
";
            var core = new ProtoCore.Core(new ProtoCore.Options());
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
            var runner = new ProtoScriptRunner();
            var runtimeCore = runner.Execute(script, core);

            Assert.That(core.BuildStatus.ErrorCount, Is.EqualTo(0));
            var rData = new RuntimeMirror("r", 0, runtimeCore).GetData();
            var prismData = new RuntimeMirror("prism", 0, runtimeCore).GetData();
            var edgesData = new RuntimeMirror("edges", 0, runtimeCore).GetData();
            var e0Data = new RuntimeMirror("e0", 0, runtimeCore).GetData();
            var firstElem = edgesData.IsCollection ? edgesData.GetElements().FirstOrDefault()?.Data : null;
            var warnings = string.Join("; ", runtimeCore.RuntimeStatus.Warnings.Select(w => w.Message));
            Assert.That(warnings, Is.Empty, $"r={rData.Data?.GetType().Name ?? "null"}, prism={prismData.Data?.GetType().Name ?? "null"}, edgesCount={(edgesData.IsCollection ? edgesData.GetElements().Count() : -1)}, firstElem={firstElem?.GetType().Name ?? "null"}, e0={e0Data.Data?.GetType().Name ?? "null"}");
            var volMirror = new RuntimeMirror("vol", 0, runtimeCore);
            Assert.That(volMirror.GetData().Data, Is.Not.Null);
            Assert.That(Convert.ToDouble(volMirror.GetData().Data), Is.LessThan(200.0));
            Assert.That(Convert.ToDouble(volMirror.GetData().Data), Is.GreaterThan(190.0));

            var areaMirror = new RuntimeMirror("triArea", 0, runtimeCore);
            Assert.That(Convert.ToDouble(areaMirror.GetData().Data), Is.EqualTo(6.0).Within(1e-5));
        }

        private sealed class MeshCapturePackage : IRenderPackage
        {
            public int MeshVertexCount { get; private set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public bool HasRenderingData => MeshVertexCount > 0;
            public bool DisplayLabels { get; set; }
            public bool RequiresPerVertexColoration { get; set; }
            public int PointVertexCount => 0;
            public int LineVertexCount => 0;
            public IEnumerable<int> LineStripVertexCounts => Enumerable.Empty<int>();
            public IEnumerable<int> LineStripIndices => Enumerable.Empty<int>();
            public IEnumerable<double> LineStripVertices => Enumerable.Empty<double>();
            public IEnumerable<byte> LineStripVertexColors => Enumerable.Empty<byte>();
            public IEnumerable<int> MeshIndices => Enumerable.Empty<int>();
            public IEnumerable<double> MeshVertices => Enumerable.Empty<double>();
            public IEnumerable<double> MeshNormals => Enumerable.Empty<double>();
            public IEnumerable<double> MeshTextureCoordinates => Enumerable.Empty<double>();
            public IEnumerable<byte> MeshVertexColors => Enumerable.Empty<byte>();
            public IEnumerable<int> PointIndices => Enumerable.Empty<int>();
            public IEnumerable<double> PointVertices => Enumerable.Empty<double>();
            public IEnumerable<byte> PointVertexColors => Enumerable.Empty<byte>();
            public IEnumerable<byte> Colors => Enumerable.Empty<byte>();
            public int ColorsStride { get; set; }
            public void AddPointVertex(double x, double y, double z) { }
            public void AddPointVertexColor(byte red, byte green, byte blue, byte alpha) { }
            public void AddTriangleVertex(double x, double y, double z) => MeshVertexCount++;
            public void AddTriangleVertexNormal(double x, double y, double z) { }
            public void AddTriangleVertexUV(double u, double v) { }
            public void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha) { }
            public void AddLineStripVertex(double x, double y, double z) { }
            public void AddLineStripVertexCount(int n) { }
            public void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha) { }
            public void ApplyPointVertexColors(byte[] colors) { }
            public void ApplyLineVertexColors(byte[] colors) { }
            public void ApplyMeshVertexColors(byte[] colors) { }
            public void SetColors(byte[] colors) { }
            public void Clear() => MeshVertexCount = 0;
        }
    }
}
