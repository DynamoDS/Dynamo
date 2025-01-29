using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class SquareRootCurve : CurveBase
    {
        private PolyLineSegment polySegment;

        /// <summary>
        /// Initializes a square root curve with two control points.
        /// </summary>
        public SquareRootCurve(CurveMapperControlPoint controlPoint1, CurveMapperControlPoint controlPoint2, double maxWidth, double maxHeight)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            PathFigure = new PathFigure();

            GenerateSquareRootCurve();

            PathFigure.Segments.Add(polySegment);

            PathGeometry = new PathGeometry
            {
                Figures = { PathFigure }
            };

            PathCurve = new Path
            {
                Data = PathGeometry,
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private double ComputeSquareRootFactor()
        {
            double Ox = controlPoint1.Point.X / MaxWidth;
            double Oy = 1 - (controlPoint1.Point.Y / MaxHeight);
            double Gx = controlPoint2.Point.X / MaxWidth;
            double Gy = 1 - (controlPoint2.Point.Y / MaxHeight);

            if (Gx == Ox) return double.NaN;

            return (Gy - Oy) / Math.Sqrt(Math.Abs(Gx - Ox));
        }

        private void GenerateSquareRootCurve()
        {
            if (polySegment == null)
            {
                polySegment = new PolyLineSegment { IsStroked = true };
            }
            else
            {
                polySegment.Points.Clear();
            }

            double sqrtFactor = ComputeSquareRootFactor();

            // Find the first valid X-value where the curve is within the visible canvas
            double startX = 0;
            for (double x = 0; x <= MaxWidth; x += 2.0)
            {
                double y = ComputeSquareRootY(x, sqrtFactor);
                if (y >= 0 && y <= MaxHeight)
                {
                    startX = x;
                    break;
                }
            }

            // Set the correct start point dynamically
            PathFigure.StartPoint = new Point(startX, ComputeSquareRootY(startX, sqrtFactor));

            for (double x = startX; x <= MaxWidth; x += 2.0)
            {
                double y = ComputeSquareRootY(x, sqrtFactor);

                // Only add points that are within the visible canvas
                if (y >= 0 && y <= MaxHeight)
                {
                    polySegment.Points.Add(new Point(x, y));
                }
            }

            double finalY = ComputeSquareRootY(MaxWidth, sqrtFactor);
            if (finalY >= 0 && finalY <= MaxHeight)
            {
                polySegment.Points.Add(new Point(MaxWidth, finalY));
            }
        }

        /// <summary>
        /// Computes the Y value for a given X based on the square root equation.
        /// </summary>
        private double ComputeSquareRootY(double x, double sqrtFactor)
        {
            double Ox = controlPoint1.Point.X / MaxWidth;
            double Oy = 1 - (controlPoint1.Point.Y / MaxHeight);

            double baseX = x / MaxWidth;
            double adjustedX = baseX - Ox;

            double sqrtComponent = sqrtFactor * Math.Sqrt(Math.Abs(adjustedX));

            // Mirror the Y-values if X is to the left of controlPoint1
            double normalizedY = Oy + (adjustedX < 0 ? -sqrtComponent : sqrtComponent);
            return (1 - normalizedY) * MaxHeight;
        }

        /// <summary>
        /// Regenerates the square root curve when control points are updated.
        /// </summary>
        public override void Regenerate()
        {
            GenerateSquareRootCurve();
        }

        /// <summary>
        /// Calculates the Y-values for the square root curve.
        /// </summary>
        public override List<double> GetCurveYValues(double minY, double maxY, int count)
        {
            if (count < 1) return null;

            var values = new List<double>();
            double step = MaxWidth / (count - 1);
            double sqrtFactor = ComputeSquareRootFactor();

            for (int i = 0; i < count; i++)
            {
                double x = i * step;
                double normalizedY = ComputeSquareRootY(x, sqrtFactor) / MaxHeight;
                double scaledY = maxY - ((maxY - minY) * normalizedY);
                values.Add(Math.Round(scaledY, rounding));
            }

            return values;
        }
    }
}
