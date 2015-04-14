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
        public static IEnumerable<Curve> ByParametersOnSurface(IEnumerable<UV> uvs, Surface face)
        {
            var verts = uvs.Select(Vertex2.FromUV).ToList();
            var voronoiMesh = VoronoiMesh.Create<Vertex2, Cell2>(verts);

            return from edge in voronoiMesh.Edges
                   let _from = edge.Source.Circumcenter
                   let to = edge.Target.Circumcenter
                   let start = face.PointAtParameter(_from.X, _from.Y)
                   let end = face.PointAtParameter(to.X, to.Y)
                   where start.DistanceTo(end) > 0.1
                   select Line.ByStartPointEndPoint(start, end);
        }
    }
}