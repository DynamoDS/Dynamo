using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using Geometry = Autodesk.DesignScript.Geometry.Geometry;

namespace DSCore
{
    public class ColoredGeometry :  IGraphicItem
    {
        internal Geometry geometry;
        internal Color color;

        private ColoredGeometry(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.color = color;
        }

        public static ColoredGeometry ByGeometryAndColor(Geometry geometry, Color color)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (color == null)
            {
                throw new ArgumentNullException("color");
            }

            return new ColoredGeometry(geometry, color);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            var color_idx = 0;
            
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
            color_idx++;
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
