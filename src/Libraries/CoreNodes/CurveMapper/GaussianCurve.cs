using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a Gaussian curve in the CurveMapper.
    /// The Gaussian curve follows a bell-shaped distribution defined by four control points.
    /// </summary>
    public class GaussianCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;
        private double ControlPoint3X;
        private double ControlPoint3Y;
        private double ControlPoint4X;
        private double ControlPoint4Y;

        private double lastControlPoint2X;
        private double previousHorizontalOffset;

        /// <summary>
        /// Indicates whether the node is currently being resized, preventing unintended control point updates.
        /// </summary>
        public GaussianCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double cp3X, double cp3Y, double cp4X, double cp4Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
            ControlPoint3X = cp3X;
            ControlPoint3Y = cp3Y;
            ControlPoint4X = cp4X;
            ControlPoint4Y = cp4Y;
        }

        private double ComputeGaussianY(double x, double A, double mu, double sigma)
        {
            double exponent = -Math.Pow(x - mu, 2) / (2 * Math.Pow(sigma, 2));
            double normalizedY = A * Math.Exp(exponent);

            return CanvasSize - normalizedY;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            // Compute Gaussian parameters
            double A = ControlPoint1Y * 4;
            double mu = ControlPoint2X;
            double sigma = Math.Abs(ControlPoint4X - ControlPoint3X) / 4;
            if (sigma < 1) sigma = 1;

            if (isRender)
            {
                for (double x = 0; x <= ControlPoint2X; x += renderIncrementX)
                {
                    double y = CanvasSize - ComputeGaussianY(x, A, mu, sigma);

                    if (y <= CanvasSize)
                    {
                        valuesX.Add(x);
                        valuesY.Add(y);
                    }
                    else
                    {
                        // Find the x value for which y == CanvasSize
                        double xAtCanvasSize = mu - Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));
                        xAtCanvasSize = Math.Max(xAtCanvasSize, 0);

                        if (!valuesX.Contains(xAtCanvasSize))
                        {
                            valuesX.Add(xAtCanvasSize);
                            valuesY.Add(CanvasSize);
                        }
                        break;
                    }
                }
                for (double x = ControlPoint2X; x <= CanvasSize; x += renderIncrementX)
                {
                    double y = CanvasSize - ComputeGaussianY(x, A, mu, sigma);

                    if (y <= CanvasSize)
                    {
                        valuesX.Add(x);
                        valuesY.Add(y);
                    }
                    else
                    {
                        // Find the x value for which y == CanvasSize
                        double xAtCanvasSize = mu + Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));
                        xAtCanvasSize = Math.Max(xAtCanvasSize, 0);

                        if (!valuesX.Contains(xAtCanvasSize))
                        {
                            valuesX.Add(xAtCanvasSize);
                            valuesY.Add(CanvasSize);
                        }
                    }
                }
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = pointsDomain[0];
                var step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = 0 + i * step;
                    valuesX.Add(x);
                    valuesY.Add(CanvasSize - ComputeGaussianY(x, A, mu, sigma));
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => CanvasSize - ComputeGaussianY(x, A, mu, sigma));
            }

            return (valuesX, valuesY);
        }
    }
}
