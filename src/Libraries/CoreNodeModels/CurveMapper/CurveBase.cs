using System.Collections.Generic;

namespace CoreNodeModels.CurveMapper
{
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
        protected abstract (List<double> XValues, List<double> YValues) GenerateCurve(int pointsCount, bool isRender);

        /// <summary>
        /// Common method for retrieving X values.
        /// </summary>
        public virtual List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            return GenerateCurve(pointsCount, isRender).XValues;
        }

        /// <summary>
        /// Common method for retrieving Y values.
        /// </summary>
        public virtual List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            return GenerateCurve(pointsCount, isRender).YValues;
        }
    }
}
