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
        internal Vertex(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            nx = ny = 0.0;
            nz = 1.0;
            a = r = g = b = 255;
        }

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

    public class HeightMap
    {
        private int size = 2;
        private double gap = 10.0;
        private Triangles triangles = null;

        #region Public Class Operational Methods

        public static HeightMap BySize(int size, double gap)
        {
            return new HeightMap(size, gap);
        }

        public Triangles Triangles
        {
            get { return null; }
        }

        #endregion

        #region Private Class Helper Methods

        private HeightMap(int size, double gap)
        {
            this.size = size;
            this.gap = gap;
            this.triangles = GenerateTriangles(size, gap);
        }

        private static Triangles GenerateTriangles(int size, double gap)
        {
            Random random = new Random();

            int xDivisions = size, yDivisions = size;

            xDivisions = ((xDivisions > 0) ? xDivisions : 1);
            yDivisions = ((yDivisions > 0) ? yDivisions : 1);
            Vertex[,] vertices = new Vertex[yDivisions + 1, xDivisions + 1];

            for (int yDiv = 0; yDiv <= yDivisions; ++yDiv)
            {
                for (int xDiv = 0; xDiv <= xDivisions; ++xDiv)
                {
                    var x = xDiv * gap;
                    var y = yDiv * gap;
                    var z = random.Next(size) * gap;

                    var vertex = new Vertex(x, y, z);
                    vertices[yDiv, xDiv] = vertex;
                }
            }

            var triangles = new Triangles(yDivisions * xDivisions * 2);
            for (int vDiv = 0; vDiv < yDivisions; ++vDiv)
            {
                for (int uDiv = 0; uDiv < xDivisions; ++uDiv)
                {
                    triangles.PushQuad(
                        vertices[vDiv + 0, uDiv + 0],
                        vertices[vDiv + 0, uDiv + 1],
                        vertices[vDiv + 1, uDiv + 0],
                        vertices[vDiv + 1, uDiv + 1]);
                }
            }

            return triangles;
        }

        #endregion
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

        internal void ComputeNormals()
        {
            // The same number of entries in "normals" as "vertices".
            normals.Capacity = vertices.Capacity;

            for (int i = 0; i < vertices.Count; i = i + 9)
            {
                double ux = vertices[i + 3] - vertices[i + 0];
                double uy = vertices[i + 4] - vertices[i + 1];
                double uz = vertices[i + 5] - vertices[i + 2];

                double vx = vertices[i + 6] - vertices[i + 0];
                double vy = vertices[i + 7] - vertices[i + 1];
                double vz = vertices[i + 8] - vertices[i + 2];

                double nx = uy * vz - uz * vy;
                double ny = uz * vx - ux * vz;
                double nz = ux * vy - uy * vx;

                // Push one normal per vertex.
                normals[i + 0] = normals[i + 3] = normals[i + 6] = nx;
                normals[i + 1] = normals[i + 4] = normals[i + 7] = ny;
                normals[i + 2] = normals[i + 5] = normals[i + 8] = nz;
            }
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

            triangles.ComputeNormals();
            return triangles;
        }
    }
}
