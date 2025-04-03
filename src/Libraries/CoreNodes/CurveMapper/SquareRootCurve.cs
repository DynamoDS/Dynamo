using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a square root curve in the CurveMapper.
    /// The curve follows a square root function and is influenced by two control points.
    /// </summary>
    public class SquareRootCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        public SquareRootCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }

        private double ComputeSquareRootFactor()
        {
            double Ox = ControlPoint1X / CanvasSize;
            double Oy = 1 - (ControlPoint1Y / CanvasSize);
            double Gx = ControlPoint2X / CanvasSize;
            double Gy = 1 - (ControlPoint2Y / CanvasSize);

            double deltaX = Gx - Ox;
            if (deltaX == 0) return double.NaN;

            double sqrtFactor = (Gy - Oy) / Math.Sqrt(Math.Abs(deltaX));

            // If controlPoint1 is to the right of controlPoint2, flip the curve
            return (Gx < Ox) ? -sqrtFactor : sqrtFactor;
        }

        private double ComputeSquareRootY(double x, double sqrtFactor)
        {
            double Ox = ControlPoint1X / CanvasSize;
            double Oy = 1 - (ControlPoint1Y / CanvasSize);

            double baseX = x / CanvasSize;
            double adjustedX = baseX - Ox;

            double sqrtComponent = sqrtFactor * Math.Sqrt(Math.Abs(adjustedX));

            // Mirror the Y-values if X is to the left of controlPoint1
            double normalizedY = Oy + (adjustedX < 0 ? -sqrtComponent : sqrtComponent);
            return (1 - normalizedY) * CanvasSize;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            double sqrtFactor = ComputeSquareRootFactor();

            // Find the first valid X-value where the curve is within the visible canvas
            double startX = 0;

            for (double x = 0; x <= CanvasSize; x += renderIncrementX)
            {
                double y = ComputeSquareRootY(x, sqrtFactor);
                if (y >= 0 && y <= CanvasSize)
                {
                    startX = x;
                    break;
                }
            }

            if (isRender)
            {
                for (double x = startX; x <= CanvasSize; x += renderIncrementX)
                {
                    double y = ComputeSquareRootY(x, sqrtFactor);

                    // Only add points that are within the visible canvas
                    if (y >= 0 && y <= CanvasSize)
                    {
                        valuesX.Add(x);
                        valuesY.Add(y);
                    }
                }
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = (int)pointsDomain[0];
                var step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = 0 + i * step;

                    valuesX.Add(x);
                    valuesY.Add(ComputeSquareRootY(x, sqrtFactor));
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => ComputeSquareRootY(x, sqrtFactor));
            }

            return (valuesX, valuesY);
        }
    }
}
