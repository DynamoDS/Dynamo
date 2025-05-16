using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a linear curve in the CurveMapper.
    /// A linear curve is a straight line between two control points.
    /// </summary>

    [IsVisibleInDynamoLibrary(false)]
    public class LinearCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        private bool flipHorizontally;
        private bool flipVertically;

        public LinearCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }

        /// <summary>
        /// Calculates the Y values (canvas coordinates) for min and max X values
        /// </summary>
        private double LineEquation(double x)
        {
            double dx = ControlPoint2X - ControlPoint1X;
            double dy = ControlPoint2Y - ControlPoint1Y;
            if (Math.Abs(dx) < double.Epsilon) return double.NaN;

            return dy / dx * (x - ControlPoint1X) + ControlPoint1Y;
        }

        /// <summary>
        /// Calculates the X values (canvas coordinates) for min and max Y values
        /// </summary>
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
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender = false)
        {
            double leftX = SolveForXGivenY(0);
            double rightX = SolveForXGivenY(CanvasSize);

            double lowY = LineEquation(0);
            double highY = LineEquation(CanvasSize);

            var valuesX = new List<double>();
            var valuesY = new List<double>();

            flipHorizontally = ControlPoint1X > ControlPoint2X;
            flipVertically = ControlPoint1Y > ControlPoint2Y;

            if (isRender)
            {
                valuesX = new List<double> { System.Math.Clamp(leftX, 0, CanvasSize), System.Math.Clamp(rightX, 0, CanvasSize) };

                valuesY = (flipHorizontally ^ flipVertically)
                    ? new List<double> { System.Math.Clamp(highY, 0, CanvasSize), System.Math.Clamp(lowY, 0, CanvasSize) }
                    : new List<double> { System.Math.Clamp(lowY, 0, CanvasSize), System.Math.Clamp(highY, 0, CanvasSize) };
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = (int)pointsDomain[0];

                // For full point distribution
                double stepX = CanvasSize / (pointsCount - 1);
                double stepY = (highY - lowY) / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    valuesX.Add(i * stepX);
                    valuesY.Add((lowY + i * stepY));
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => LineEquation(x));
            }

            return (valuesX, valuesY);
        }
    }
}
