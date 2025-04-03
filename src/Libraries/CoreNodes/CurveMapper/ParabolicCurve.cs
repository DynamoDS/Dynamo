using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a parabolic curve in the CurveMapper.
    /// The curve follows a quadratic equation based on two control points.
    /// </summary>
    public class ParabolicCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        public ParabolicCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }

        private double SolveParabolaForX(double y, bool isNegative = false)
        {
            double a = Math.Pow(ControlPoint2X - ControlPoint1X, 2) / (4.0 * (ControlPoint2Y - ControlPoint1Y));
            double h = ControlPoint1X;
            double k = ControlPoint1Y;
            return ((isNegative) ? -1.0 : 1.0) * Math.Sqrt(4.0 * a * (y - k)) + h;
        }

        private double SolveParabolaForY(double x)
        {
            double a = Math.Pow(ControlPoint2X - ControlPoint1X, 2) / (4.0 * (ControlPoint2Y - ControlPoint1Y));
            double h = ControlPoint1X;
            double k = ControlPoint1Y;
            return (Math.Pow(x - h, 2) / (4 * a)) + k;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender)
        {
            double leftBoundaryY = (ControlPoint2Y > ControlPoint1Y) ? CanvasSize : 0.0;
            double rightBoundaryY = (ControlPoint2Y < ControlPoint1Y) ? CanvasSize : 0.0;
            double startX = SolveParabolaForX(leftBoundaryY, true);
            double endX = SolveParabolaForX(leftBoundaryY);

            var valuesX = new List<double>();
            var valuesY = new List<double>();

            if (isRender)
            {
                double minX = Math.Max(0, Math.Min(startX, endX));
                double maxX = Math.Min(CanvasSize, Math.Max(startX, endX));

                // First point
                double firstY = SolveParabolaForY(minX);

                valuesX.Add(minX);
                valuesY.Add(firstY);

                for (double d = minX; d < maxX; d += renderIncrementX)
                {
                    double vy = SolveParabolaForY(d);
                    if (vy >= 0 && vy < CanvasSize)
                    {
                        valuesX.Add(d);
                        valuesY.Add(vy);
                    }
                }

                // Last point
                valuesX.Add(maxX);
                valuesY.Add(SolveParabolaForY(maxX));

                return (valuesX, valuesY);
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = pointsDomain[0];

                var step = (rightBoundaryY - leftBoundaryY) / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double d = leftBoundaryY + i * step;
                    double vy = SolveParabolaForY(d);

                    valuesX.Add(d);
                    valuesY.Add(vy);
                }

                // Reverse lists if needed to ensure X values increase from left to right
                if (ControlPoint2Y > ControlPoint1Y)
                {
                    valuesX.Reverse();
                    valuesY.Reverse();
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => SolveParabolaForY(x));
            }

            return (valuesX, valuesY);
        }
    }
}
