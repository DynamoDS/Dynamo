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
    public sealed class CurvesSurfacesTopologyTests
    {
        private const double Tolerance = 1e-4;

        [Test]
        public void WhenLineCreatedAndOperatedThenLengthTangentSplitTrimAndPreviewAreExact()
        {
            using var line = Line.ByStartPointEndPoint(Point.ByCoordinates(1, 2, 3), Point.ByCoordinates(4, 6, 3));
            Assert.That(line.Length, Is.EqualTo(5.0).Within(1e-6));
            Assert.That(line.IsClosed, Is.False);
            Assert.That(line.Direction.X, Is.EqualTo(3.0).Within(1e-6));
            Assert.That(line.Direction.Y, Is.EqualTo(4.0).Within(1e-6));

            using var mid = line.PointAtParameter(0.5);
            Assert.That(mid.X, Is.EqualTo(2.5).Within(1e-6));
            Assert.That(mid.Y, Is.EqualTo(4.0).Within(1e-6));
            Assert.That(mid.Z, Is.EqualTo(3.0).Within(1e-6));

            using var tan = line.TangentAtParameter(0.5);
            Assert.That(tan.X, Is.EqualTo(0.6).Within(1e-6));
            Assert.That(tan.Y, Is.EqualTo(0.8).Within(1e-6));

            using var atLen = line.PointAtSegmentLength(2.5);
            Assert.That(atLen.DistanceTo(mid), Is.LessThan(1e-6));

            using var rev = line.Reverse();
            Assert.That(rev.StartPoint.DistanceTo(line.EndPoint), Is.LessThan(1e-6));
            Assert.That(rev.EndPoint.DistanceTo(line.StartPoint), Is.LessThan(1e-6));

            var pieces = line.SplitByParameter(0.4);
            try
            {
                Assert.That(pieces.Length, Is.EqualTo(2));
                Assert.That(pieces[0].Length, Is.EqualTo(2.0).Within(1e-6));
                Assert.That(pieces[1].Length, Is.EqualTo(3.0).Within(1e-6));
            }
            finally
            {
                foreach (var p in pieces) p.Dispose();
            }

            var pkg = new CurveRenderPackage();
            line.Tessellate(pkg, new TessellationParameters());
            Assert.That(pkg.LineVertices.Count, Is.EqualTo(2));
            Assert.That(pkg.StripCounts, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void WhenCircleAndArcCreatedThenCircumferenceNormalAndThreePointFitAreExact()
        {
            using var circle = Circle.ByCenterPointRadiusNormal(
                Point.ByCoordinates(1, 2, 3), 5.0, Vector.ByCoordinates(0, 1, 0));
            Assert.That(circle.IsClosed, Is.True);
            Assert.That(circle.Length, Is.EqualTo(2.0 * Math.PI * 5.0).Within(1e-4));
            Assert.That(circle.Normal.Y, Is.EqualTo(1.0).Within(1e-6));

            using var from3 = Circle.ByThreePoints(
                Point.ByCoordinates(3, 0, 0),
                Point.ByCoordinates(0, 3, 0),
                Point.ByCoordinates(-3, 0, 0));
            Assert.That(from3.Radius, Is.EqualTo(3.0).Within(1e-6));
            Assert.That(from3.CenterPoint.DistanceTo(Point.Origin()), Is.LessThan(1e-6));

            using var arc = Arc.ByCenterPointRadiusAngle(Point.Origin(), 4.0, 0, 90, Vector.ZAxis());
            Assert.That(arc.IsClosed, Is.False);
            Assert.That(arc.Length, Is.EqualTo(2.0 * Math.PI).Within(1e-4));
            Assert.That(arc.StartPoint.DistanceTo(Point.ByCoordinates(4, 0, 0)), Is.LessThan(1e-5));
            Assert.That(arc.EndPoint.DistanceTo(Point.ByCoordinates(0, 4, 0)), Is.LessThan(1e-5));

            using var arc3 = Arc.ByThreePoints(
                Point.ByCoordinates(5, 0, 0),
                Point.ByCoordinates(0, 5, 0),
                Point.ByCoordinates(-5, 0, 0));
            Assert.That(arc3.Radius, Is.EqualTo(5.0).Within(1e-5));
            Assert.That(arc3.Length, Is.EqualTo(Math.PI * 5.0).Within(1e-4));
        }

        [Test]
        public void WhenNurbsCurveCreatedThenInterpolationAndControlPointFormsWork()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(1, 2, 0),
                Point.ByCoordinates(3, 2, 1),
                Point.ByCoordinates(5, 0, 0)
            };

            using var interp = NurbsCurve.ByPoints(pts);
            Assert.That(interp.StartPoint.DistanceTo(pts[0]), Is.LessThan(1e-5));
            Assert.That(interp.EndPoint.DistanceTo(pts[^1]), Is.LessThan(1e-5));
            foreach (var p in pts)
                Assert.That(interp.DistanceTo(p), Is.LessThan(1e-4));

            using var ctrl = NurbsCurve.ByControlPoints(pts, degree: 3);
            Assert.That(ctrl.Degree, Is.EqualTo(3));
            Assert.That(ctrl.StartPoint.DistanceTo(pts[0]), Is.LessThan(1e-5));
            Assert.That(ctrl.EndPoint.DistanceTo(pts[^1]), Is.LessThan(1e-5));
            Assert.That(ctrl.Length, Is.GreaterThan(5.0));
        }

        [Test]
        public void WhenPolyCurvePolygonAndRectangleCreatedThenOrderingPerimeterAndAreaMatchAnalyticValues()
        {
            // Supply rectangle edges out of order and with one reversed; ByJoinedCurves must stitch into closed chain.
            using var e0 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(4, 0, 0));
            using var e2Reversed = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 3, 0), Point.ByCoordinates(4, 3, 0));
            using var e1 = Line.ByStartPointEndPoint(Point.ByCoordinates(4, 0, 0), Point.ByCoordinates(4, 3, 0));
            using var e3 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 3, 0), Point.ByCoordinates(0, 0, 0));

            using var poly = PolyCurve.ByJoinedCurves(new Curve[] { e0, e2Reversed, e1, e3 });
            Assert.That(poly.NumberOfCurves, Is.EqualTo(4));
            Assert.That(poly.IsClosed, Is.True);
            Assert.That(poly.Length, Is.EqualTo(14.0).Within(1e-5));

            using var circle = Circle.ByCenterPointRadius(Point.Origin(), 10.0);
            using var hex = Polygon.RegularPolygon(circle, 6);
            Assert.That(hex.Points.Length, Is.EqualTo(6));
            Assert.That(hex.Length, Is.EqualTo(60.0).Within(1e-4));
            Assert.That(hex.Center().DistanceTo(Point.Origin()), Is.LessThan(1e-6));

            using var rect = Rectangle.ByWidthLength(6.0, 4.0);
            Assert.That(rect.Width, Is.EqualTo(6.0).Within(1e-6));
            Assert.That(rect.Height, Is.EqualTo(4.0).Within(1e-6));
            Assert.That(rect.Length, Is.EqualTo(20.0).Within(1e-5));
            Assert.That(rect.IsClosed, Is.True);
        }

        [Test]
        public void WhenSurfacesCreatedByPerimeterPatchAndLoftThenAreaNormalAndPreviewAreAccurate()
        {
            using var quad = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(5, 0, 0),
                Point.ByCoordinates(5, 4, 0),
                Point.ByCoordinates(0, 4, 0)
            });
            Assert.That(quad.Area, Is.EqualTo(20.0).Within(Tolerance));
            Assert.That(quad.Perimeter, Is.EqualTo(18.0).Within(Tolerance));
            Assert.That(quad.PointAtParameter(0.5, 0.5).DistanceTo(Point.ByCoordinates(2.5, 2.0, 0)), Is.LessThan(1e-5));
            Assert.That(Math.Abs(quad.NormalAtParameter(0.5, 0.5).Z), Is.EqualTo(1.0).Within(1e-5));

            using var diskCurve = Circle.ByCenterPointRadius(Point.ByCoordinates(0, 0, 2), 3.0);
            using var disk = Surface.ByPatch(diskCurve);
            Assert.That(disk.Area, Is.EqualTo(Math.PI * 9.0).Within(1e-3));

            using var rectCurve = Rectangle.ByWidthLength(8.0, 5.0);
            using var rectPatch = Surface.ByPatch(rectCurve);
            Assert.That(rectPatch.Area, Is.EqualTo(40.0).Within(Tolerance));

            using var low = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(6, 0, 0));
            using var high = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 4, 3), Point.ByCoordinates(6, 4, 3));
            using var loft = Surface.ByLoft(new Curve[] { low, high });
            // Slanted rectangle: width 6, slant height sqrt(4^2 + 3^2) = 5 => area 30.
            Assert.That(loft.Area, Is.EqualTo(30.0).Within(Tolerance));

            var pkg = new CurveRenderPackage();
            loft.Tessellate(pkg, new TessellationParameters());
            Assert.That(pkg.TriangleVertexCount, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void WhenSixBoxFacesAreJoinedThenPolySurfaceAndStitchedSolidMatchExactVolume()
        {
            var p000 = Point.ByCoordinates(0, 0, 0);
            var p100 = Point.ByCoordinates(10, 0, 0);
            var p110 = Point.ByCoordinates(10, 10, 0);
            var p010 = Point.ByCoordinates(0, 10, 0);
            var p001 = Point.ByCoordinates(0, 0, 10);
            var p101 = Point.ByCoordinates(10, 0, 10);
            var p111 = Point.ByCoordinates(10, 10, 10);
            var p011 = Point.ByCoordinates(0, 10, 10);

            var faces = new[]
            {
                Surface.ByPerimeterPoints(new[] { p000, p010, p110, p100 }),
                Surface.ByPerimeterPoints(new[] { p001, p101, p111, p011 }),
                Surface.ByPerimeterPoints(new[] { p000, p100, p101, p001 }),
                Surface.ByPerimeterPoints(new[] { p100, p110, p111, p101 }),
                Surface.ByPerimeterPoints(new[] { p110, p010, p011, p111 }),
                Surface.ByPerimeterPoints(new[] { p010, p000, p001, p011 })
            };

            try
            {
                using var polySurf = PolySurface.ByJoinedSurfaces(faces);
                Assert.That(polySurf.SurfaceCount, Is.EqualTo(6));
                Assert.That(polySurf.Area, Is.EqualTo(600.0).Within(1e-3));

                using var stitched = polySurf.Solid();
                Assert.That(stitched.Volume, Is.EqualTo(1000.0).Within(1e-3));
                Assert.That(stitched.Faces.Length, Is.EqualTo(6));
            }
            finally
            {
                foreach (var f in faces) f.Dispose();
            }
        }

        [Test]
        public void WhenSolidTopologyIsQueriedThenEulerInvariantsAdjacencyAndFaceExtractionHold()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            var faces = box.Faces;
            var edges = box.Edges;
            var vertices = box.Vertices;

            Assert.That(faces.Length, Is.EqualTo(6));
            Assert.That(edges.Length, Is.EqualTo(12));
            Assert.That(vertices.Length, Is.EqualTo(8));
            // Euler formula V - E + F = 2 for genus-0 closed solid.
            Assert.That(vertices.Length - edges.Length + faces.Length, Is.EqualTo(2));

            Assert.That(faces.Sum(f => f.Area), Is.EqualTo(600.0).Within(1e-3));
            foreach (var f in faces)
            {
                Assert.That(f.Edges.Length, Is.EqualTo(4));
                Assert.That(f.Vertices.Length, Is.EqualTo(4));
                using var srf = f.SurfaceGeometry();
                Assert.That(srf.Area, Is.EqualTo(100.0).Within(1e-3));
            }

            foreach (var e in edges)
            {
                Assert.That(e.Length, Is.EqualTo(10.0).Within(1e-5));
                Assert.That(e.AdjacentFaces.Length, Is.EqualTo(2));
                Assert.That(e.StartVertex, Is.Not.Null);
                Assert.That(e.EndVertex, Is.Not.Null);
                using var crv = e.CurveGeometry;
                Assert.That(crv.Length, Is.EqualTo(10.0).Within(1e-5));
            }

            foreach (var v in vertices)
            {
                Assert.That(v.AdjacentEdges.Length, Is.EqualTo(3));
                Assert.That(v.AdjacentFaces.Length, Is.EqualTo(3));
                Assert.That(Math.Abs(v.PointGeometry.X), Is.EqualTo(5.0).Within(1e-5));
            }

            // Cut cuboid with through-bore and verify trimmed face area reflects the circular hole.
            using var bore = Cylinder.ByPointsRadius(Point.ByCoordinates(0, 0, -6), Point.ByCoordinates(0, 0, 6), 2);
            using var cut = box.Difference(bore);
            var cutFaceAreas = cut.Faces.Select(f => f.Area).OrderBy(a => a).ToArray();
            var expectedEndFaceArea = 100.0 - 4.0 * Math.PI;
            Assert.That(cutFaceAreas.Count(a => Math.Abs(a - expectedEndFaceArea) < 1e-2), Is.EqualTo(2));
        }

        [Test]
        public void WhenDesignScriptExecutesCurvesSurfacesAndTopologyThenResultsMatch()
        {
            var assemblyPath = typeof(Solid).Assembly.Location.Replace("\\", "\\\\");
            var script = $@"
import(""{assemblyPath}"");
c = Circle.ByCenterPointRadius(Point.Origin(), 5.0);
disk = Surface.ByPatch(c);
diskArea = disk.Area;
box = Cuboid.ByLengths(4, 6, 8);
faceCount = box.Faces;
edgeCount = box.Edges;
";
            var core = new ProtoCore.Core(new ProtoCore.Options());
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
            var runner = new ProtoScriptRunner();
            var runtimeCore = runner.Execute(script, core);

            Assert.That(core.BuildStatus.ErrorCount, Is.EqualTo(0));
            var areaMirror = new RuntimeMirror("diskArea", 0, runtimeCore);
            Assert.That(Convert.ToDouble(areaMirror.GetData().Data), Is.EqualTo(25.0 * Math.PI).Within(1e-3));

            var facesMirror = new RuntimeMirror("faceCount", 0, runtimeCore);
            Assert.That(facesMirror.GetData().GetElements().Count(), Is.EqualTo(6));

            var edgesMirror = new RuntimeMirror("edgeCount", 0, runtimeCore);
            Assert.That(edgesMirror.GetData().GetElements().Count(), Is.EqualTo(12));
        }

        private sealed class CurveRenderPackage : IRenderPackage
        {
            public List<(double X, double Y, double Z)> LineVertices { get; } = new();
            public List<int> StripCounts { get; } = new();
            public int TriangleVertexCount { get; private set; }

            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public bool HasRenderingData => LineVertices.Count > 0 || TriangleVertexCount > 0;
            public bool DisplayLabels { get; set; }
            public bool RequiresPerVertexColoration { get; set; }
            public int PointVertexCount => 0;
            public int LineVertexCount => LineVertices.Count;
            public int MeshVertexCount => TriangleVertexCount;
            public IEnumerable<int> LineStripVertexCounts => StripCounts;
            public IEnumerable<int> LineStripIndices => Enumerable.Empty<int>();
            public IEnumerable<double> LineStripVertices => LineVertices.SelectMany(v => new[] { v.X, v.Y, v.Z });
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
            public void AddTriangleVertex(double x, double y, double z) => TriangleVertexCount++;
            public void AddTriangleVertexNormal(double x, double y, double z) { }
            public void AddTriangleVertexUV(double u, double v) { }
            public void AddTriangleVertexColor(byte red, byte green, byte blue, byte alpha) { }
            public void AddLineStripVertex(double x, double y, double z) => LineVertices.Add((x, y, z));
            public void AddLineStripVertexCount(int n) => StripCounts.Add(n);
            public void AddLineStripVertexColor(byte red, byte green, byte blue, byte alpha) { }
            public void ApplyPointVertexColors(byte[] colors) { }
            public void ApplyLineVertexColors(byte[] colors) { }
            public void ApplyMeshVertexColors(byte[] colors) { }
            public void SetColors(byte[] colors) { }
            public void Clear() { LineVertices.Clear(); StripCounts.Clear(); TriangleVertexCount = 0; }
        }
    }
}
