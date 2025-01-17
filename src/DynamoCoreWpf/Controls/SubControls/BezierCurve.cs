using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.Controls.SubControls
{
    /// <summary>
    /// Represents a bezier curve connecting two fixed points and controlled by two control points.
    /// </summary>
    public class BezierCurve : CurveBase
    {
        private CurveMapperControlPoint controlPoint1;
        private CurveMapperControlPoint controlPoint2;
        private CurveMapperControlPoint othoPoint1;
        private CurveMapperControlPoint orthoPoint2;
        private BezierSegment bezierSegment;
        private Dictionary<double, double> xToYMap = new Dictionary<double, double>();

        private double maxWidth;
        private double maxHeight;
        private double tFactor;
        private const int rounding = 10;

        public double MaxWidth
        {
            get => maxWidth;
            set
            {
                if (maxWidth != value)
                {
                    maxWidth = value;
                    Regenerate(controlPoint1); // Ensure the curve regenerates if needed
                }
            }
        }
        public double MaxHeight
        {
            get => maxHeight;
            set
            {
                if (maxHeight != value)
                {
                    maxHeight = value;
                    Regenerate(controlPoint1); // Ensure the curve regenerates if needed
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the CurveBezier class with the given control points and limits.
        /// </summary>
        public BezierCurve(CurveMapperControlPoint fixedPointStart,
            CurveMapperControlPoint fixedPointEnd,
            CurveMapperControlPoint controlPoint1,
            CurveMapperControlPoint controlPoint2,
            double maxWidth,
            double maxHeight)
        {
            this.othoPoint1 = fixedPointStart;
            this.orthoPoint2 = fixedPointEnd;
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.tFactor = 1.0 / this.maxWidth;
            this.bezierSegment = new BezierSegment(this.controlPoint1.Point, this.controlPoint2.Point, orthoPoint2.Point, true);

            PathFigure = new PathFigure()
            {
                StartPoint = othoPoint1.Point,
                Segments = { bezierSegment }
            };

            PathGeometry = new PathGeometry
            {
                Figures = { PathFigure }
            };

            PathCurve = new System.Windows.Shapes.Path
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Purple color
                StrokeThickness = 3
            };
        }

        /// <summary>
        /// Calculates the X and Y coordinates at a specific T parameter on the bezier curve.
        /// </summary>
        public void GetValueAtT(double t, out double x, out double y)
        {
            Point p1 = new Point();
            Point p2 = new Point();
            Point p3 = new Point();
            Point p4 = new Point();

            // Ensure we access UI elements on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                p1 = PathFigure.StartPoint;
                p2 = bezierSegment.Point1;
                p3 = bezierSegment.Point2;
                p4 = bezierSegment.Point3;
            }); ;

            x = Math.Pow(1 - t, 3) * p1.X +
                3 * Math.Pow(1 - t, 2) * t * p2.X +
                3 * (1 - t) * t * t * p3.X +
                t * t * t * p4.X;

            y = Math.Pow(1 - t, 3) * p1.Y +
                3 * Math.Pow(1 - t, 2) * t * p2.Y +
                3 * (1 - t) * t * t * p3.Y +
                t * t * t * p4.Y;
        }

        /// <summary>
        /// Computes the minimum and maximum ordinates (Y values) for the bezier curve.
        /// </summary>
        public void GetMaximumMinimumOrdinates(double samplingResolution, out double min, out double max)
        {
            min = double.PositiveInfinity;
            max = double.NegativeInfinity;

            for (double i = 0; i <= samplingResolution; i++)
            {
                GetValueAtT(i / samplingResolution, out _, out double y);
                y = Math.Round(maxHeight - y, 2);
                min = Math.Min(min, y);
                max = Math.Max(max, y);
            }
        }

        private void GenerateXYPairs()
        {
            xToYMap.Clear();

            for (double t = 0; t <= 1.0; t += 1.0 / (maxWidth * 5.0))
            {
                GetValueAtT(t, out double x, out double y);
                x = Math.Round(x, 0);
                if (!xToYMap.ContainsKey(x))
                {
                    xToYMap.Add(x, y);
                }
            }
        }

        /// <summary>
        /// Gets interpolated Y values based on the assigned parameters and limits.
        /// </summary>
        public override List<double> GetCurveYValues(double lowLimit, double highLimit, int count)
        {
            if (count < 1) return null;

            GenerateXYPairs();

            var values = new List<double>();

            for (int i = 0; i < count; i++)
            {
                double x = Math.Round((maxWidth / (count - 1.0)) * i);
                if (xToYMap.TryGetValue(x, out double y))
                {
                    double normalizedY = maxHeight - y;
                    double scaledY = lowLimit + ((highLimit - lowLimit) * normalizedY / maxHeight);
                    values.Add(scaledY);
                }
            }
            return values;
        }
        /// <summary>
        /// Calculates the X-axis values for the curve based on input limits and count.
        /// </summary>
        public override List<double> GetCurveXValues(double minX, double maxX, int pointCount)
        {            
            if (pointCount < 1 || Math.Abs(othoPoint1.Point.X - orthoPoint2.Point.X) < double.Epsilon)
                return null;

            var values = new List<double>();
            double step = (maxX - minX) / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double xMapped = Math.Round(minX + i * step, rounding);
                values.Add(xMapped);
            }

            return values;
        }


        /// <summary>
        /// Regenerates the bezier curve when a control point is updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (updatedControlPoint == othoPoint1)
            {
                PathFigure.StartPoint = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == orthoPoint2)
            {
                bezierSegment.Point3 = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == controlPoint1)
            {
                bezierSegment.Point1 = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == controlPoint2)
            {
                bezierSegment.Point2 = updatedControlPoint.Point;
            }
        }
    }
}
