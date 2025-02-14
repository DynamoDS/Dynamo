using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class PowerCurve
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private const double renderIncrementX = 1.0; // ADD THIS BASE CLASS ?

        public PowerCurve(double cp1X, double cp1Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
        }

        private double UpdatePowerEquation()
        {
            // Normalize a & y of the control point
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

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            var values = new List<double>();
            double powerFactor = UpdatePowerEquation();

            if (isRender)
            {
                for (double i = 0.0; i < CanvasSize; i += renderIncrementX)
                {
                    double x = i;
                    double y = ComputePowerY(x, powerFactor);
                    values.Add(y);
                }

                // Last value
                values.Add(ComputePowerY(CanvasSize, powerFactor));
            }
            else
            {
                double step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = i * step;
                    double y = ComputePowerY(x, powerFactor);
                    values.Add(y);
                }
            }

            return values;
        }

        public List<double> GetCurveXValues(int pointsCount, bool isRender = false) // TODO : MOVE TO BASE CLASS
        {
            var values = new List<double>();

            if (isRender)
            {
                for (double i = 0.0; i < CanvasSize; i += renderIncrementX)
                {
                    values.Add(i);
                }
                values.Add(CanvasSize);

                return values;
            }
            else
            {
                double step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double d = 0 + i * step;
                    values.Add(d);
                }
            }

            return values;
        }
    }
}
