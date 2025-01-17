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

        public CurveMapperControlPoint StartPoint { get; set; }
        public CurveMapperControlPoint EndPoint { get; set; }
        public double MaxWidth { get; set; } // Needs to be public to update curve when the canvas is resized
        public double MaxHeight { get; set; }
        public Path PathCurve { get; set; }

        public LinearCurve(CurveMapperControlPoint startPoint, CurveMapperControlPoint endPoint, double maxWidth, double maxHeight)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
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
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Centralize the color
                StrokeThickness = 3
            };
        }

        public Action CurveUpdated;


        /// <summary>
        /// Calculates the Y value for a given X using the linear equation.
        /// </summary>
        private double LineEquation(double x)
        {
            double slope = (EndPoint.Point.Y - StartPoint.Point.Y) / (EndPoint.Point.X - StartPoint.Point.X);
            if (double.IsNaN(slope))
            {
                return double.NaN;
            }

            return slope * (x - StartPoint.Point.X) + (StartPoint.Point.Y);
        }

        /// <summary>
        /// Calculates the X value for a given Y using the linear equation.
        /// </summary>
        private double SolveForXGivenY(double y)
        {
            double slope = (EndPoint.Point.Y - StartPoint.Point.Y) / (EndPoint.Point.X - StartPoint.Point.X);
            if (double.IsNaN(slope))
            {
                return double.NaN;
            }

            return ((y - StartPoint.Point.Y) / slope) + StartPoint.Point.X;
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

            //// Raise event to notify listeners (CurveMapperNodeModel)
            CurveUpdated?.Invoke();
        }

        /// <summary>
        /// Ensures the points are clamped within the canvas bounds.
        /// </summary>
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
        public override List<double> GetCurveYValues(double minY, double maxY, int pointCount)
        {
            if (pointCount < 1 || Math.Abs(StartPoint.Point.X - EndPoint.Point.X) < double.Epsilon) return null;

            var values = new List<double>();
            double step = MaxWidth / (pointCount - 1);

            for (double x = 0; x <= MaxWidth; x += step)
            {
                double yCanvas = LineEquation(x);
                double yMapped = MapCanvasToRange(yCanvas, minY, maxY);
                values.Add(yMapped);
            }

            return values;
        }


        private double MapCanvasToRange(double canvasY, double lowLimit, double highLimit)
        {
            double normalizedY = MaxHeight - canvasY;
            return lowLimit + (normalizedY / MaxHeight) * (highLimit - lowLimit);
        }

        
    }
}
