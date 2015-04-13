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
        /// <returns></returns>
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
                surf.PerimeterCurves().ForEach(
                        e =>
                            e.Tessellate(
                                package,
                                tol,
                                maxGridLines));
            }

            var solid = geometry as Solid;
            if (solid != null)
            {
                solid.Edges.ForEach(
                        e =>
                            e.CurveGeometry.Tessellate(
                                package,
                                tol,
                                maxGridLines));
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
