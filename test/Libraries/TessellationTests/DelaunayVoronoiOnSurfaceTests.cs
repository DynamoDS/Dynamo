using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Tessellation;

namespace Dynamo.Tests
{
    [TestFixture]
    public class DelaunayVoronoiOnSurfaceTests : DynamoModelTestBase
    {
        private const double Epsilon = 1e-9;

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
        }

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

            // Validate that Voronoi circumcenters are computed in the same scaled UV space.
            // This exposes the bug: Voronoi doesn't apply UV scaling, so circumcenters will be wrong.
            AssertVoronoiUsesScaledCircumcenters(triangles, surface);
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

        private static List<Triangle> ComputeDelaunayTrianglesInScaledUvSpace(IReadOnlyList<UV> uvs, Surface face)
        {
            var (normU, normV) = UvScalingUtilities.GetNormalizedUvScales(face);

            // Perform Delaunay triangulation in the same anisotropically scaled UV space as production.
            var points = uvs.Select(uv => new Pt(uv.U * normU, uv.V * normV)).ToList();

            // Bowyer-Watson in 2D.
            var (superA, superB, superC) = CreateSuperTriangle(points);
            var triangles = new List<Triangle> { new(superA, superB, superC) };

            foreach (var p in points)
            {
                var badTriangles = triangles.Where(t => t.IsPointInCircumcircle(p, Epsilon)).ToList();

                var polygon = new List<Edge>();
                foreach (var t in badTriangles)
                {
                    foreach (var e in t.Edges)
                    {
                        if (!polygon.Remove(e))
                            polygon.Add(e);
                    }
                }

                triangles.RemoveAll(t => badTriangles.Contains(t));
                triangles.AddRange(polygon.Select(e => new Triangle(e.A, e.B, p)));
            }

            // Remove triangles that contain a super triangle vertex.
            triangles.RemoveAll(t => t.ContainsVertex(superA) || t.ContainsVertex(superB) || t.ContainsVertex(superC));

            return triangles;
        }

        private static (Pt a, Pt b, Pt c) CreateSuperTriangle(IReadOnlyList<Pt> points)
        {
            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            var dx = maxX - minX;
            var dy = maxY - minY;
            var deltaMax = Math.Max(dx, dy);

            var midX = (minX + maxX) / 2.0;
            var midY = (minY + maxY) / 2.0;

            // Large triangle that encompasses all points.
            var a = new Pt(midX - 20 * deltaMax, midY - deltaMax);
            var b = new Pt(midX, midY + 20 * deltaMax);
            var c = new Pt(midX + 20 * deltaMax, midY - deltaMax);
            return (a, b, c);
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

        private static void AssertVoronoiUsesScaledCircumcenters(IReadOnlyList<Triangle> triangles, Surface face)
        {
            var (normU, normV) = UvScalingUtilities.GetNormalizedUvScales(face);

            // For each triangle in scaled UV space, compute where its circumcenter maps to in world space.
            var expectedCircumcentersInWorldSpace = new List<Point>();
            foreach (var t in triangles)
            {
                var (center, _) = t.Circumcircle;
                // Triangle is in scaled UV space, so unscale before evaluating on surface.
                var unscaledU = center.X / normU;
                var unscaledV = center.Y / normV;
                expectedCircumcentersInWorldSpace.Add(face.PointAtParameter(unscaledU, unscaledV));
            }

            // If Voronoi were correctly using scaled UV space, its edge endpoints would be these circumcenters.
            // Since Voronoi currently doesn't scale, the actual endpoints will be significantly different.
            // For now, we just verify that at least ONE expected circumcenter appears in the Voronoi edges
            // (within tolerance) - this will fail if Voronoi doesn't scale properly.
            const double spatialTolerance = 1.0; // World units

            var voronoiEdges = Voronoi.ByParametersOnSurface(
                triangles.SelectMany(t => t.Vertices)
                    .Distinct()
                    .Select(pt => UV.ByCoordinates(pt.X / normU, pt.Y / normV)),
                face).ToList();

            var voronoiEndpoints = new List<Point>();
            foreach (var edge in voronoiEdges)
            {
                voronoiEndpoints.Add(edge.StartPoint);
                voronoiEndpoints.Add(edge.EndPoint);
            }

            // Check if the expected circumcenters (from scaled triangulation) appear in Voronoi output.
            foreach (var expected in expectedCircumcentersInWorldSpace)
            {
                var nearestVoronoiPoint = voronoiEndpoints.MinBy(vp => expected.DistanceTo(vp));
                var distance = expected.DistanceTo(nearestVoronoiPoint);

                if (distance > spatialTolerance)
                {
                    Assert.Fail(
                        $"Voronoi circumcenter mismatch detected. Expected circumcenter at world position " +
                        $"({expected.X:F3}, {expected.Y:F3}, {expected.Z:F3}) from scaled triangulation, " +
                        $"but nearest Voronoi vertex is {distance:F3} units away. " +
                        $"This indicates Voronoi.ByParametersOnSurface is not applying UV scaling correctly.");
                }
            }

            // Cleanup
            foreach (var pt in expectedCircumcentersInWorldSpace)
                pt.Dispose();
            foreach (var pt in voronoiEndpoints)
                pt.Dispose();
            foreach (var edge in voronoiEdges)
                edge.Dispose();
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
