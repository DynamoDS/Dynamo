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

            // Initialize coefficients and generate sine wave
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
            //double x1 = controlPoint1.Point.X / MaxWidth;
            //double y1 = 1 - (controlPoint1.Point.Y / MaxHeight);
            //double x2 = controlPoint2.Point.X / MaxWidth;
            //double y2 = 1 - (controlPoint2.Point.Y / MaxHeight);

            //if (x2 == x1)
            //    return double.NaN; // Avoid division by zero

            //return y2 / Math.Sqrt(x2);

            double Ox = controlPoint1.Point.X / MaxWidth; // Origin X (controlPoint1)
            double Oy = 1 - (controlPoint1.Point.Y / MaxHeight); // Origin Y (controlPoint1)
            double Gx = controlPoint2.Point.X / MaxWidth; // Control point X (controlPoint2)
            double Gy = 1 - (controlPoint2.Point.Y / MaxHeight); // Control point Y (controlPoint2)

            if (Gx == Ox) return double.NaN; // Prevent division by zero

            return (Gy - Oy) / Math.Sqrt(Math.Abs(Gx - Ox)); // Updated scaling factor
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

            //PathFigure.StartPoint = new Point(0, MaxHeight);

            //double sqrtFactor = ComputeSquareRootFactor();

            ////for (double x = 0; x <= MaxWidth; x += 2.0)
            ////{
            ////    double y = ComputeSquareRootY(x, sqrtFactor);
            ////    polySegment.Points.Add(new Point(x, y));
            ////}


            //for (double x = 0; x <= MaxWidth; x += 2.0)
            //{
            //    double y = ComputeSquareRootY(x, sqrtFactor);

            //    // Only add points that are within the visible canvas
            //    if (y >= 0 && y <= MaxHeight)
            //    {
            //        polySegment.Points.Add(new Point(x, y));
            //    }
            //}

            //// Ensure the curve reaches the final point if it's within the canvas
            //double finalY = ComputeSquareRootY(MaxWidth, sqrtFactor);
            //if (finalY >= 0 && finalY <= MaxHeight)
            //{
            //    polySegment.Points.Add(new Point(MaxWidth, finalY));
            //}






            double sqrtFactor = ComputeSquareRootFactor();

            // Find the first valid X-value where the curve is within the visible canvas
            double startX = 0;
            for (double x = 0; x <= MaxWidth; x += 2.0)
            {
                double y = ComputeSquareRootY(x, sqrtFactor);
                if (y >= 0 && y <= MaxHeight)
                {
                    startX = x;
                    break; // Stop once we find the first valid point
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

            // Ensure the curve reaches the final point if it's within the canvas
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
            double Ox = controlPoint1.Point.X / MaxWidth; // Origin X
            double Oy = 1 - (controlPoint1.Point.Y / MaxHeight); // Origin Y

            double baseX = x / MaxWidth;
            double adjustedX = baseX - Ox; // Shift X to consider controlPoint1

            double sqrtComponent = sqrtFactor * Math.Sqrt(Math.Abs(adjustedX));

            // Mirror the Y-values if X is to the left of controlPoint1
            double normalizedY = Oy + (adjustedX < 0 ? -sqrtComponent : sqrtComponent);

            return (1 - normalizedY) * MaxHeight; // Convert to canvas coordinates
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
                double scaledY = maxY - ((maxY - minY) * normalizedY); // Flip Y values for output
                values.Add(Math.Round(scaledY, rounding));
            }

            return values;
        }
    }
}
