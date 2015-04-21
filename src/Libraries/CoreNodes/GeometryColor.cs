using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using Geometry = Autodesk.DesignScript.Geometry.Geometry;

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
            // As you add more data to the render package, you need
            // to keep track of the index where this coloration will 
            // start from.
            var lineStripStartIndex = package.LineStripVertexColors.Count;
            var pointVertexStartIndex = package.PointVertexColors.Count;
            var triangleVertexStartIndex = package.TriangleVertexColors.Count;

            geometry.Tessellate(package, tol, maxGridLines);

            SetColorOnArray(package.LineStripVertexColors, color, lineStripStartIndex);
            SetColorOnArray(package.PointVertexColors, color, pointVertexStartIndex);
            SetColorOnArray(package.TriangleVertexColors, color, triangleVertexStartIndex);

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
                ReColorVerticesFromTo(start, end, package);
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
                ReColorVerticesFromTo(start, end, package);
            }

            for (var i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                NudgeVertexAlongVector(package.TriangleVertices, package.TriangleNormals, i, 0.001);
            }

        }

        private void NudgeVertexAlongVector(IList<double> vertices, IList<double> normals, int i, double amount)
        {
            var x = (float)vertices[i];
            var y = (float)vertices[i + 1];
            var z = (float)vertices[i + 2];
            var v = Vector.ByCoordinates(x, y, z);

            var nx = (float)vertices[i];
            var ny = (float)vertices[i + 1];
            var nz = (float)vertices[i + 2];
            var n = Vector.ByCoordinates(nx, ny, nz);

            var nudge = v.Add(n.Normalized().Scale(amount));
            vertices[i] = nudge.X;
            vertices[i + 1] = nudge.Y;
            vertices[i + 2] = nudge.Z;
        }

        private void ReColorVerticesFromTo(int start, int end, IRenderPackage package)
        {
            for (var i = start; i < end; i += 4)
            {
                package.LineStripVertexColors[i] = color.Red;
                package.LineStripVertexColors[i + 1] = color.Green;
                package.LineStripVertexColors[i + 2] = color.Blue;
            }
        }

        private void SetColorOnArray(IList<byte> array, Color color, int startIndex)
        {
            for (int i = startIndex; i < array.Count; i += 4)
            {
                array[i] = color.Red;
                array[i + 1] = color.Green;
                array[i + 2] = color.Blue;
                array[i + 3] = color.Alpha;
            }
        }
    }
}
