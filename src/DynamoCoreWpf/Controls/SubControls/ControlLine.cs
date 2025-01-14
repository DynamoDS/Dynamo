using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    /// <summary>
    /// Represents a visual connection (line) between two points in the Curve Mapper.
    /// </summary>
    public class ControlLine : CurveBase
    {
        private readonly LineSegment lineSeg;

        public ControlLine(Point controlPoint, Point fixPoint)
        {
            // Initialize the LineSegment for the line
            lineSeg = new LineSegment(fixPoint, true);

            // Configure the PathFigure with the start point and line segment
            PathFigure = new PathFigure();
            PathFigure.StartPoint = controlPoint;
            PathFigure.Segments.Add(lineSeg);

            // Configure the PathGeometry with the PathFigure
            PathGeometry = new PathGeometry();
            PathGeometry.Figures.Add(PathFigure);

            // Configure the Path with visual properties
            PathCurve = new Path()
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Centralize the color
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            };
        }

        /// <summary>
        /// Updates the line based on the new position of a single control point.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (updatedControlPoint.IsOrthogonal)
            {
                lineSeg.Point = updatedControlPoint.Point;
            }
            else
            {
                PathFigure.StartPoint = updatedControlPoint.Point;
            }
        }

        /// <summary>
        /// Updates the line based on the new positions of both control and fix points.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint controlPoint, CurveMapperControlPoint fixPoint)
        {
            lineSeg.Point = fixPoint.Point;
            PathFigure.StartPoint = controlPoint.Point;
        }
    }
}
