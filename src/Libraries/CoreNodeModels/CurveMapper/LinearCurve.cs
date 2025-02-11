using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class LinearCurve
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        public LinearCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }

        // Calculates the Y values (canvas coordinates) for min and max X values
        private double LineEquation(double x)
        {
            double dx = ControlPoint2X - ControlPoint1X;
            double dy = ControlPoint2Y - ControlPoint1Y;
            if (Math.Abs(dx) < double.Epsilon) return double.NaN;

            return dy / dx * (x - ControlPoint1X) + ControlPoint1Y;
        }
        // Calculates the X values (canvas coordinates) for bottom and to edge of the canvas 
        private double SolveForXGivenY(double y)
        {
            double slope = (ControlPoint2Y - ControlPoint1Y) / (ControlPoint2X - ControlPoint1X);
            if (double.IsNaN(slope) || Math.Abs(ControlPoint2X - ControlPoint1X) < double.Epsilon)
            {
                return double.NaN;
            }

            var result = ((y - ControlPoint1Y) / slope) + ControlPoint1X;

            return result;
        }

        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            var leftX = double.NaN;
            var rightX = double.NaN;

            if (ControlPoint1X < ControlPoint2X)
            {
                leftX = SolveForXGivenY(0);
                rightX = SolveForXGivenY(CanvasSize);
            }
            else
            {
                leftX = SolveForXGivenY(CanvasSize);
                rightX = SolveForXGivenY(0);
            }

            var c1 = leftX;
            var c2 = rightX;

            if (isRender)
            {
                var cp1Xrender = Math.Max(leftX, 0);
                cp1Xrender = Math.Min(cp1Xrender, CanvasSize);

                var cp2Xrender = Math.Max(rightX, 0);
                cp2Xrender = Math.Min(cp2Xrender, CanvasSize);

                    return new List<double> { cp1Xrender, cp2Xrender };
            }

            var values = new List<double>();
            double step = CanvasSize / (pointsCount - 1);

            for (int i = 0; i < pointsCount; i++)
            {
                double d = 0 + i * step;
                values.Add(d);
            }

            return values;
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            var lowY = double.NaN;
            var highY = double.NaN;

            lowY = LineEquation(0);
            highY = LineEquation(CanvasSize);

            if (isRender)
            {
                if (ControlPoint1Y > ControlPoint2Y)
                {
                    highY = LineEquation(0);
                    lowY = LineEquation(CanvasSize);
                }

                var cp1Yrender = Math.Max(lowY, 0);
                cp1Yrender = Math.Min(cp1Yrender, CanvasSize);

                var cp2Yrender = Math.Max(highY, 0);
                cp2Yrender = Math.Min(cp2Yrender, CanvasSize);

                return new List<double> { cp1Yrender, cp2Yrender };
            }

            var values = new List<double>();
            var range = highY - lowY;
            double step = range / (pointsCount - 1);

            for (int i = 0; i < pointsCount; i++)
            {
                double d = lowY + i * step;
                values.Add(d);
            }

            return values;
        }
    }
}
