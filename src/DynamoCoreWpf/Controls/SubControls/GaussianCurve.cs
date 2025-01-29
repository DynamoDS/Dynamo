using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class GaussianCurve : CurveBase
    {
        private CurveMapperControlPoint controlPoint3;
        private CurveMapperControlPoint controlPoint4;
        private PolyLineSegment polySegment;

        public GaussianCurve(CurveMapperControlPoint orthoControlPoint1,
            CurveMapperControlPoint orthoControlPoint2,
            CurveMapperControlPoint orthoControlPoint3,
            CurveMapperControlPoint orthoControlPoint4,
            double maxWidth, double maxHeight)
        {
            this.controlPoint1 = orthoControlPoint1;
            this.controlPoint2 = orthoControlPoint2;
            this.controlPoint3 = orthoControlPoint3;
            this.controlPoint4 = orthoControlPoint4;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            PathFigure = new PathFigure();

            GenerateGaussianCurve();

            PathFigure.Segments.Add(polySegment);

            PathGeometry = new PathGeometry()
            {
                Figures = { PathFigure }
            };

            PathCurve = new Path()
            {
                Data = PathGeometry,
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private void GenerateGaussianCurve()
        {
            if (polySegment == null)
                polySegment = new PolyLineSegment { IsStroked = true };
            else
                polySegment.Points.Clear();

            // Compute parameters based on control points
            double A = (MaxHeight - controlPoint1.Point.Y) * 4; // Peak height
            double mu = controlPoint2.Point.X;            // Center shift
            double sigma = Math.Abs(controlPoint4.Point.X - controlPoint3.Point.X) / 4; // Spread

            if (sigma < 1) sigma = 1; // Prevent division by zero

            PathFigure.StartPoint = new Point(0, ComputeGaussianY(0, A, mu, sigma));

            for (double x = 0; x <= MaxWidth; x += 2.0)
            {
                double y = ComputeGaussianY(x, A, mu, sigma);

                if (y >= 0 && y <= MaxHeight) // Ensure it's within the visible canvas
                {
                    polySegment.Points.Add(new Point(x, y));
                }
            }

            // Ensure the curve reaches the final point within bounds
            double finalY = ComputeGaussianY(MaxWidth, A, mu, sigma);
            if (finalY >= 0 && finalY <= MaxHeight)
            {
                polySegment.Points.Add(new Point(MaxWidth, finalY));
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
                double normalizedY = ComputeGaussianY(x, A, mu, sigma);
                double scaledY = minY + ((maxY - minY) * normalizedY / MaxHeight);
                values.Add(Math.Round(scaledY, rounding));
            }

            return values;
        }
    }
}
