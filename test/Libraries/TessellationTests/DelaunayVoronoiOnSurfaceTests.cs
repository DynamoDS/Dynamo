using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;
using NUnit.Framework;
using Tessellation;
using Tessellation.Adapters;

namespace Dynamo.Tests
{
    [TestFixture]
    public class DelaunayVoronoiOnSurfaceTests : DynamoModelTestBase
    {
        private const double Epsilon = 1e-9;

        // Validates that Delaunay.ByParametersOnSurface produces the expected triangulation edges
        // (in world space) on a strongly anisotropic surface, and that the triangulation satisfies
        // the empty-circumcircle property in the same scaled UV space used by production.
        [Test]
        public void Delaunay_ByParametersOnSurface_SatisfiesEmptyCircumcircleProperty_WithScaledSurface()
        {
            using var surface = CreateSurface(width: 1000, height: 1); // extremely anisotropic to exercise UV scaling
            var uvs = CreateUvGrid(countU: 5, countV: 5);

            var triangles = ComputeDelaunayTrianglesInScaledUvSpace(uvs, surface);
            Assert.That(triangles.Count, Is.GreaterThan(0));

            AssertTrianglesSatisfyEmptyCircumcircle(triangles);

            // Basic sanity: API returns some edges.
            var edges = Delaunay.ByParametersOnSurface(uvs, surface).ToList();
            Assert.That(edges.Count, Is.GreaterThan(0));

            // Validate that the API output matches the edges implied by the Delaunay triangles.
            AssertDelaunayEdgesMatchTriangulation(triangles, surface, edges);
        }

        // Validates that Voronoi.ByParametersOnSurface returns edges whose vertices behave like the
        // dual of a scaled-UV Delaunay triangulation. The assertion compares API vertices against
        // scaled-vs-unscaled dual circumcenters and requires scaled geometry to be measurably closer.
        [Test]
        public void Voronoi_ByParametersOnSurface_ProducesEdges_AndDelaunayDualIsValid_WithScaledSurface()
        {
            using var surface = CreateSurface(width: 1000, height: 1); // extremely anisotropic to exercise UV scaling
            var uvs = CreateUvGrid(countU: 5, countV: 5);

            // Voronoi edges should be generated.
            var edges = Voronoi.ByParametersOnSurface(uvs, surface).ToList();
            Assert.That(edges.Count, Is.GreaterThan(0));

            // Voronoi is the dual of Delaunay. The Voronoi vertices are the circumcenters of the Delaunay triangles.
            // Compute the correct circumcenters in scaled UV space (as Delaunay does).
            var triangles = ComputeDelaunayTrianglesInScaledUvSpace(uvs, surface);
            Assert.That(triangles.Count, Is.GreaterThan(0));

            // Validate that Delaunay triangulation satisfies empty circumcircle property.
            AssertTrianglesSatisfyEmptyCircumcircle(triangles);

            // Validate that the API output matches the Voronoi edges implied by the Delaunay dual.
            AssertVoronoiEdgesMatchDelaunayDual(uvs, triangles, surface, edges);
        }

        private static Surface CreateSurface(double width, double height)
        {
            // Construct a planar surface whose param space is [0,1]x[0,1] with strong anisotropy in world units.
            using var rect = Rectangle.ByWidthLength(width, height);
            return Surface.ByPatch(rect);
        }

        private static List<UV> CreateUvGrid(int countU, int countV)
        {
            if (countU < 2) throw new ArgumentOutOfRangeException(nameof(countU));
            if (countV < 2) throw new ArgumentOutOfRangeException(nameof(countV));

            var uvs = new List<UV>(countU * countV);
            for (int i = 0; i < countU; i++)
            {
                var u = i / (double)(countU - 1);
                for (int j = 0; j < countV; j++)
                {
                    var v = j / (double)(countV - 1);
                    // Slightly jitter interior points to reduce degenerate cocircular cases.
                    if (i != 0 && i != countU - 1 && j != 0 && j != countV - 1)
                    {
                        u += (i + j) * 1e-6;
                        v += (i - j) * 1e-6;
                    }

                    uvs.Add(UV.ByCoordinates(u, v));
                }
            }

            return uvs;
        }

