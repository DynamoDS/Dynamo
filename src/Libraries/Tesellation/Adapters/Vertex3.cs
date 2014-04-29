using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using MIConvexHull;

namespace Tessellation.Adapters
{
    internal class Vertex3 : IVertex, IGraphicItem
    {
        public static Vertex3 FromPoint(Point pt)
        {
            return new Vertex3(pt.X, pt.Y, pt.Z);
        }

        public Vector AsVector()
        {
            return Vector.ByCoordinates(Position[0], Position[1], Position[2]);
        }

        public Point AsPoint()
        {
            return Point.ByCoordinates(Position[0], Position[1], Position[2]);
        }

        public Vertex3(double x, double y, double z)
        {
            Position = new[] { x, y, z };
        }

        public double[] Position { get; set; }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            AsVector().Tessellate(package, tol, maxGridLines);
        }

        public double NormSquared()
        {
            var v = AsVector();
            return v.X * v.X + v.Y * v.Y + v.Z * v.Z;

            //return AsVector().ByTwoPoints(AsPoint.Origin(), AsVector).NormalizedSquared();
        }

        public double Norm()
        {
            return Math.Sqrt(NormSquared());
        }
    }
}