using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;
using Tessellation.Adapters;

namespace Tessellation
{
    /// <summary>
    ///     Functions for creating convex hulls.
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        ///     Creates a convex hull of a set of points.
        /// </summary>
        /// <param name="points">A set of points.</param>
        public static IEnumerable<Curve> ByPoints(IEnumerable<Point> points)
        {
            var verts = points.Select(Vertex3.FromPoint).ToList();
            var triResult = ConvexHull<Vertex3, TriangleFace>.Create(verts, new ConvexHullComputationConfig());

            // make edges
            foreach (var face in triResult.Faces)
            {
                // form lines for use in dynamo or revit
                var start1 = face.Vertices[0].AsPoint();
                var end1 = face.Vertices[1].AsPoint();

                var start2 = face.Vertices[1].AsPoint();
                var end2 = face.Vertices[2].AsPoint();

                var start3 = face.Vertices[2].AsPoint();
                var end3 = face.Vertices[0].AsPoint();

                if (start1.DistanceTo(end1) > 0.1)
                {
                    var l1 = Line.ByStartPointEndPoint(start1, end1);
                    yield return l1;
                }

                if (start2.DistanceTo(end2) > 0.1)
                {
                    var l1 = Line.ByStartPointEndPoint(start2, end2);
                    yield return l1;
                }

                if (start3.DistanceTo(end3) > 0.1)
                {
                    var l1 = Line.ByStartPointEndPoint(start3, end3);
                    yield return l1;
                }
            }
        }
    }
}
