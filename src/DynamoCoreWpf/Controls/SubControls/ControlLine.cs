using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class ControlLine : CurveBase
    {
        LineSegment lseg = null;

        public ControlLine(Point sp, Point ep)
        {
            lseg = new LineSegment(ep, true);

            PathFigure = new PathFigure();
            PathFigure.StartPoint = sp;
            PathFigure.Segments.Add(lseg);

            PathGeometry = new PathGeometry();
            PathGeometry.Figures.Add(PathFigure);

            PathCurve = new Path()
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Centralize the color
                StrokeThickness = 1
            };

            PathCurve.StrokeDashArray.Add(4);
            PathCurve.StrokeDashArray.Add(4);
        }
        public void Regenerate(CurveMapperControlPoint s)
        {
            PathFigure.StartPoint = s.Point;
        }

        public void Regenerate(CurveMapperControlPointOrtho c)
        {
            lseg.Point = c.Point;
        }
    }
}
