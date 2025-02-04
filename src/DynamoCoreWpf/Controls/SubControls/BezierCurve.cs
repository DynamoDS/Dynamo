using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Gets interpolated Y values based on the assigned parameters and limits.
        /// </summary>
        public List<double> GetBezierCurveYValues(double lowLimit, double highLimit, int count, double canvasSize)
        {
            if (count < 1) return null;

            var values = new List<double>();
            List<double> xSamples = new List<double>();
            List<double> ySamples = new List<double>();

            // Generate fine-grained samples to ensure better interpolation
            int fineSteps = (int)(count * canvasSize);
            for (int i = 0; i <= fineSteps; i++)
            {
                double t = i / (double)fineSteps;
                GetValueAtT(t, out double x, out double y);
                xSamples.Add(x);
                ySamples.Add(y);
            }

            // Pick values at evenly spaced X points
            for (int i = 0; i < count; i++)
            {
                double targetX = (i / (double)(count - 1)) * MaxWidth;
                int closestIndex = xSamples.IndexOf(xSamples.OrderBy(x => Math.Abs(x - targetX)).First());
                double y = ySamples[closestIndex];

                // Normalize Y to the user-specified range
                double normalizedY = MaxHeight - y;
                double scaledY = lowLimit + ((highLimit - lowLimit) * normalizedY / MaxHeight);
                values.Add(Math.Round(scaledY, rounding));
            }
            return values;
        }

        /// <summary>
        /// Regenerates the bezier curve when a control point is updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            switch (updatedControlPoint)
            {
                case var point when point == controlPoint1:
                    PathFigure.StartPoint = point.Point;
                    break;
                case var point when point == controlPoint2:
                    bezierSegment.Point3 = point.Point;
                    break;
                case var point when point == freeControlPoint1:
                    bezierSegment.Point1 = point.Point;
                    break;
                case var point when point == freeControlPoint2:
                    bezierSegment.Point2 = point.Point;
                    break;
            }
        }

        /// <summary>
        /// Regenerates the bezier curve fully.
        /// </summary>
        public override void Regenerate()
        {
            if (PathFigure == null || bezierSegment == null ||
                controlPoint1 == null || controlPoint2 == null ||
                freeControlPoint1 == null || freeControlPoint2 == null)
            {
                return;
            }

            PathFigure.StartPoint = controlPoint1.Point;
            bezierSegment.Point3 = controlPoint2.Point;
            bezierSegment.Point1 = freeControlPoint1.Point;
            bezierSegment.Point2 = freeControlPoint2.Point;
        }
    }
}
