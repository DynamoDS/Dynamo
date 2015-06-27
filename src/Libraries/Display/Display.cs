using System;
using System.Collections.Generic;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSCore;
using Math = DSCore.Math;

namespace Display
{
    public class Display :  IGraphicItem
    {
        #region private members

        private readonly Geometry geometry;
        private readonly Color singleColor;
        private readonly Color[][] colorMap;

        #endregion

        #region private constructors

        private Display(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.singleColor = color;
        }

        private Display(Surface surface, Color[][] colors)
        {
            geometry = surface;

            // Transpose the colors array. This is required
            // to correctly align the colors on the surface with
            // the UV space of the surface.

            var rows = colors.GetLength(0);
            var columns = colors[0].Count();

            colorMap = new Color[columns][];
            for (var c = 0; c < columns; c++)
            {
                colorMap[c] = new Color[rows];
            }

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    colorMap[j][i] = colors[i][j];
                }
            } 
        }

        #endregion

        #region static constructors

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
        /// Display color values on a surface.
        /// </summary>
        /// <param name="surface">The surface on which to apply the colors.</param>
        /// <param name="colors">A two dimensional list of Colors.</param>
        /// <returns>A Display object.</returns>
        public static Display BySurfaceColors(Surface surface, Color[][] colors)
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
                throw new ArgumentException("You must supply some colors");
            }

            if (colors.Length == 1)
            {
                throw new ArgumentException("You must supply a two dimensional list of Colors.");
            }

            var size = colors[0].Count();
            foreach (var list in colors)
            {
                if (list.Count() != size)
                {
                    throw new ArgumentException("The list of colors must not be a jagged list.");
                }
            }

            return new Display(surface, colors);
        }

        #endregion

        #region public methods

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

                CreateColorMapOnSurface(colorMap, package, parameters);
            }
        }

        public override string ToString()
        {
            return string.Format("Display" + "(Geometry = {0}, Appearance = {1})", geometry, singleColor != null ? singleColor.ToString() : "Multiple colors.");
        }

        #endregion

        #region private helper methods

        /// <summary>
        /// Compute a set of color maps from a set of SurfaceData objects.
        /// </summary>
        /// <returns></returns>
        private static Color[][] ComputeColorMap(Surface surface, IEnumerable<UV> uvs, Color[] colors, int samplesU, int samplesV)
        {
            return Utils.CreateGradientColorMap(colors, uvs, samplesU, samplesV);
        }

        private void CreateColorMapOnSurface(Color[][] colorMap , IRenderPackage package, TessellationParameters parameters)
        {
            const byte gray = 80;
 
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

            TessellateEdges(package, parameters);

            if (package.LineVertexCount > 0)
            {
                package.ApplyLineVertexColors(CreateColorByteArrayOfSize(package.LineVertexCount, gray, gray,
                    gray, 255));
            }
        }

        private void CreateGeometryRenderData(Color color, IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            // As you add more data to the render package, you need
            // to keep track of the index where this coloration will 
            // start from.

            geometry.Tessellate(package, parameters);

            TessellateEdges(package, parameters);

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

        private void TessellateEdges(IRenderPackage package, TessellationParameters parameters)
        {
            if (!parameters.ShowEdges) return;

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

        /// <summary>
        /// This method remaps a number between 0.0 and 1.0 to an integer value between lowestPower and highestPower
        /// </summary>
        private static int ComputeSamplesFromNormalizedValue(double value, int lowestPower, int highestPower)
        {
            // Calculate the size of the image
            // Samples range from 2^2 (4) - 2^9 (512)
            var expRange = highestPower - lowestPower;
            var t = expRange*value;
            var finalExp = (int)Math.Pow(2, (int)(lowestPower + t));
            return finalExp;
        }

        #endregion
    }
}
