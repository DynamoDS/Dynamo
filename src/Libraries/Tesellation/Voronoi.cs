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

            // Physical scale per unit U/V (affine for planar Rectangle->Surface.ByPatch)
            var p00 = face.PointAtParameter(0, 0);
            var p10 = face.PointAtParameter(1, 0);
            var p01 = face.PointAtParameter(0, 1);

            var scaleU = p00.DistanceTo(p10);
            var scaleV = p00.DistanceTo(p01);

            // Normalize scales to keep values in a reasonable range, preserve aspect ratio
            var max = System.Math.Max(scaleU, scaleV);
            if (max <= 1e-9) max = 1.0;
            var normU = scaleU / max;
            var normV = scaleV / max;
            if (normU <= 1e-9) normU = 1.0;
            if (normV <= 1e-9) normV = 1.0;

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

                var start = face.PointAtParameter(u1, v1);
                var end = face.PointAtParameter(u2, v2);

                if (start.DistanceTo(end) > 0.1)
                    yield return Line.ByStartPointEndPoint(start, end);
            }
        }
    }
}
