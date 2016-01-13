using System;
using System.Linq;
using MIConvexHull;

using StarMathLib;

namespace Tessellation.Adapters
{
    /// <summary>
    /// A cell for a 3d tesselation
    /// </summary>
    internal class Cell3 : TriangulationCell<Vertex3, Cell3>
    {
        Vertex3 GetCircumcenter()
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumsphere.html

            if (Vertices.Count() != 4)
                throw new Exception("Malformed voronoi cell");

            var points = Vertices;
            var norms = points.Select(pt => pt.NormSquared()).ToList();

            var aM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                aM[i, 0] = pt.Position[0];
                aM[i, 1] = pt.Position[1];
                aM[i, 2] = pt.Position[2];
                aM[i, 3] = 1;
            }

            double a2 = StarMath.determinant(aM) * 2;

            var dxM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                dxM[i, 0] = norms[i];
                dxM[i, 1] = pt.Position[1];
                dxM[i, 2] = pt.Position[2];
                dxM[i, 3] = 1;
            }

            double dx = StarMath.determinant(dxM);

            var dyM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                dyM[i, 0] = norms[i];
                dyM[i, 1] = pt.Position[0];
                dyM[i, 2] = pt.Position[2];
                dyM[i, 3] = 1;
            }

            double dy = -StarMath.determinant(dyM);

            var dzM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                dzM[i, 0] = norms[i];
                dzM[i, 1] = pt.Position[0];
                dzM[i, 2] = pt.Position[1];
                dzM[i, 3] = 1;
            }

            double dz = StarMath.determinant(dzM);

            // used for obtaining circumradius
            //double[,] cM = new double[4, 4];

            //for (var i = 0; i < 4; i++)
            //{
            //    var pt = points[i];

            //    cM[i, 0] = norms[i];
            //    cM[i, 1] = pt.Position[0];
            //    cM[i, 2] = pt.Position[1];
            //    cM[i, 3] = pt.Position[2];
            //}

            return new Vertex3(dx / a2, dy / a2, dz / a2);

        }

        private Vertex3 circumCenter;
        public Vertex3 Circumcenter
        {
            get
            {
                return circumCenter = circumCenter ?? GetCircumcenter();
            }
        }
    }
}