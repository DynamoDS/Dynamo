using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.CurveMapper
{
    public class CurveRenderer
    {
        public static Path RenderCurve(List<double> xValues, List<double> yValues, double canvasSize)
        {
            if (xValues == null || yValues == null || xValues.Count != yValues.Count || xValues.Count < 2)
                return null;

            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(xValues[0], canvasSize - yValues[0])
            };

            for (int i = 1; i < xValues.Count; i++)
            {
                pathFigure.Segments.Add(new LineSegment(new Point(xValues[i], canvasSize - yValues[i]), true));
            }

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B385F2")),
                StrokeThickness = 3
            };
        }
    }
}
