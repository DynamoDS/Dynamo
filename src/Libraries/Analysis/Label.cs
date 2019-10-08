using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    public class Label : IGraphicItem
    {
        private string label;
        private Point point;

        private Label(Point point, string label)
        {
            this.point = point;
            this.label = label;
        }
        /// <summary>
        /// Returns a Label object given a point object and a string label.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Label ByPointAndString(Point point, string label)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point","You must supply a valid point.");
            }
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label","You must supply a label");
            }

            return new Label(point,label);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            package.AddPointVertex(point.X, point.Y, point.Z);
            package.Description = label;
        }
    }
}
