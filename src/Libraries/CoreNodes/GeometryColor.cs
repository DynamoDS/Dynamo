using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    public class Display :  IGraphicItem
    {
        internal Geometry geometry;
        internal Color color;

        private Display(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.color = color;
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

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            package.RequiresPerVertexColoration = true;

            // As you add more data to the render package, you need
            // to keep track of the index where this coloration will 
            // start from.
            var lineStripStartIndex = package.LineStripVertexColors.Count;
            var pointVertexStartIndex = package.PointVertexColors.Count;
            var triangleVertexStartIndex = package.TriangleVertexColors.Count;

            geometry.Tessellate(package, tol, maxGridLines);

            package.LineStripVertexColors = ResetColorOnArray(package.LineStripVertexColors, color, lineStripStartIndex);
            package.PointVertexColors = ResetColorOnArray(package.PointVertexColors, color, pointVertexStartIndex);
            package.TriangleVertexColors = ResetColorOnArray(package.TriangleVertexColors, color, triangleVertexStartIndex);

            var surf = geometry as Surface;
            if (surf != null)
            {
                var start = package.LineStripVertexColors.Count;
                surf.PerimeterCurves().ForEach(
                        e =>
                            e.Tessellate(
                                package,
                                tol,
                                maxGridLines));
                var end = package.LineStripVertexColors.Count - 1;
                ReColorLineVerticesFromTo(start, end, package);
            }

            var solid = geometry as Solid;
            if (solid != null)
            {
                var start = package.LineStripVertexColors.Count;
                solid.Edges.ForEach(
                        e =>
                            e.CurveGeometry.Tessellate(
                                package,
                                tol,
                                maxGridLines));
                var end = package.LineStripVertexColors.Count - 1;
                ReColorLineVerticesFromTo(start, end, package);
            }

            var existingVerts = package.TriangleVertices;
            var existingNormals = package.TriangleNormals;

            var newVerts = new List<double>();
            for (var i = 0; i < existingVerts.Count; i += 3)
            {
                newVerts.AddRange(NudgeVertexAlongVector(existingVerts, existingNormals, i, 0.001));
            }
            package.TriangleVertices = newVerts;
        }

        private static IEnumerable<double> NudgeVertexAlongVector(IList<double> vertices, IList<double> normals, int i, double amount)
        {
            var x = (float)vertices[i];
            var y = (float)vertices[i + 1];
            var z = (float)vertices[i + 2];
            var v = Vector.ByCoordinates(x, y, z);

            var nx = (float)normals[i];
            var ny = (float)normals[i + 1];
            var nz = (float)normals[i + 2];
            var n = Vector.ByCoordinates(nx, ny, nz);

            var nudge = v.Add(n.Normalized().Scale(amount));

            return new [] { nudge.X, nudge.Y, nudge.Z };
        }

        private void ReColorLineVerticesFromTo(int start, int end, IRenderPackage package)
        {
            var colors = package.LineStripVertexColors;
            for (var i = start; i < end; i += 4)
            {
                colors[i] = color.Red;
                colors[i + 1] = color.Green;
                colors[i + 2] = color.Blue;
                colors[i + 3] = color.Alpha;
            }
            package.LineStripVertexColors = null;
            package.LineStripVertexColors = colors;
        }

        private static List<byte> ResetColorOnArray(List<byte> array, Color color, int startIndex)
        {
            var colors = array;
            for (var i = startIndex; i < array.Count; i += 4)
            {
                colors[i] = color.Red;
                colors[i + 1] = color.Green;
                colors[i + 2] = color.Blue;
                colors[i + 3] = color.Alpha;
            }
            return colors;
        }
    }
}
