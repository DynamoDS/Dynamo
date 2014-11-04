using System;
using System.Diagnostics;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

using DSCore;

using Math = System.Math;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Analysis
{
    public class ColoredSurface : IGraphicItem
    {
        private Surface surface;
        private DSCore.Color[] colors;
        private UV[] uvs;

        private ColoredSurface(Surface surface, DSCore.Color[] colors, UV[] uvs)
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
            // As we write the vertex locations to the render package,
            // calculate the UV location for the vertex and 
            surface.Tessellate(package);

            var colorCount = 0;

            for (int i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                var vx = package.TriangleVertices[i];
                var vy = package.TriangleVertices[i + 1];
                var vz = package.TriangleVertices[i + 2];

                // Get the triangle vertex
                var v = Point.ByCoordinates(vx, vy, vz);

                var an = package.TriangleNormals[i];
                var bn = package.TriangleNormals[i + 1];
                var cn = package.TriangleNormals[i + 2];

                var norm = Vector.ByCoordinates(an, bn, cn);
                var xsects = surface.ProjectInputOnto(v, norm);

                if (!xsects.Any()) continue;

                // The parameter at the triangle vertex
                var vUV = surface.UVParameterAtPoint(xsects.First() as Point);

                // The distances from this to each of the calculation points
                var distances = new double[uvs.Count()];
                for (int k=0; k<uvs.Count(); k++)
                {
                    var uv = uvs[k];
                    var d = Math.Sqrt(Math.Pow(uv.U - vUV.U, 2) + Math.Pow(uv.V - vUV.V, 2));
                    distances[k] = d;
                }

                // Calculate the averages of all 
                // color components
                var a = 0.0;
                var r = 0.0;
                var g = 0.0;
                var b = 0.0;

                var totalWeight = 0.0;

                for (int j = 0; j < colors.Count(); j++)
                {
                    var c = colors[j];
                    var d = distances[j];

                    a += c.Alpha * d;
                    r += c.Red * d;
                    g += c.Green * d;
                    b += c.Blue * d;

                    totalWeight += d;
                }

                var totalR = (byte)(r/totalWeight);
                var totalG = (byte)(g/totalWeight);
                var totalB = (byte)(b/totalWeight);
                var totalA = (byte)(a/totalWeight);

                package.TriangleVertexColors[colorCount] = totalR;
                package.TriangleVertexColors[colorCount + 1] = totalG;
                package.TriangleVertexColors[colorCount + 2] = totalB;
                package.TriangleVertexColors[colorCount + 3] = totalA;

                Debug.WriteLine(string.Format("v:{0}, uv:{1}, c:{2}", v, vUV, Color.ByARGB(totalA, totalR, totalG, totalB)));

                colorCount += 4;
            }
        }
    }
}