        /// <summary>
        /// Extracts triangles from the actual Delaunay implementation using the same algorithm as production code.
        /// This calls the real MIConvexHull library that Delaunay.ByParametersOnSurface uses internally.
        /// </summary>
        private static List<Triangle> ComputeDelaunayTrianglesInScaledUvSpace(IReadOnlyList<UV> uvs, Surface face)
        {
            var (normU, normV) = UvScalingUtilities.GetNormalizedUvScales(face);

            return ComputeDelaunayTrianglesInUvSpace(uvs, normU, normV);
        }

        private static List<Triangle> ComputeDelaunayTrianglesInUnscaledUvSpace(IReadOnlyList<UV> uvs)
        {
            return ComputeDelaunayTrianglesInUvSpace(uvs, 1.0, 1.0);
        }

        private static List<Triangle> ComputeDelaunayTrianglesInUvSpace(IReadOnlyList<UV> uvs, double scaleU, double scaleV)
        {
            // Use the same triangulation algorithm as the production Delaunay.ByParametersOnSurface method.
            var verts = uvs.Select(uv => new Vertex2(uv.U * scaleU, uv.V * scaleV)).ToList();

            const double PlaneDistanceTolerance = 1e-6;
            var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts, PlaneDistanceTolerance);

            // Convert each cell (triangle) to our test Triangle structure
            var triangles = new List<Triangle>();
            foreach (var cell in triangulation.Cells)
            {
                var v1 = cell.Vertices[0].AsVector();
                var v2 = cell.Vertices[1].AsVector();
                var v3 = cell.Vertices[2].AsVector();

                var a = new Pt(v1.X, v1.Y);
                var b = new Pt(v2.X, v2.Y);
                var c = new Pt(v3.X, v3.Y);

                triangles.Add(new Triangle(a, b, c));
            }

            return triangles;
        }

        private static void AssertTrianglesSatisfyEmptyCircumcircle(IReadOnlyList<Triangle> triangles)
        {
            var allPoints = triangles.SelectMany(t => t.Vertices).Distinct().ToList();

            foreach (var t in triangles)
            {
                var (center, radius) = t.Circumcircle;

                foreach (var p in allPoints)
                {
                    if (t.ContainsVertex(p))
                        continue;

                    var dist = center.DistanceTo(p);
                    Assert.That(dist + 1e-9, Is.GreaterThanOrEqualTo(radius),
                        $"Point should be outside circumcircle. dist={dist}, radius={radius}, triangle={t}");
                }
            }
        }

