using System;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

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

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            package.PushPointVertex(point.X, point.Y, point.Z);
            package.Tag = label;
        }
    }
}
