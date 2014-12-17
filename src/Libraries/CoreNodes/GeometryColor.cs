using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using Geometry = Autodesk.DesignScript.Geometry.Geometry;

namespace DSCore
{
    public class ColoredGeometry : IGraphicItem
    {
        internal IList<Geometry> geometry;
        internal IList<Color> colors;

        private ColoredGeometry(IList<Geometry> geometry, IList<Color> color)
        {
            this.geometry = geometry;
            this.colors = color;
        }

        public static ColoredGeometry Color(IList<Geometry> geometry, IList<Color> color)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (color == null)
            {
                throw new ArgumentNullException("color");
            }

            if (geometry.Count != color.Count)
            {
                throw new Exception("You must supply an equivalent number of pieces of geometry and colors.");
            }

            return new ColoredGeometry(geometry, color);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            var color_idx = 0;
            
            foreach (var geom in geometry)
            {
                // As you add more data to the render package, you need
                // to keep track of the index where this coloration will 
                // start from.
                var lineStripStartIndex = package.LineStripVertexColors.Count;
                var pointVertexStartIndex = package.PointVertexColors.Count;
                var triangleVertexStartIndex = package.TriangleVertexColors.Count;

                geom.Tessellate(package, tol, maxGridLines);

                SetColorOnArray(package.LineStripVertexColors, colors[color_idx], lineStripStartIndex);
                SetColorOnArray(package.PointVertexColors, colors[color_idx], pointVertexStartIndex);
                SetColorOnArray(package.TriangleVertexColors, colors[color_idx], triangleVertexStartIndex);
                color_idx++;
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
