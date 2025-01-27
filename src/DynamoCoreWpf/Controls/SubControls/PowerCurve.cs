using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class PowerCurve : CurveBase
    {
        private PolyLineSegment polySegment;
        private const double powerFactor = 2.0; // Adjust for different curve behaviors

        /// <summary>
        /// Initializes a power curve with a single control point.
        /// </summary>
        public PowerCurve(CurveMapperControlPoint controlPoint, double maxWidth, double maxHeight)
        {
            controlPoint1 = controlPoint;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            PathFigure = new PathFigure();

            // Generate the curve
            GeneratePowerCurve();

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

        /// <summary>
        /// Computes the power factor dynamically based on the control point position.
        /// </summary>
        private double UpdatePowerEquation()
        {
            // Normalize a & y of the control point
            double xControl = controlPoint1.Point.X / MaxWidth;
            double yControl = 1 - (controlPoint1.Point.Y / MaxHeight);

            if (xControl == yControl)
                return 1.0;

            if (xControl <= 0.0 || yControl >= 1.0)
                return double.NegativeInfinity;

            if (xControl >= 1.0 || yControl <= 0.0)
                return double.PositiveInfinity;

            return Math.Log(yControl) / Math.Log(xControl);
        }

        private void GeneratePowerCurve()
        {
            if (polySegment == null)
            {
                polySegment = new PolyLineSegment();
                polySegment.IsStroked = true;
            }
            else
            {
                polySegment.Points.Clear();
            }

            PathFigure.StartPoint = new Point(0, MaxHeight);

            double powerFactor = UpdatePowerEquation();

            for (double x = 0; x <= MaxWidth; x += 2.0)
            {
                double y = ComputePowerY(x, powerFactor);
                polySegment.Points.Add(new Point(x, y));
            }

            // Ensure the curve reaches the final point
            polySegment.Points.Add(new Point(MaxWidth, ComputePowerY(MaxWidth, powerFactor)));
        }

        private double ComputePowerY(double x, double powerFactor)
        {
            // Normalize x to [0,1] & compute the function normally
            double baseX = x / MaxWidth;
            double normalizedY = Math.Pow(baseX, powerFactor);

            // Convert to canvas coordinates (invert Y)
            return (1 - normalizedY) * MaxHeight;
        }

        /// <summary>
        /// Regenerates the power curve when the control point is updated.
        /// </summary>
        public override void Regenerate()
        {
            GeneratePowerCurve();
        }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and point count.
        /// </summary>
        public override List<double> GetCurveYValues(double minY, double maxY, int count)
        {
            if (count < 1) return null;

            var values = new List<double>();
            double step = MaxWidth / (count - 1);
            double powerFactor = UpdatePowerEquation();

            for (int i = 0; i < count; i++)
            {
                double x = i * step;
                double normalizedY = ComputePowerY(x, powerFactor) / MaxHeight;
                double scaledY = maxY - ((maxY - minY) * normalizedY);
                values.Add(Math.Round(scaledY, rounding));
            }

            return values;
        }
    }
}
