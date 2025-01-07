using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    /// <summary>
    /// Represents a linear curve rendered on the canvas, updated dynamically with control points.
    /// </summary>
    public class CurveLinear : CurveBase
    {
        private readonly LineSegment lineSegment;
        private readonly PathFigure pathFigure;
        private readonly PathGeometry pathGeometry;

        public CurveMapperControlPoint StartPoint { get; set; }
        public CurveMapperControlPoint EndPoint { get; set; }
        public double MaxWidth { get; set; }
        public double MaxHeight { get; set; }
        public Path PathCurve { get; set; }

        public CurveLinear(CurveMapperControlPoint startPoint, CurveMapperControlPoint endPoint, double maxWidth, double maxHeight)
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
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Centalize the color
                StrokeThickness = 3
            };
        }

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
        public void Regenerate(CurveMapperControlPoint controlPoint)
        {
            double yStart = LineEquation(0);
            double yEnd = LineEquation(MaxWidth);

            Point p1 = GetClampedPoint(0, yStart);
            Point p2 = GetClampedPoint(MaxWidth, yEnd);

            pathFigure.StartPoint = p1;
            lineSegment.Point = p2;
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
        /// Generates values between the limits based on the curve.
        /// </summary>
        public override List<double> GetValuesFromAssignedParameters(double lowLimit, double highLimit, int count)
        {
            if (count < 1 || Math.Abs(StartPoint.Point.X - EndPoint.Point.X) < double.Epsilon)
                return null;

            var values = new List<double>();
            double step = MaxWidth / (count - 1);

            for (double x = 0; x <= MaxWidth; x += step)
            {
                double yCanvas = LineEquation(x);
                double yMapped = MapCanvasToRange(yCanvas, lowLimit, highLimit);
                values.Add(yMapped);
            }

            return values;

            //List<double> livalues = new List<double>();

            ////  Test if vertical
            //if (StartPoint.Point.X == EndPoint.Point.X)
            //{
            //    return null;
            //}
            //if (double.IsNaN(LineEquation(0.0)))
            //{
            //    return null;
            //}

            //double inCount = (double)(count - 1);
            //for (double d = 0.0; d < MaxWidth; d += (MaxWidth / inCount))
            //{
            //    double md = MaxHeight - LineEquation(d);
            //    double rd = (highLimit - lowLimit) * md / MaxHeight;
            //    rd += lowLimit;
            //    livalues.Add(rd);
            //}

            //if (livalues.Count < count)
            //{
            //    double mdx = MaxHeight - LineEquation(MaxWidth);
            //    double rdx = (highLimit - lowLimit) * mdx / MaxHeight;
            //    rdx += lowLimit;
            //    livalues.Add(rdx);
            //}

            //return livalues;
        }

        /// <summary>
        /// Maps canvas Y values to the range defined by the limits.
        /// </summary>
        private double MapCanvasToRange(double canvasY, double lowLimit, double highLimit)
        {
            double normalizedY = MaxHeight - canvasY;
            return lowLimit + (normalizedY / MaxHeight) * (highLimit - lowLimit);
        }
    }
}
