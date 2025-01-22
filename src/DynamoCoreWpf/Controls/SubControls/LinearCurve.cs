using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    /// <summary>
    /// Represents a linear curve rendered on the canvas, updated dynamically with control points.
    /// </summary>
    public class LinearCurve : CurveBase
    {
        private readonly LineSegment lineSegment;
        private readonly PathFigure pathFigure;
        private readonly PathGeometry pathGeometry;

        public LinearCurve(CurveMapperControlPoint startPoint, CurveMapperControlPoint endPoint, double maxWidth, double maxHeight)
        {
            controlPoint1 = startPoint;
            controlPoint2 = endPoint;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            // Initialize the line segment and path geometry
            lineSegment = new LineSegment(endPoint.Point, true);
            pathFigure = new PathFigure { StartPoint = startPoint.Point };
            pathFigure.Segments.Add(lineSegment);

            pathGeometry = new PathGeometry { Figures = { pathFigure } };

            PathCurve = new Path
            {
                Data = pathGeometry,
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private double LineEquation(double x)
        {
            double dx = controlPoint2.Point.X - controlPoint1.Point.X;
            double dy = controlPoint2.Point.Y - controlPoint1.Point.Y;
            if (Math.Abs(dx) < double.Epsilon) return double.NaN;

            return dy / dx * (x - controlPoint1.Point.X) + controlPoint1.Point.Y;
        }

        private double SolveForXGivenY(double y)
        {
            double slope = (controlPoint2.Point.Y - controlPoint1.Point.Y) / (controlPoint2.Point.X - controlPoint1.Point.X);
            if (double.IsNaN(slope))
            {
                return double.NaN;
            }

            return ((y - controlPoint1.Point.Y) / slope) + controlPoint1.Point.X;
        }

        /// <summary>
        /// Regenerates the curve based on the updated positions of control points.
        /// </summary>
        public void Regenerate()
        {
            double yStart = LineEquation(0);
            double yEnd = LineEquation(MaxWidth);

            Point p1 = GetClampedPoint(0, yStart);
            Point p2 = GetClampedPoint(MaxWidth, yEnd);

            pathFigure.StartPoint = p1;
            lineSegment.Point = p2;
        }

        private Point GetClampedPoint(double x, double y)
        {
            if (y < 0)
                return new Point(SolveForXGivenY(0), 0);
            if (y > MaxHeight)
                return new Point(SolveForXGivenY(MaxHeight), MaxHeight);
            return new Point(x, y);
        }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public override List<double> GetCurveYValues(double minX, double maxX, int pointCount)
        {
            if (pointCount < 1 || controlPoint1.Point.X == controlPoint2.Point.X || double.IsNaN(LineEquation(0.0)))
                return null;

            var values = new List<double>();
            double step = MaxWidth / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double d = i * step;
                double md = MaxHeight - LineEquation(d);
                double rd = minX + (maxX - minX) * md / MaxHeight;
                values.Add(Math.Round(rd, rounding));
            }

            return values;
        }
    }
}
