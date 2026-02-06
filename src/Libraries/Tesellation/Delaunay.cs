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

            // Build Delaunay in anisotropically scaled (u*normU, v*normV) space.
            var verts = uvList.Select(uv => new Vertex2(uv.U * normU, uv.V * normV)).ToList();

            const double PlaneDistanceTolerance = 1e-6;
            var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts, PlaneDistanceTolerance);

            // there are three vertices per cell in 2D
            foreach (var cell in triangulation.Cells)
            {
                var v1 = cell.Vertices[0].AsVector();
                var v2 = cell.Vertices[1].AsVector();
                var v3 = cell.Vertices[2].AsVector();

                var xyz1 = face.PointAtParameter(v1.X / normU, v1.Y / normV);
                var xyz2 = face.PointAtParameter(v2.X / normU, v2.Y / normV);
                var xyz3 = face.PointAtParameter(v3.X / normU, v3.Y / normV);

                if (xyz1.DistanceTo(xyz2) > 0.1)
                {
                    var l1 = Line.ByStartPointEndPoint(xyz1, xyz2);
                    yield return l1;
                }

                if (xyz2.DistanceTo(xyz3) > 0.1)
                {
                    var l2 = Line.ByStartPointEndPoint(xyz3, xyz2);
                    yield return l2;
                }

                if (xyz3.DistanceTo(xyz1) > 0.1)
                {
                    var l3 = Line.ByStartPointEndPoint(xyz1, xyz3);
                    yield return l3;
                }
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
                    var start1 = cell.Vertices[face[0]].AsPoint();
                    var end1 = cell.Vertices[face[1]].AsPoint();

                    var start2 = cell.Vertices[face[1]].AsPoint();
                    var end2 = cell.Vertices[face[2]].AsPoint();

                    var start3 = cell.Vertices[face[2]].AsPoint();
                    var end3 = cell.Vertices[face[0]].AsPoint();

                    var l1 = Line.ByStartPointEndPoint(start1, end1);
                    yield return l1;

                    var l2 = Line.ByStartPointEndPoint(start2, end2);
                    yield return l2;

                    var l3 = Line.ByStartPointEndPoint(start3, end3);
                    yield return l3;
                }
            }
        }
    }
}