        private static void AssertVoronoiEdgesMatchDelaunayDual(
            IReadOnlyList<UV> uvs,
            IReadOnlyList<Triangle> triangles,
            Surface face,
            IReadOnlyList<Curve> apiEdges)
        {
            const double spatialTolerance = 1e-2;
            const double requiredRelativeImprovement = 0.003;
            var (normU, normV) = UvScalingUtilities.GetNormalizedUvScales(face);
            var unscaledTriangles = ComputeDelaunayTrianglesInUnscaledUvSpace(uvs);

            var expectedScaledVertices = new List<WorldPoint>(triangles.Count);
            foreach (var t in triangles)
            {
                var (center, _) = t.Circumcircle;
                var unscaledU = center.X / normU;
                var unscaledV = center.Y / normV;
                using var point = face.PointAtParameter(unscaledU, unscaledV);
                AddUniquePoint(expectedScaledVertices, new WorldPoint(point.X, point.Y, point.Z), spatialTolerance);
            }

            var expectedUnscaledVertices = new List<WorldPoint>(unscaledTriangles.Count);
            foreach (var t in unscaledTriangles)
            {
                var (center, _) = t.Circumcircle;
                using var point = face.PointAtParameter(center.X, center.Y);
                AddUniquePoint(expectedUnscaledVertices, new WorldPoint(point.X, point.Y, point.Z), spatialTolerance);
            }

            var apiVertices = new List<WorldPoint>();
            foreach (var edge in apiEdges)
            {
                using var start = edge.StartPoint;
                using var end = edge.EndPoint;

                AddUniquePoint(apiVertices, new WorldPoint(start.X, start.Y, start.Z), spatialTolerance);
                AddUniquePoint(apiVertices, new WorldPoint(end.X, end.Y, end.Z), spatialTolerance);
                edge.Dispose();
            }

            Assert.That(apiVertices.Count, Is.GreaterThan(0));

            var meanDistanceToScaled = apiVertices
                .Select(api => expectedScaledVertices.Min(expected => expected.DistanceTo(api)))
                .Average();

            var meanDistanceToUnscaled = apiVertices
                .Select(api => expectedUnscaledVertices.Min(expected => expected.DistanceTo(api)))
                .Average();

            var relativeImprovement = (meanDistanceToUnscaled - meanDistanceToScaled) /
                                      Math.Max(meanDistanceToUnscaled, 1e-9);

            Assert.That(relativeImprovement, Is.GreaterThanOrEqualTo(requiredRelativeImprovement),
                $"Voronoi dual mismatch. API Voronoi vertices are not sufficiently closer to scaled-dual " +
                $"circumcenters than to unscaled-dual circumcenters. meanScaled={meanDistanceToScaled:F6}, " +
                $"meanUnscaled={meanDistanceToUnscaled:F6}, relativeImprovement={relativeImprovement:F6}, " +
                $"requiredRelativeImprovement={requiredRelativeImprovement:F6}.");
        }

        private static void AssertDelaunayEdgesMatchTriangulation(
            IReadOnlyList<Triangle> triangles,
            Surface face,
            IReadOnlyList<Curve> apiEdges)
        {
            const double spatialTolerance = 1e-3;
            const double minEdgeLength = 0.1;
            var (normU, normV) = UvScalingUtilities.GetNormalizedUvScales(face);

            var expectedEdges = new List<WorldSegment>();
            void AddExpectedEdge(Pt a, Pt b)
            {
                using var start = face.PointAtParameter(a.X / normU, a.Y / normV);
                using var end = face.PointAtParameter(b.X / normU, b.Y / normV);

                if (start.DistanceTo(end) <= minEdgeLength)
                    return;

                AddUniqueSegment(expectedEdges, new WorldSegment(
                    new WorldPoint(start.X, start.Y, start.Z),
                    new WorldPoint(end.X, end.Y, end.Z)), spatialTolerance);
            }

            foreach (var t in triangles)
            {
                AddExpectedEdge(t.A, t.B);
                AddExpectedEdge(t.B, t.C);
                AddExpectedEdge(t.C, t.A);
            }

            var remainingApiEdges = new List<WorldSegment>();
            foreach (var edge in apiEdges)
            {
                using var start = edge.StartPoint;
                using var end = edge.EndPoint;
                AddUniqueSegment(remainingApiEdges, new WorldSegment(
                    new WorldPoint(start.X, start.Y, start.Z),
                    new WorldPoint(end.X, end.Y, end.Z)), spatialTolerance);
            }

            try
            {
                foreach (var expected in expectedEdges)
                {
                    var matchIndex = remainingApiEdges.FindIndex(e => e.Matches(expected, spatialTolerance));
                    if (matchIndex < 0)
                    {
                        Assert.Fail("Expected Delaunay edge was not found in API output.");
                    }

                    remainingApiEdges.RemoveAt(matchIndex);
                }

                if (remainingApiEdges.Count > 0)
                {
                    Assert.Fail("API returned extra Delaunay edges not present in triangulation.");
                }
            }
            finally
            {
                foreach (var edge in apiEdges)
                    edge.Dispose();
            }
        }

