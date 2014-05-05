using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;

namespace Tessellation.Adapters
{
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    internal class Tetrahedron : TriangulationCell<Vertex3, Tetrahedron>
    {
        /// <summary>
        /// Helper function to get the position of the i-th vertex.
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Position of the i-th vertex</returns>
        Vector GetPosition(int i)
        {
            return Vertices[i].AsVector();
        }

        /// <summary>
        /// This function adds indices for a triangle representing the face.
        /// The order is in the CCW (counter clock wise) order so that the automatically calculated normals point in the right direction.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="center"></param>
        int[] MakeFace(int i, int j, int k, Vector center)
        {
            var u = GetPosition(j).Subtract(GetPosition(i));
            var v = GetPosition(k).Subtract(GetPosition(j));

            // compute the normal and the plane corresponding to the side [i,j,k]
            var n = u.Cross(v);
            var d = -(n.Dot(center));

            // check if the normal faces towards the center
            var t = n.Dot(GetPosition(i)) + d;

            // swapping indices j and k also changes the sign of the normal, because cross product is anti-commutative
            return t >= 0 ? new[] { k, j, i } : new[] { i, j, k };
        }

        /// <summary>
        /// Creates a model of the tetrahedron. Transparency is applied to the color.
        /// </summary>
        /// <returns>A model representing the tetrahedron</returns>
        public int[][] MakeFaces()
        {
            var points = Enumerable.Range(0, 4).Select(GetPosition);

            var center = points.Aggregate(Vector.ByCoordinates(0, 0, 0), (a, c) => a.Add(c)).Scale(1/4);

            var indices = new int[4][];
            indices[0] = MakeFace(0, 1, 2, center);
            indices[1] = MakeFace(0, 1, 3, center);
            indices[2] = MakeFace(0, 2, 3, center);
            indices[3] = MakeFace(1, 2, 3, center);

            return indices;
        }
    }
}
