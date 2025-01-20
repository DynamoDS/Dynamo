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
        public  List<double> GetLinearCurveYValues(double minX, double maxX, double minY, double maxY, int pointCount, double canvasSize)
        {
            var (calcMinY, calcMaxY) = GetCurveLimits(minX, maxX, canvasSize, false);
            minY = Math.Max(minY, calcMinY);
            maxY = Math.Min(maxY, calcMaxY);

            // TODO: Return values when the curve is vertical ?
            if (pointCount < 1) return null;

            var values = new List<double>();
            double step = (maxY-minY) / (pointCount -1) ;

            for (int i = 0; i < pointCount; i++)
            {
                values.Add(minY + i * step);
            }

            return values;
        }

        /// <summary>
        /// Calculates the X-axis values for the curve based on input limits and count.
        /// </summary>
        public List<double> GetLinearCurveXValues(double minX, double maxX, double minY, double maxY, int pointCount, double canvasSize)
        {
            var (calcMinX, calcMaxX) = GetCurveLimits(minY, maxY, canvasSize, true);
            minX = Math.Max(minX, calcMinX);
            maxX = Math.Min(maxX, calcMaxX);

            // TODO: Return values when the curve is horizontal ?
            if (pointCount < 1) return null;

            var values = new List<double>();
            double step = (maxX - minX) / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                values.Add(minX + i * step);
            }

            return values;
        }

        private (double min, double max) GetCurveLimits(double minValue, double maxValue, double canvasSize, bool computeX)
        {
            double x1 = controlPoint1.Point.X;
            double x2 = controlPoint2.Point.X;
            double y1 = canvasSize - controlPoint1.Point.Y;
            double y2 = canvasSize - controlPoint2.Point.Y;

            // Determine which variable is independent
            double independent1 = computeX ? y1 : x1;
            double independent2 = computeX ? y2 : x2;
            double dependent1 = computeX ? x1 : y1;
            double dependent2 = computeX ? x2 : y2;

            double dIndependent = (independent2 - independent1) / canvasSize;
            double dDependent = (dependent2 - dependent1) / canvasSize;

            if (Math.Abs(dIndependent) < double.Epsilon) return (dependent1, dependent2); // Avoid division by zero

            double m = dDependent / dIndependent;
            double b = dependent1 / canvasSize - m * independent1 / canvasSize;

            return (Math.Max(m * minValue + b, minValue), Math.Min(m * maxValue + b, maxValue));
        }
    }
}
