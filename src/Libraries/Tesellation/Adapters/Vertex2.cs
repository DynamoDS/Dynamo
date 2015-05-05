using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using MIConvexHull;

namespace Tessellation.Adapters
{
    [SupressImportIntoVM]
    public class Vertex2 : IVertex, IGraphicItem
    {
        public static Vertex2 FromUV(UV uv)
        {
            return new Vertex2(uv.U, uv.V);
        }

        public Vector AsVector()
        {
            return Vector.ByCoordinates(Position[0], Position[1], 0);
        }

        public Point AsPoint()
        {
            return Point.ByCoordinates(Position[0], Position[1]);
        }

        public Vertex2(double x, double y)
        {
            Position = new[] { x, y };
        }

        public double[] Position { get; set; }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            AsVector().Tessellate(package, tol, maxGridLines);
        }
    }
}