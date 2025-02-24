using System;
using System.Collections.Generic;

namespace CoreNodeModels.CurveMapper
{
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
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(int pointsCount, bool isRender)
        {
            double leftBoundaryY = (ControlPoint2Y > ControlPoint1Y) ? CanvasSize : 0.0;
            double rightBoundaryY = (ControlPoint2Y < ControlPoint1Y) ? CanvasSize : 0.0;

            double startX = SolveParabolaForX(leftBoundaryY, true);
            double endX = SolveParabolaForX(leftBoundaryY);

            if (isRender)
            {
                double minX = Math.Max(0, Math.Min(startX, endX));
                double maxX = Math.Min(CanvasSize, Math.Max(startX, endX));

                // First point
                double firstY = SolveParabolaForY(minX);

                var valuesX = new List<double> { minX };
                var valuesY = new List<double> { firstY };

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
            else
            {
                bool flip = ControlPoint2Y > ControlPoint1Y;

                // First point
                double firstY = SolveParabolaForY(leftBoundaryY);
                double firstX = leftBoundaryY;

                var valuesX = new List<double>();
                var valuesY = new List<double>();

                var step = (rightBoundaryY - leftBoundaryY) / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double d = leftBoundaryY + i * step;
                    double vy = SolveParabolaForY(d);

                    valuesX.Add(d);
                    valuesY.Add(vy);
                }

                // Reverse lists if needed to ensure X values increase from left to right
                if (flip)
                {
                    valuesX.Reverse();
                    valuesY.Reverse();
                }

                return (valuesX, valuesY);
            }
        }
    }
}
