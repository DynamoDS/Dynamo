using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tessellation
{
    struct Vertex
    {
        internal Vertex(Point point, Vector normal)
        {
            x = point.X;
            y = point.Y;
            z = point.Z;
            nx = normal.X;
            ny = normal.Y;
            nz = normal.Z;
            a = r = g = b = 255;
        }

        internal double x, y, z;
        internal double nx, ny, nz;
        internal byte a, r, g, b;
    }

    public class Triangles : IGraphicItem
    {
        private List<double> vertices = new List<double>();
        private List<double> normals = new List<double>();
        private List<byte> colors = new List<byte>();

        internal Triangles(int triangleCount)
        {
            vertices.Capacity = triangleCount * 3 * 3;
            normals.Capacity = triangleCount * 3 * 3;
            colors.Capacity = triangleCount * 3 * 4;
        }

        internal void PushQuad(Vertex lt, Vertex rt, Vertex lb, Vertex rb)
        {
            PushVertex(lt);
            PushVertex(rt);
            PushVertex(lb);
            PushVertex(lb);
            PushVertex(rt);
            PushVertex(rb);
        }

        internal void PushVertex(Vertex vertex)
        {
            vertices.Add(vertex.x);
            vertices.Add(vertex.y);
            vertices.Add(vertex.z);

            normals.Add(vertex.nx);
            normals.Add(vertex.ny);
            normals.Add(vertex.nz);

            colors.Add(vertex.a);
            colors.Add(vertex.r);
            colors.Add(vertex.g);
            colors.Add(vertex.b);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            for (int i = 0; i < vertices.Count; i = i + 3)
            {
                package.PushTriangleVertex(
                    vertices[i], vertices[i + 1], vertices[i + 2]);
            }

            for (int i = 0; i < normals.Count; i = i + 3)
            {
                package.PushTriangleVertexNormal(
                    normals[i], normals[i + 1], normals[i + 2]);
            }

            for (int i = 0; i < colors.Count; i = i + 4)
            {
                package.PushTriangleVertexColor(
                    colors[i], colors[i + 1], colors[i + 2], colors[i + 3]);
            }
        }
    }

    public class Visualization
    {
        public static Triangles Tessellate(Surface surface, int uDivisions, int vDivisions)
        {
            if (surface == null)
                throw new ArgumentNullException("surface");

            uDivisions = ((uDivisions > 0) ? uDivisions : 1);
            vDivisions = ((vDivisions > 0) ? vDivisions : 1);
            Vertex[,] vertices = new Vertex[vDivisions + 1, uDivisions + 1];

            var uSize = 1.0 / uDivisions;
            var vSize = 1.0 / vDivisions;

            for (int vDiv = 0; vDiv <= vDivisions; ++vDiv)
            {
                for (int uDiv = 0; uDiv <= uDivisions; ++uDiv)
                {
                    var u = uSize * uDiv;
                    var v = vSize * vDiv;
                    var point = surface.PointAtParameter(u, v);
                    var normal = surface.NormalAtParameter(u, v);

                    var vertex = new Vertex(point, normal);
                    vertices[vDiv, uDiv] = vertex;
                }
            }

            var triangles = new Triangles(vDivisions * uDivisions * 2);
            for (int vDiv = 0; vDiv < vDivisions; ++vDiv)
            {
                for (int uDiv = 0; uDiv < uDivisions; ++uDiv)
                {
                    triangles.PushQuad(
                        vertices[vDiv, uDiv],
                        vertices[vDiv, uDiv + 1],
                        vertices[vDiv + 1, uDiv],
                        vertices[vDiv + 1, uDiv + 1]);
                }
            }

            return triangles;
        }
    }
}
