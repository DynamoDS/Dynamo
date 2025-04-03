using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a power function curve in the CurveMapper.
    /// The curve is defined by a power equation derived from a control point.
    /// </summary>
    public class PowerCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;

        public PowerCurve(double cp1X, double cp1Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
        }

        private double UpdatePowerEquation()
        {
            // Normalize x & y of the control point
            double xControl = ControlPoint1X / CanvasSize;
            double yControl = ControlPoint1Y / CanvasSize;

            if (xControl == yControl)
                return 1.0;

            if (xControl <= 0.0 || yControl >= 1.0)
                return double.NegativeInfinity;

            if (xControl >= 1.0 || yControl <= 0.0)
                return double.PositiveInfinity;

            return Math.Log(yControl) / Math.Log(xControl);
        }


        private double ComputePowerY(double x, double powerFactor)
        {
            double baseX = x / CanvasSize;
            double normalizedY = Math.Pow(baseX, powerFactor);

            return normalizedY * CanvasSize;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();
            double powerFactor = UpdatePowerEquation();

            if (isRender)
            {
                for (double x = 0.0; x <= CanvasSize; x += renderIncrementX)
                {
                    valuesX.Add(x);
                    double y = ComputePowerY(x, powerFactor);
                    valuesY.Add(y);
                }
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = (int)pointsDomain[0];
                double step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = i * step;
                    valuesX.Add(x);
                    valuesY.Add(ComputePowerY(x, powerFactor));
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => ComputePowerY(x, powerFactor));
            }

            return (valuesX, valuesY);
        }
    }
}
