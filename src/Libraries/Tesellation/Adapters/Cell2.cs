using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using MIConvexHull;

using StarMathLib;

namespace Tessellation.Adapters
{
    /// <summary>
    /// A cell for a 2d tesselation
    /// </summary>
    [SupressImportIntoVM]
    public class Cell2 : TriangulationCell<Vertex2, Cell2>
    {
        Point GetCircumcenter()
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumcircle.html

            var points = Vertices;

            var m = new double[3, 3];

            // x, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = points[i].Position[0];
                m[i, 1] = points[i].Position[1];
                m[i, 2] = 1;
            }
            var a = StarMath.determinant(m);

            // size, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = StarMath.norm2(points[i].Position, true);
            }
            var dx = -StarMath.determinant(m);

            // size, x, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 1] = points[i].Position[0];
            }
            var dy = StarMath.determinant(m);

            // size, x, y
            for (int i = 0; i < 3; i++)
            {
                m[i, 2] = points[i].Position[1];
            }

            var s = -1.0 / (2.0 * a);
            return Point.ByCoordinates(s * dx, s * dy);
        }

        Point GetCentroid()
        {
            return Point.ByCoordinates(Vertices.Select(v => v.Position[0]).Average(), Vertices.Select(v => v.Position[1]).Average());
        }

        Point circumCenter;
        public Point Circumcenter
        {
            get
            {
                return circumCenter = circumCenter ?? GetCircumcenter();
            }
        }

        Point centroid;
        public Point Centroid
        {
            get
            {
                return centroid = centroid ?? GetCentroid();
            }
        }
    }
}