using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class SquareRootCurve
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;
        private const double renderIncrementX = 1.0; // ADD THIS BASE CLASS ?

        public SquareRootCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
        {
            CanvasSize = canvasSize;
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

        private List<double>[] GenerateSquareRootCurve(int pointsCount, bool isRender)
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
            else
            {
                var step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = 0 + i * step;
                    double y = ComputeSquareRootY(x, sqrtFactor);

                    valuesX.Add(x);
                    valuesY.Add(y);
                }
            }

            return [valuesX, valuesY];
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

        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            return GenerateSquareRootCurve(pointsCount, isRender)[0];
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            return GenerateSquareRootCurve(pointsCount, isRender)[1];
        }
    }
}
