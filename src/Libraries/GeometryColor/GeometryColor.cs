using System;
using System.Collections.Generic;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using GeometryColor.Properties;
using DSCore;
using Dynamo.Graph.Nodes;
using Math = DSCore.Math;

namespace Modifiers
{
    public class GeometryColor :  IGraphicItem
    {
        #region private members

        private readonly Point[] vertices;
        private readonly Geometry geometry;
        private readonly Color singleColor;
        private readonly Color[][] colorMap;
        private readonly Color[] meshVertexColors;

        #endregion

        #region private constructors

        private GeometryColor(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.singleColor = color;
        }

        private GeometryColor(Surface surface, Color[][] colors)
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

        private GeometryColor(Point[] vertices, Color[] colors)
        {
            this.vertices = vertices;
            meshVertexColors = colors;
        }

        #endregion

        #region static constructors

        /// <summary>
        /// Display geometry using a color.
        /// </summary>
        /// <param name="geometry">The geometry to which you would like to apply color.</param>
        /// <param name="color">The color.</param>
        /// <returns>A Display object.</returns>
        public static GeometryColor ByGeometryColor([KeepReferenceAttribute]Geometry geometry, Color color)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (color == null)
            {
                throw new ArgumentNullException("color");
            }

            return new GeometryColor(geometry, color);
        }

        /// <summary>
        /// Display color values on a surface.
        /// 
        /// The colors provided are converted internally to an image texture which is
        /// mapped to the surface. 
        /// </summary>
        /// <param name="surface">The surface on which to apply the colors.
        /// </param>
        /// <param name="colors">A two dimensional list of Colors.
        /// 
        /// The list of colors must be square. Attempting to pass a jagged array
        /// will result in an exception. </param>
        /// <returns>A Display object.</returns>
        public static GeometryColor BySurfaceColors([KeepReferenceAttribute]Surface surface,
            [DefaultArgument("{{Color.ByARGB(255,255,0,0),Color.ByARGB(255,255,255,0)},{Color.ByARGB(255,0,255,255),Color.ByARGB(255,0,0,255)}};")] Color[][] colors)
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
                throw new ArgumentException(Resources.NoColorsExceptionMessage);
            }

            if (colors.Length == 1)
            {
                throw new ArgumentException(Resources.TwoDimensionalListExceptionMessage);
            }

            var size = colors[0].Count();
            foreach (var list in colors)
            {
                if (list.Count() != size)
                {
                    throw new ArgumentException(Resources.JaggedListExceptionMessage);
                }
            }

            return new GeometryColor(surface, colors);
        }

        /// <summary>
        /// Create a colored mesh using points and colors.
        /// 
        /// The list of points supplied is used to construct a triangulated mesh, with
        /// non-joined vertices.
        /// </summary>
        /// <param name="points">A list of Points. 
        /// 
        /// Only triangular meshes are currently supported. Each triplet of points in the list will form one 
        /// triangle in the mesh. Points should be ordered CCW. 
        /// Attempting to pass a list of vertices whose count is not divisble by 3 will throw an exception.</param>
        /// <param name="colors">A list of colors. 
        /// 
        /// The number of colors must match the number of vertices. Attempting pass a list of colors which does not
        /// have the same number of Colors as the list of points will throw an exception.</param>
        /// <returns>A Display object.</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static GeometryColor ByPointsColors([KeepReferenceAttribute]Point[] points, Color[] colors)
        {
            if(points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!points.Any())
            {
                throw new ArgumentException(Resources.NoVertexExceptionMessage, "points");
            }

            if (points.Count() %3 != 0)
            {
                throw new ArgumentException(Resources.VerticesDivisibleByThreeExceptionMessage);
            }

            if(colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (!colors.Any())
            {
                throw new ArgumentException(Resources.NoColorsExceptionMessage, "colors");
            }

            if (colors.Count() != points.Count())
            {
                throw new ArgumentException(Resources.VertexColorCountMismatchExceptionMessage, "colors");
            }

            return new GeometryColor(points, colors);
        }

        #endregion

        #region public methods

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            if(vertices != null)
            {
                CreateVertexColoredMesh(vertices, meshVertexColors, package, parameters);
                return;
            }

            if (singleColor != null)
            {
                CreateGeometryRenderData(singleColor, package, parameters);
                return;
            }

            if (colorMap != null)
            {
                if (!colorMap.Any())
                {
                    return;
                }

                CreateColorMapOnSurface(colorMap, package, parameters);
                return;
            }
        }

        public override string ToString()
        {
            return string.Format("GeometryColor" + "(Geometry = {0}, Appearance = {1})", geometry, singleColor != null ? singleColor.ToString() : "Multiple colors.");
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

        private static void CreateVertexColoredMesh(Point[] vertices, Color[] colors, IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            for (var i = 0; i <= vertices.Count()-3; i+=3)
            {
                var ptA = vertices[i];
                var ptB = vertices[i+1];
                var ptC = vertices[i+2];

                if (ptA.IsAlmostEqualTo(ptB) ||
                    ptB.IsAlmostEqualTo(ptC) ||
                    ptA.IsAlmostEqualTo(ptC))
                {
                    continue;
                }

                var alongLine = false;
                using (var l = Line.ByStartPointEndPoint(ptA, ptC))
                {
                    alongLine = ptB.DistanceTo(l) < 0.00001;
                }
                if (alongLine)
                {
                    continue;
                }

                var cA = colors[i];
                var cB = colors[i+1];
                var cC = colors[i+2];

                var s1 = ptB.AsVector().Subtract(ptA.AsVector()).Normalized();
                var s2 = ptC.AsVector().Subtract(ptA.AsVector()).Normalized();
                var n = s1.Cross(s2);

                package.AddTriangleVertex(ptA.X, ptA.Y, ptA.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cA.Red, cA.Green, cA.Blue, cA.Alpha);
                package.AddTriangleVertexUV(0, 0);

                package.AddTriangleVertex(ptB.X, ptB.Y, ptB.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cB.Red, cB.Green, cB.Blue, cB.Alpha);
                package.AddTriangleVertexUV(0, 0);

                package.AddTriangleVertex(ptC.X, ptC.Y, ptC.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cC.Red, cC.Green, cC.Blue, cC.Alpha);
                package.AddTriangleVertexUV(0, 0);
            }
        }

        #endregion
    }
}
