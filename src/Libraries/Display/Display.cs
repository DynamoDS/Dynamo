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
        private const int Samples = 64;

        private Display(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.singleColor = color;
        }

        private Display(Surface surface, UV[] uvs, Color[] colors)
        {
            this.geometry = surface;
            this.colorMap = ComputeColorMap(surface, uvs, colors);
        }

        /// <summary>
        /// Compute a set of color maps from a set of SurfaceData objects.
        /// </summary>
        /// <param name="surfaceDatas"></param>
        /// <param name="colorRange"></param>
        /// <returns></returns>
        private Color[][] ComputeColorMap(Surface surface, UV[] uvs,  Color[] colors)
        {
            return Utils.CreateGradientColorMap(colors, uvs, Samples, Samples);
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
        /// <param name="surfaceData">The SurfaceData object.</param>
        /// <param name="colorRange">A ColorRange1D object.</param>
        /// <returns>A Display object.</returns>
        public static Display BySurfaceUvsColors(Surface surface, UV[] uvs, Color[] colors)
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

            return new Display(surface, uvs, colors);
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
            package.ColorsStride = colorMap.First().Length;
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
    }
}
