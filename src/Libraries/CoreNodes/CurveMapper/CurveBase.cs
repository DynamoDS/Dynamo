using System;
using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a base class for all curve types in the CurveMapper.
    /// Provides common functionality for generating and retrieving curve values.
    /// </summary>
    public abstract class CurveBase
    {
        protected double CanvasSize;
        protected const double renderIncrementX = 1.0;

        protected CurveBase(double canvasSize)
        {
            CanvasSize = canvasSize;
        }

        /// <summary>
        /// Abstract method to be implemented by derived classes for generating curve values.
        /// </summary>
        protected abstract (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsCount, bool isRender);

        /// <summary>
        /// Common method for retrieving X values.
        /// </summary>
        public virtual List<double> GetCurveXValues(List<double> pointsCount, bool isRender = false)
        {
            return GenerateCurve(pointsCount, isRender).XValues;
        }

        /// <summary>
        /// Common method for retrieving Y values.
        /// </summary>
        public virtual List<double> GetCurveYValues(List<double> pointsCount, bool isRender = false)
        {
            return GenerateCurve(pointsCount, isRender).YValues;
        }

        /// <summary>
        /// Generates X and Y values by mapping domain inputs to canvas space and evaluating a curve function.
        /// </summary>
        protected (List<double> xValues, List<double> yValues) GenerateFromDomain(List<double> domain, Func<double, double> computeY)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            double minInput = domain.Min();
            double maxInput = domain.Max();

            foreach (var t in domain)
            {
                // Normalize domain value & map to canvas X coordinate
                double normalizedT = (t - minInput) / (maxInput - minInput);
                double x = normalizedT * CanvasSize;

                valuesX.Add(x);
                valuesY.Add(computeY(x));
            }

            return (valuesX, valuesY);
        }
    }
}
