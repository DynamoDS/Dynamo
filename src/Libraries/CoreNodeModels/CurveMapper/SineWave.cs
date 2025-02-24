using System;
using System.Collections.Generic;

namespace CoreNodeModels.CurveMapper
{
    public class SineWave : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        // Coefficients
        private double coefA;   // Amplitude
        private double coefB;   // 2*PI/period
        private double coefC;   // Phase shift
        private double coefD;   // Vertical shift

        public SineWave(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
            : base(canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
            GetEquationCoefficients();
        }

        private double ConvertXToTrigo(double x) => Math.PI * x / CanvasSize;
        private double ConvertYToTrigo(double y) => 2.0 * y / CanvasSize - 1.0;
        private double ConvertTrigoToX(double unitX) => unitX * CanvasSize / Math.PI;
        private double ConvertTrigoToY(double unitY) => (unitY + 1.0) * CanvasSize / (2.0);
        private double CosineEquation(double x) => -(coefA * Math.Cos(coefB * x - coefC)) + coefD;

        private void GetEquationCoefficients()
        {
            coefA = (ConvertYToTrigo(ControlPoint2Y) - ConvertYToTrigo(ControlPoint1Y)) / 2.0;
            coefB = Math.PI / (ConvertXToTrigo(ControlPoint2X) - ConvertXToTrigo(ControlPoint1X));
            coefC = coefB * ConvertXToTrigo(ControlPoint1X);
            coefD = (ConvertYToTrigo(ControlPoint1Y) + ConvertYToTrigo(ControlPoint2Y)) / 2.0;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(int pointsCount, bool isRender = false)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            if (isRender)
            {
                for (double x = 0.0; x < CanvasSize; x += renderIncrementX)
                {
                    valuesX.Add(x);
                    double y = CosineEquation(ConvertXToTrigo(x));
                    valuesY.Add(ConvertTrigoToY(y));
                }
            }
            else
            {
                double step = CanvasSize / (pointsCount - 1);
                for (int i = 0; i < pointsCount; i++)
                {
                    double x = i * step;
                    valuesX.Add(x);
                    double y = CosineEquation(ConvertXToTrigo(x));
                    valuesY.Add(ConvertTrigoToY(y));
                }
            }

            return (valuesX, valuesY);
        }
    }
}
