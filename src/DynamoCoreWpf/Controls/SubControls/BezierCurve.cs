using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.Controls.SubControls
{
    /// <summary>
    /// Represents a bezier curve connecting two fixed points and controlled by two control points.
    /// </summary>
    public class BezierCurve : CurveBase
    {
        private CurveMapperControlPoint freeControlPoint1;
        private CurveMapperControlPoint freeControlPoint2;
        private BezierSegment bezierSegment;
        private Dictionary<double, double> xToYMap = new Dictionary<double, double>();
        private double tFactor;

        /// <summary>
        /// Initializes a new instance of the CurveBezier class with the given control points and limits.
        /// </summary>
        public BezierCurve(CurveMapperControlPoint orthoPointStart,
            CurveMapperControlPoint orthoPointEnd,
            CurveMapperControlPoint freeControlPoint1,
            CurveMapperControlPoint freeControlPoint2,
            double maxWidth,
            double maxHeight)
        {
            controlPoint1 = orthoPointStart;
            controlPoint2 = orthoPointEnd;
            this.freeControlPoint1 = freeControlPoint1;
            this.freeControlPoint2 = freeControlPoint2;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            tFactor = 1.0 / this.MaxWidth;
            bezierSegment = new BezierSegment(this.freeControlPoint1.Point, this.freeControlPoint2.Point, this.controlPoint2.Point, true);

            PathFigure = new PathFigure()
            {
                StartPoint = this.controlPoint1.Point,
                Segments = { bezierSegment }
            };

            PathGeometry = new PathGeometry
            {
                Figures = { PathFigure }
            };

            PathCurve = new System.Windows.Shapes.Path
            {
                Data = PathGeometry,
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private void GetValueAtT(double t, out double x, out double y)
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

        // public void GetMaximumMinimumOrdinates(double samplingResolution, out double min, out double max)
        //{
        //    min = double.PositiveInfinity;
        //    max = double.NegativeInfinity;

        //    for (double i = 0; i <= samplingResolution; i++)
        //    {
        //        GetValueAtT(i / samplingResolution, out _, out double y);
        //        y = Math.Round(maxHeight - y, 2);
        //        min = Math.Min(min, y);
        //        max = Math.Max(max, y);
        //    }
        //}

        private void GenerateXYPairs()
        {
            xToYMap.Clear();

            for (double t = 0; t <= 1.0; t += 1.0 / (MaxWidth * 5.0))
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
                double x = Math.Round((MaxWidth / (count - 1.0)) * i);
                if (xToYMap.TryGetValue(x, out double y))
                {
                    double normalizedY = MaxHeight - y;
                    double scaledY = lowLimit + ((highLimit - lowLimit) * normalizedY / MaxHeight);
                    scaledY = Math.Round(scaledY, rounding);
                    values.Add(scaledY);
                }
            }
            return values;
        }

        /// <summary>
        /// Regenerates the bezier curve when a control point is updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (updatedControlPoint == controlPoint1)
            {
                PathFigure.StartPoint = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == controlPoint2)
            {
                bezierSegment.Point3 = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == freeControlPoint1)
            {
                bezierSegment.Point1 = updatedControlPoint.Point;
            }
            else if (updatedControlPoint == freeControlPoint2)
            {
                bezierSegment.Point2 = updatedControlPoint.Point;
            }
        }
    }
}