        private static void AddUniqueSegment(List<WorldSegment> segments, WorldSegment candidate, double tol)
        {
            if (!segments.Any(existing => existing.Matches(candidate, tol)))
                segments.Add(candidate);
        }

        private static void AddUniquePoint(List<WorldPoint> points, WorldPoint candidate, double tol)
        {
            if (!points.Any(existing => existing.DistanceTo(candidate) <= tol))
                points.Add(candidate);
        }

        private readonly record struct Pt(double X, double Y)
        {
            public double DistanceTo(Pt other)
            {
                var dx = X - other.X;
                var dy = Y - other.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        private readonly record struct Edge(Pt A, Pt B)
        {
            public bool Equals(Edge other) => (A.Equals(other.A) && B.Equals(other.B)) || (A.Equals(other.B) && B.Equals(other.A));
            public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();
        }

        private readonly record struct WorldPoint(double X, double Y, double Z)
        {
            public double DistanceTo(WorldPoint other)
            {
                var dx = X - other.X;
                var dy = Y - other.Y;
                var dz = Z - other.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        private readonly record struct WorldSegment(WorldPoint Start, WorldPoint End)
        {
            public bool Matches(WorldSegment other, double tol)
            {
                var sameDirection = Start.DistanceTo(other.Start) <= tol && End.DistanceTo(other.End) <= tol;
                var reverseDirection = Start.DistanceTo(other.End) <= tol && End.DistanceTo(other.Start) <= tol;
                return sameDirection || reverseDirection;
            }
        }

        private sealed class Triangle
        {
            public Triangle(Pt a, Pt b, Pt c)
            {
                A = a;
                B = b;
                C = c;
                Circumcircle = ComputeCircumcircle(a, b, c);
                Edges = new[] { new Edge(a, b), new Edge(b, c), new Edge(c, a) };
                Vertices = new[] { a, b, c };
            }

            public Pt A { get; }
            public Pt B { get; }
            public Pt C { get; }

            public (Pt center, double radius) Circumcircle { get; }

            public IReadOnlyList<Edge> Edges { get; }
            public IReadOnlyList<Pt> Vertices { get; }

            public bool ContainsVertex(Pt p) => A.Equals(p) || B.Equals(p) || C.Equals(p);

            public bool IsPointInCircumcircle(Pt p, double eps)
            {
                var (center, radius) = Circumcircle;
                var dist = center.DistanceTo(p);
                return dist <= radius - eps;
            }

            private static (Pt center, double radius) ComputeCircumcircle(Pt a, Pt b, Pt c)
            {
                // Compute circumcenter for triangle (a,b,c).
                var ax = a.X;
                var ay = a.Y;
                var bx = b.X;
                var by = b.Y;
                var cx = c.X;
                var cy = c.Y;

                var d = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
                if (Math.Abs(d) <= 1e-20)
                {
                    // Degenerate triangle; return huge circle to avoid false failures.
                    var center = new Pt((ax + bx + cx) / 3.0, (ay + by + cy) / 3.0);
                    return (center, double.PositiveInfinity);
                }

                var ax2ay2 = ax * ax + ay * ay;
                var bx2by2 = bx * bx + by * by;
                var cx2cy2 = cx * cx + cy * cy;

                var ux = (ax2ay2 * (by - cy) + bx2by2 * (cy - ay) + cx2cy2 * (ay - by)) / d;
                var uy = (ax2ay2 * (cx - bx) + bx2by2 * (ax - cx) + cx2cy2 * (bx - ax)) / d;

                var centerPt = new Pt(ux, uy);
                var radius = centerPt.DistanceTo(a);
                return (centerPt, radius);
            }

            public override string ToString() => $"({A})-({B})-({C})";
        }
    }
}
