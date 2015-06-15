using System;
using System.Collections.Generic;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    public class Display :  IGraphicItem
    {
        private readonly Geometry geometry;
        private readonly Color singleColor;
        private Color[][] colorMap;
        private bool renderEdges = false;
        private int samples;
        private const int LowestPower = 2;
        private const int HighestPower = 9;

        private Display(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.singleColor = color;
        }

        private Display(Surface surface, UV[] uvs, Color[] colors, int samples)
        {
            geometry = surface;
            this.samples = samples;
            colorMap = ComputeColorMap(surface, uvs, colors, samples, samples);
        }

        /// <summary>
        /// Compute a set of color maps from a set of SurfaceData objects.
        /// </summary>
        /// <returns></returns>
        private static Color[][] ComputeColorMap(Surface surface, IEnumerable<UV> uvs,  Color[] colors, int samplesU, int samplesV)
        {
            return Utils.CreateGradientColorMap(colors, uvs, samplesU, samplesV);
        }

        /// <summary>
        /// Display geometry using a color.
        /// </summary>
        /// <param name="geometry">The geometry to which you would like to apply color.</param>
        /// <param name="color">The color.</param>
        /// <returns>A Display object.</returns>
        public static Display ByGeometryColor(Geometry geometry, Color color)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (color == null)
            {
                throw new ArgumentNullException("color");
            }

            return new Display(geometry, color);
        }

        /// <summary>
        /// Display interpolated color values on a surface from data stored in a SurfaceData object.
        /// </summary>
        /// <param name="surface">The surface on which to apply the colors.</param>
        /// <param name="uvs">A set of UV locations on the surface corresponding to the colors.</param>
        /// <param name="colors">A set of Colors corresponding to the uvs.</param>
        /// <param name="precision">A value between 0.0 (low) and 1.0 (high) which defines the resolution</param>
        /// <returns>A Display object.</returns>
        public static Display BySurfaceUvsColors(Surface surface, UV[] uvs, Color[] colors, double precision = 0.5)
        {
            if (!uvs.Any())
            {
                throw new ArgumentException("uvs");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (precision < 0.0 || precision > 1.0)
            {
                throw new Exception("The precision must be in the range 0.0 to 1.0");
            }

            var samples = ComputeSamplesFromNormalizedValue(precision, LowestPower, HighestPower);

            return new Display(surface, uvs, colors, samples);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            if (singleColor != null)
            {
                CreateGeometryRenderData(singleColor, package, parameters);
            }
            else if (colorMap != null)
            {
                if (!colorMap.Any())
                {
                    return;
                }

                CreateColorMappedSurfaceRenderData(colorMap, package, parameters);
            }
        }

        private void CreateColorMappedSurfaceRenderData(Color[][] colorMap , IRenderPackage package, TessellationParameters parameters)
        {
            geometry.Tessellate(package, parameters);

            var colorBytes = new List<byte>();

            foreach (var colorArr in colorMap)
            {
                foreach (var c in colorArr)
                {
                    colorBytes.Add(c.Blue);
                    colorBytes.Add(c.Green);
                    colorBytes.Add(c.Red);
                    colorBytes.Add(c.Alpha);
                }
            }

            package.SetColors(colorBytes.ToArray());
            package.ColorsStride = colorMap.First().Length * 4;
        }

        private void CreateGeometryRenderData(Color color, IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            // As you add more data to the render package, you need
            // to keep track of the index where this coloration will 
            // start from.

            geometry.Tessellate(package, parameters);

            if (renderEdges)
            {
                var surf = geometry as Surface;
                if (surf != null)
                {
                    foreach (var curve in surf.PerimeterCurves())
                    {
                        curve.Tessellate(package, parameters);
                        curve.Dispose();
                    }
                }

                var solid = geometry as Solid;
                if (solid != null)
                {
                    foreach (var geom in solid.Edges.Select(edge => edge.CurveGeometry))
                    {
                        geom.Tessellate(package, parameters);
                        geom.Dispose();
                    }
                }
            }

            if (package.LineVertexCount > 0)
            {
                package.ApplyLineVertexColors(CreateColorByteArrayOfSize(package.LineVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }

            if (package.PointVertexCount > 0)
            {
                package.ApplyPointVertexColors(CreateColorByteArrayOfSize(package.PointVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }

            if (package.MeshVertexCount > 0)
            {
                package.ApplyMeshVertexColors(CreateColorByteArrayOfSize(package.MeshVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }
        }

        private static byte[] CreateColorByteArrayOfSize(int size, byte red, byte green, byte blue, byte alpha)
        {
            var arr = new byte[size * 4];
            for (var i = 0; i < arr.Count(); i+=4)
            {
                arr[i] = red;
                arr[i + 1] = green;
                arr[i + 2] = blue;
                arr[i + 3] = alpha;
            }
            return arr;
        }

        public override string ToString()
        {
            return string.Format("Display" + "(Geometry = {0}, Appearance = {1})", geometry, singleColor != null ? singleColor.ToString() : "Multiple colors.");
        }

        private static int ComputeSamplesFromNormalizedValue(double value, int lowestPower, int highestPower)
        {
            // Calculate the size of the image
            // Samples range from 2^2 (4) - 2^9 (512)
            var expRange = highestPower - lowestPower;
            var t = expRange*value;
            var finalExp = (int)Math.Pow(2, (int) (LowestPower + t));
            return finalExp;
        }
    }
}
