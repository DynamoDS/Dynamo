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
            var (normU, normV, minPhysicalScale) = UvScalingUtilities.GetNormalizedUvScales(face);

            // Minimum edge length threshold in world units: 0.1% of the shorter physical dimension
            // of the surface. Filters degenerate near-zero edges from duplicate or near-duplicate
            // input UV points without ever affecting valid geometry under normal usage.
            var minEdgeLength = minPhysicalScale * 1e-3;

            // Clamp input UVs to [0,1] before scaling to guard against out-of-range user input.
            var verts = uvList
                .Select(uv => new Vertex2(
                    Math.Clamp(uv.U, 0.0, 1.0) * normU,
                    Math.Clamp(uv.V, 0.0, 1.0) * normV))
                .ToList();

            const double PlaneDistanceTolerance = 1e-6;
            var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts, PlaneDistanceTolerance);

            // there are three vertices per cell in 2D
            foreach (var cell in triangulation.Cells)
            {
                var cv0 = cell.Vertices[0];
                var cv1 = cell.Vertices[1];
                var cv2 = cell.Vertices[2];

                // Project scaled UV coordinates back to surface points.
                // Dispose intermediate Points once the Lines have been constructed.
                using var xyz0 = face.PointAtParameter(cv0.Position[0] / normU, cv0.Position[1] / normV);
                using var xyz1 = face.PointAtParameter(cv1.Position[0] / normU, cv1.Position[1] / normV);
                using var xyz2 = face.PointAtParameter(cv2.Position[0] / normU, cv2.Position[1] / normV);

                if (xyz0.DistanceTo(xyz1) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(xyz0, xyz1);

                if (xyz1.DistanceTo(xyz2) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(xyz1, xyz2);

                if (xyz2.DistanceTo(xyz0) > minEdgeLength)
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

            // make edges
            foreach (var cell in triResult.Cells)
            {
                foreach (var face in cell.MakeFaces())
                {
                    using var start1 = cell.Vertices[face[0]].AsPoint();
                    using var end1 = cell.Vertices[face[1]].AsPoint();

                    using var start2 = cell.Vertices[face[1]].AsPoint();
                    using var end2 = cell.Vertices[face[2]].AsPoint();

                    using var start3 = cell.Vertices[face[2]].AsPoint();
                    using var end3 = cell.Vertices[face[0]].AsPoint();

                    yield return Line.ByStartPointEndPoint(start1, end1);
                    yield return Line.ByStartPointEndPoint(start2, end2);
                    yield return Line.ByStartPointEndPoint(start3, end3);
                }
            }
        }
    }
}
