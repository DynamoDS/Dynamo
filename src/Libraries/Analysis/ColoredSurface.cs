using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

using DSCore;

using MIConvexHull;

using Point = Autodesk.DesignScript.Geometry.Point;

namespace Analysis
{
    // This is a duplication of the Vertex2 class in the Tesselation
    // library. We need to re-implement here in order to avoid marking 
    // that class as public.
    internal class Vertex2 : IVertex, IGraphicItem
    {
        public object Tag { get; set; }

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

    internal class Vertex2EqualityComparer : IEqualityComparer<Vertex2>
    {
        public bool Equals(Vertex2 x, Vertex2 y)
        {
            var v1 = x.AsVector();
            var v2 = y.AsVector();

            return v1.IsAlmostEqualTo(v2);
        }

        public int GetHashCode(Vertex2 obj)
        {
            int hCode = obj.Position[0].GetHashCode() ^ obj.Position[1].GetHashCode();
            return hCode.GetHashCode();
        }
    }

    // This is a duplication of the Cell2 class in the Tesselation.
    // library. We need to re-implement here in order to avoid marking 
    // that class as public.
    internal class Cell2 : TriangulationCell<Vertex2, Cell2>
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
                m[i, 0] = StarMath.norm2(points[i].Position, 2, true);
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

    public class ColoredSurface : IGraphicItem
    {
        private Surface surface;
        private Color[] colors;
        private UV[] uvs;

        private ColoredSurface(Surface surface, 
            DSCore.Color[] colors, UV[] uvs)
        {
            this.surface = surface;
            this.colors = colors;
            this.uvs = uvs;
        }

        public static ColoredSurface ByColorsAndUVs(Surface surface, DSCore.Color[] colors, UV[] uvs)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (!colors.Any())
            {
                throw new ArgumentException("There are no colors specified.");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (!uvs.Any())
            {
                throw new ArgumentException("There are no UVs specified.");
            }

            if (uvs.Count() != colors.Count())
            {
                throw new Exception("The number of colors and the number of locations specified must be equal.");
            }

            return new ColoredSurface(surface, colors, uvs);
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            // Use ASM's tesselation routine to tesselate
            // the surface. Tesselate with a high degree 
            // of precision to ensure that UVs can be matched 
            // to vertices.
            surface.Tessellate(package, 0.01);

            int colorCount = 0;
            for (int i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                var vx = package.TriangleVertices[i];
                var vy = package.TriangleVertices[i + 1];
                var vz = package.TriangleVertices[i + 2];

                // Get the triangle vertex
                var v = Point.ByCoordinates(vx, vy, vz);
                var uv = surface.UVParameterAtPoint(v);
                var avgColor = Color.BuildColorFrom2DRange(colors, uvs, uv);

                package.TriangleVertexColors[colorCount] = avgColor.Red;
                package.TriangleVertexColors[colorCount + 1] = avgColor.Green;
                package.TriangleVertexColors[colorCount + 2] = avgColor.Blue;
                package.TriangleVertexColors[colorCount + 3] = avgColor.Alpha;

                colorCount += 4;
            }
        }

        private Color CalculateColorDistance(UV uv)
        {
            // The distances from this to each of the calculation points
            var distances = new double[uvs.Count()];
            var maxDistance = 0.0;
            for (int k = 0; k < uvs.Count(); k++)
            {
                var uvTest = uvs[k];
                var d =
                    System.Math.Sqrt(
                        System.Math.Pow(uvTest.U - uv.U, 2) + System.Math.Pow(uvTest.V - uv.V, 2));
                distances[k] = d;

                if (d > maxDistance)
                    maxDistance = d;
            }

            var colorContributions = new List<DSCore.Color>();

            for (int j = 0; j < colors.Count(); j++)
            {
                var c = colors[j];

                var color = Color.Divide(colors[j],System.Math.Pow(distances[j] + 1,2));
                colorContributions.Add(color);
            }

            var a = 255;
            var r = 255;
            var g = 255;
            var b = 255;

            foreach (var c in colorContributions)
            {
                a += c.Alpha;
                r += c.Red;
                g += c.Green;
                b += c.Blue;
            }

            var size = colors.Count() + 1;
            return DSCore.Color.ByARGB(255, r/size, g/size, b/size);
        }
    }
}
