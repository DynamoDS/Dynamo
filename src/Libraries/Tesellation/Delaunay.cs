using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;
using Tessellation.Adapters;

namespace Tessellation
{
    /// <summary>
    ///     Functions for creating Delaunay triangulations.
    /// </summary>
    public static class Delaunay
    {
        /// <summary>
        ///     Creates a Delaunay triangulation of a surface with a given set of UV parameters.
        /// </summary>
        /// <param name="uvs">Set of UV parameters.</param>
        /// <param name="face">Surface to triangulate.</param>
        /// <search>uvs</search>
        public static IEnumerable<Curve> ByParametersOnSurface(IEnumerable<UV> uvs, Surface face)
        {
            var uvList = uvs?.ToList();
            if (uvList == null || uvList.Count == 0 || face == null)
                yield break;

            // Get normalized UV scaling factors to handle anisotropic parameter spaces
            var (normU, normV, maxPhysicalScale) = UvScalingUtilities.GetNormalizedUvScales(face);

            // Minimum edge length as a fraction of the surface's dominant physical dimension,
            // so the filter scales correctly regardless of scene units.
            var minEdgeLength = maxPhysicalScale * 1e-3;

            // Clamp input UVs to [0,1] before scaling to guard against out-of-range user input.
            var verts = uvList
                .Select(uv => new Vertex2(
                    Math.Clamp(uv.U, 0.0, 1.0) * normU,
                    Math.Clamp(uv.V, 0.0, 1.0) * normV))
                .ToList();

            const double PlaneDistanceTolerance = 1e-6;
            var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts, PlaneDistanceTolerance);

            // Map each Vertex2 to its index in verts so we can form canonical edge keys.
            // MIConvexHull reuses the same object references in cell.Vertices, so reference
            // equality (the default for class types) gives correct index lookups.
            var vertexIndex = new Dictionary<Vertex2, int>(verts.Count);
            for (int i = 0; i < verts.Count; i++)
                vertexIndex[verts[i]] = i;

            // Track emitted edges as canonical (minIndex, maxIndex) pairs to avoid
            // emitting interior edges twice (once per adjacent triangle cell).
            var emittedEdges = new HashSet<(int, int)>();

            // there are three vertices per cell in 2D
            foreach (var cell in triangulation.Cells)
            {
                var cv0 = cell.Vertices[0];
                var cv1 = cell.Vertices[1];
                var cv2 = cell.Vertices[2];

                int i0 = vertexIndex[cv0];
                int i1 = vertexIndex[cv1];
                int i2 = vertexIndex[cv2];

                bool emit01 = emittedEdges.Add((Math.Min(i0, i1), Math.Max(i0, i1)));
                bool emit12 = emittedEdges.Add((Math.Min(i1, i2), Math.Max(i1, i2)));
                bool emit20 = emittedEdges.Add((Math.Min(i2, i0), Math.Max(i2, i0)));

                if (!emit01 && !emit12 && !emit20)
                    continue;

                // Project scaled UV coordinates back to surface points.
                // Dispose intermediate Points once the Lines have been constructed.
                using var xyz0 = face.PointAtParameter(cv0.Position[0] / normU, cv0.Position[1] / normV);
                using var xyz1 = face.PointAtParameter(cv1.Position[0] / normU, cv1.Position[1] / normV);
                using var xyz2 = face.PointAtParameter(cv2.Position[0] / normU, cv2.Position[1] / normV);

                if (emit01 && xyz0.DistanceTo(xyz1) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(xyz0, xyz1);

                if (emit12 && xyz1.DistanceTo(xyz2) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(xyz1, xyz2);

                if (emit20 && xyz2.DistanceTo(xyz0) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(xyz2, xyz0);
            }
        }

        /// <summary>
        ///     Creates a Delaunay triangulation of a set of points.
        /// </summary>
        /// <param name="points">A set of points.</param>
        public static IEnumerable<Curve> ByPoints(IEnumerable<Point> points)
        {
            var verts = points.Select(Vertex3.FromPoint).ToList();
            const double PlaneDistanceTolerance = 1e-6;
            var triResult = DelaunayTriangulation<Vertex3, Tetrahedron>.Create(verts, PlaneDistanceTolerance);

            // Map each Vertex3 to its global index for canonical edge deduplication across tetrahedra.
            var vertexIndex = new Dictionary<Vertex3, int>(verts.Count);
            for (int i = 0; i < verts.Count; i++)
                vertexIndex[verts[i]] = i;

            // Tetrahedra share faces, so iterating all faces of all cells would emit shared
            // edges multiple times. Track canonical (minIndex, maxIndex) pairs to emit each once.
            var emittedEdges = new HashSet<(int, int)>();

            foreach (var cell in triResult.Cells)
            {
                foreach (var faceIndices in cell.MakeFaces())
                {
                    var edgePairs = new[]
                    {
                        (faceIndices[0], faceIndices[1]),
                        (faceIndices[1], faceIndices[2]),
                        (faceIndices[2], faceIndices[0])
                    };

                    foreach (var (a, b) in edgePairs)
                    {
                        int gi_a = vertexIndex[cell.Vertices[a]];
                        int gi_b = vertexIndex[cell.Vertices[b]];

                        if (!emittedEdges.Add((Math.Min(gi_a, gi_b), Math.Max(gi_a, gi_b))))
                            continue;

                        using var start = cell.Vertices[a].AsPoint();
                        using var end = cell.Vertices[b].AsPoint();
                        yield return Line.ByStartPointEndPoint(start, end);
                    }
                }
            }
        }
    }
}
