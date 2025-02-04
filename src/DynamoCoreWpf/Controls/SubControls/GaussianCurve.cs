using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class GaussianCurve : CurveBase
    {
        private CurveMapperControlPoint controlPoint3;
        private CurveMapperControlPoint controlPoint4;
        private PolyLineSegment polySegmentLeft;
        private PolyLineSegment polySegmentRight;
        private double lastControlPoint2X;
        private double previousHorizontalOffset;

        /// <summary>
        /// Indicates whether the node is currently being resized, preventing unintended control point updates.
        /// </summary>
        public bool IsResizing { get; set; } = false;

        public GaussianCurve(CurveMapperControlPoint orthoControlPoint1,
            CurveMapperControlPoint orthoControlPoint2,
            CurveMapperControlPoint orthoControlPoint3,
            CurveMapperControlPoint orthoControlPoint4,
            double maxWidth, double maxHeight)
        {
            this.controlPoint1 = orthoControlPoint1; // Mean
            this.controlPoint2 = orthoControlPoint2; // Amplitude
            this.controlPoint3 = orthoControlPoint3; // Negative standard deviation
            this.controlPoint4 = orthoControlPoint4; // Positive standard deviation
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            lastControlPoint2X = controlPoint2.Point.X;
            previousHorizontalOffset = controlPoint2.Point.X - controlPoint3.Point.X;

            controlPoint2.PropertyChanged += ControlPoint2_Changed;
            controlPoint3.PropertyChanged += ControlPoint3_Changed;
            controlPoint4.PropertyChanged += ControlPoint4_Changed;

            // Ensure PathFigures and Segments are initialized
            PathFigure = new PathFigure();
            PathFigure2 = new PathFigure();

            GenerateGaussianCurve();

            PathFigure.Segments.Add(polySegmentLeft);
            PathFigure2.Segments.Add(polySegmentRight);

            PathGeometry = new PathGeometry()
            {
                Figures = { PathFigure, PathFigure2 }
            };

            PathCurve = new Path()
            {
                Data = PathGeometry,
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private void ControlPoint2_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (IsResizing) return;

            if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
            {
                double deltaX = controlPoint2.Point.X - lastControlPoint2X;

                if (deltaX == 0) return;

                // Temporarily unsubscribe events to prevent recursion
                controlPoint3.PropertyChanged -= ControlPoint3_Changed;
                controlPoint4.PropertyChanged -= ControlPoint4_Changed;

                controlPoint3.Point = new Point(controlPoint3.Point.X + deltaX, controlPoint3.Point.Y);
                controlPoint4.Point = new Point(controlPoint4.Point.X + deltaX, controlPoint4.Point.Y);
                lastControlPoint2X = controlPoint2.Point.X;

                // Re-subscribe events
                controlPoint3.PropertyChanged += ControlPoint3_Changed;
                controlPoint4.PropertyChanged += ControlPoint4_Changed;
            }
        }

        private void ControlPoint3_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (IsResizing) return;
            if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
            {
                var horizontalOffset = controlPoint2.Point.X - controlPoint3.Point.X;
                if (horizontalOffset == previousHorizontalOffset) return;

                // Temporarily unsubscribe events to prevent recursion
                controlPoint2.PropertyChanged -= ControlPoint2_Changed;
                controlPoint4.PropertyChanged -= ControlPoint4_Changed;

                controlPoint4.Point = new Point(controlPoint2.Point.X + horizontalOffset, controlPoint4.Point.Y);
                previousHorizontalOffset = horizontalOffset;

                // Re-subscribe events
                controlPoint2.PropertyChanged += ControlPoint2_Changed;
                controlPoint4.PropertyChanged += ControlPoint4_Changed;
            }
        }
        private void ControlPoint4_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (IsResizing) return;

            if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
            {
                var horizontalOffset = controlPoint2.Point.X - controlPoint4.Point.X;
                if (horizontalOffset == previousHorizontalOffset) return;

                // Temporarily unsubscribe events to prevent recursion
                controlPoint2.PropertyChanged -= ControlPoint2_Changed;
                controlPoint3.PropertyChanged -= ControlPoint3_Changed;

                controlPoint3.Point = new Point(controlPoint2.Point.X + horizontalOffset, controlPoint3.Point.Y);
                previousHorizontalOffset = horizontalOffset;

                // Re-subscribe events
                controlPoint2.PropertyChanged += ControlPoint2_Changed;
                controlPoint3.PropertyChanged += ControlPoint3_Changed;
            }
        }

        private void GenerateGaussianCurve()
        {
            bool peakX = false;
            double xLeft = double.NaN, yLeft = 0;
            double xRight = double.NaN, yRight = 0;

            polySegmentLeft ??= new PolyLineSegment { IsStroked = true };
            polySegmentRight ??= new PolyLineSegment { IsStroked = true };
            polySegmentLeft.Points.Clear();
            polySegmentRight.Points.Clear();

            // Compute Gaussian parameters
            double A = (MaxHeight - controlPoint1.Point.Y) * 4;
            double mu = controlPoint2.Point.X;
            double sigma = Math.Abs(controlPoint4.Point.X - controlPoint3.Point.X) / 4;
            if (sigma < 1) sigma = 1;
            
            if (controlPoint2.Point.X > 0)
            {
                xLeft = (A <= MaxHeight)
                    ? controlPoint2.Point.X
                    : mu - Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(MaxHeight / A));

                // Ensure xLeft stays within canvas bounds
                xLeft = Math.Max(xLeft, 0);
                yLeft = ComputeGaussianY(xLeft, A, mu, sigma);

                PathFigure.StartPoint = new Point(0, ComputeGaussianY(0, A, mu, sigma));

                for (double x = 0; x <= controlPoint2.Point.X; x += 2.0)
                {
                    double y = ComputeGaussianY(x, A, mu, sigma);

                    if (y >= 0 && y <= MaxHeight)
                    {
                        if (!peakX) polySegmentLeft.Points.Add(new Point(x, y));
                    }
                    else
                    {
                        peakX = true;
                    }
                }

                polySegmentLeft.Points.Add(new Point(xLeft, yLeft));
            }

            // Calculate Right Path
            if (controlPoint2.Point.X < MaxWidth)
            {
                xRight = (A <= MaxHeight)
                    ? controlPoint2.Point.X
                    : mu + Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(MaxHeight / A));

                // Ensure xRight stays within canvas bounds
                xRight = Math.Min(xRight, MaxWidth);
                yRight = ComputeGaussianY(xRight, A, mu, sigma);

                PathFigure2.StartPoint = new Point(xRight, yRight);

                for (double x = controlPoint2.Point.X; x <= MaxWidth; x += 2.0)
                {
                    double y = ComputeGaussianY(x, A, mu, sigma);

                    if (y >= 0 && y <= MaxHeight)
                    {
                        polySegmentRight.Points.Add(new Point(x, y));
                    }
                }
                polySegmentRight.Points.Add(new Point(MaxWidth, ComputeGaussianY(MaxWidth, A, mu, sigma)));
            }
        }

        private double ComputeGaussianY(double x, double A, double mu, double sigma)
        {
            double exponent = -Math.Pow(x - mu, 2) / (2 * Math.Pow(sigma, 2));
            double normalizedY = A * Math.Exp(exponent);

            return MaxHeight - normalizedY; // Convert to canvas coordinates
        }

        /// <summary>
        /// Regenerates the Gaussian curve when the control points are updated.
        /// </summary>
        public override void Regenerate()
        {
            GenerateGaussianCurve();
        }

        /// <summary>
        /// Calculates the Y-values for the curve based on input limits and point count.
        /// </summary>
        public override List<double> GetCurveYValues(double minY, double maxY, int count)
        {
            if (count < 1) return null;

            var values = new List<double>();

            double step = MaxWidth / (count - 1);
            double A = MaxHeight - controlPoint1.Point.Y;

            double mu = controlPoint2.Point.X;
            double sigma = Math.Abs(controlPoint4.Point.X - controlPoint3.Point.X) / 4;

            if (sigma < 1) sigma = 1; // Prevent division by zero

            for (int i = 0; i < count; i++)
            {
                double x = i * step;
                double normalizedY = ComputeGaussianY(x, A * 4, mu, sigma);
                double scaledY = minY + ((maxY - minY) * (1 - (normalizedY / MaxHeight)));

                values.Add(Math.Round(scaledY, rounding));
            }

            return values;
        }
    }
}
