using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class CrossHair : CurveBase
    {
        //LineSegment cHLine = null;
        // height and width of the canvas
        //double maxWidth = 250;
        //double maxHeight = 250;
        private LineSegment cHLine;
        private readonly double maxWidth;
        private readonly double maxHeight;

        //bool isVertical = false;
        private readonly bool isVertical;

        // position of the crossHair
        Point point = new Point();

        public CrossHair(Canvas canvas, Point position, double canvasWidth, double canvasHeight, bool vertical = false)
        {
            maxWidth = canvasWidth;
            maxHeight = canvasHeight;
            isVertical = vertical;
            point = position;            

            PathFigure = new PathFigure();
            if (isVertical)
            {
                PathFigure.StartPoint = new System.Windows.Point(point.X, 0);
                cHLine = new LineSegment(new System.Windows.Point(point.X, maxHeight), true);
            }
            else
            {
                PathFigure.StartPoint = new System.Windows.Point(0, point.Y);
                cHLine = new LineSegment(new System.Windows.Point(maxWidth, point.Y), true);
            }
            PathFigure.Segments.Add(cHLine);

            PathGeometry = new PathGeometry();
            PathGeometry.Figures.Add(PathFigure);

            PathCurve = new System.Windows.Shapes.Path
            {
                Data = PathGeometry,

                Stroke = System.Windows.Media.Brushes.DimGray,
                StrokeThickness = 1,
                Opacity = 0.7
            };

            PathCurve.StrokeDashArray.Add(4);
            PathCurve.StrokeDashArray.Add(1);

            Canvas.SetZIndex(PathCurve, 10);
        }

        public void Regenerate(CurveMapperControlPoint point)
        {
            if (isVertical)
            {
                PathFigure.StartPoint = new System.Windows.Point(point.Point.X, 0);
                cHLine.Point = new System.Windows.Point(point.Point.X, maxHeight);
            }
            else
            {
                PathFigure.StartPoint = new System.Windows.Point(0, point.Point.Y);
                cHLine.Point = new System.Windows.Point(maxWidth, point.Point.Y);
            }
        }
    }
}
