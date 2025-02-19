using System;
using System.Collections.Generic;

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

            return ((y - ControlPoint1Y) / slope) + ControlPoint1X;
        }

        /// <summary>
        /// Returns X values distributed across the curve.
        /// </summary>
        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            double leftX = SolveForXGivenY(0);
            double rightX = SolveForXGivenY(CanvasSize);

            if (ControlPoint1X > ControlPoint2X)
            {
                (leftX, rightX) = (rightX, leftX); // Swap values if needed
            }                

            if (isRender)
            {
                return new List<double>
                {
                    Math.Clamp(leftX, 0, CanvasSize),
                    Math.Clamp(rightX, 0, CanvasSize)
                };
            }

            var values = new List<double>();
            double step = CanvasSize / (pointsCount - 1);

            for (int i = 0; i < pointsCount; i++)
            {
                values.Add(i * step);
            }

            return values;
        }

        /// <summary>
        /// Returns Y values distributed across the curve.
        /// </summary>
        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            double lowY = LineEquation(0);
            double highY = LineEquation(CanvasSize);

            if (ControlPoint1Y > ControlPoint2Y)
            {
                (lowY, highY) = (highY, lowY);
            }

            if (isRender)
            {
                return new List<double>
                {
                    Math.Clamp(lowY, 0, CanvasSize),
                    Math.Clamp(highY, 0, CanvasSize)
                };
            }

            var values = new List<double>();
            double step = (highY - lowY) / (pointsCount - 1);

            for (int i = 0; i < pointsCount; i++)
            {
                values.Add(lowY + i * step);
            }

            if (ControlPoint1Y > ControlPoint2Y)
            {
                values.Reverse();
            }

            return values;
        }
    }
}
