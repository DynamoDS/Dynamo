using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;
using Tessellation.Adapters;

namespace Tessellation
{
    /// <summary>
    ///     Functions for creating Voronoi tesselations.
    /// </summary>
    public static class Voronoi
    {
        /// <summary>
        ///     Creates a Voronoi tessellation of a surface with a given set of UV parameters.
        /// </summary>
        /// <param name="uvs">Set of UV parameters.</param>
        /// <param name="face">Surface to tesselate.</param>
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

            // Anisotropic scaling only by aspect ratio
            var verts = uvList.Select(uv => new Vertex2(uv.U * normU, uv.V * normV)).ToList();
            var voronoiMesh = VoronoiMesh.Create<Vertex2, Cell2>(verts);

            foreach (var edge in voronoiMesh.Edges)
            {
                // Map circumcenters back to UV
                var cFrom = edge.Source.Circumcenter;
                var cTo = edge.Target.Circumcenter;

                var u1 = cFrom.X / normU;
                var v1 = cFrom.Y / normV;
                var u2 = cTo.X / normU;
                var v2 = cTo.Y / normV;

                // Discard edges where both circumcenters lie outside the UV domain [0,1].
                // These are purely external Voronoi rays with no valid surface intersection.
                static bool IsOutsideDomain(double u, double v) =>
                    u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0;

                if (IsOutsideDomain(u1, v1) && IsOutsideDomain(u2, v2))
                    continue;

                // Clamp each endpoint to the valid UV domain before projecting onto the surface.
                // This truncates edges that cross the surface boundary rather than extrapolating.
                u1 = Math.Clamp(u1, 0.0, 1.0);
                v1 = Math.Clamp(v1, 0.0, 1.0);
                u2 = Math.Clamp(u2, 0.0, 1.0);
                v2 = Math.Clamp(v2, 0.0, 1.0);

                var start = face.PointAtParameter(u1, v1);
                var end = face.PointAtParameter(u2, v2);

                if (start.DistanceTo(end) > minEdgeLength)
                    yield return Line.ByStartPointEndPoint(start, end);
            }
        }
    }
}
