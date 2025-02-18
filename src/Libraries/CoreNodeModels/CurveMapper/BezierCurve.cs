using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class BezierCurve
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;
        private double ControlPoint3X;
        private double ControlPoint3Y;
        private double ControlPoint4X;
        private double ControlPoint4Y;
        private const double renderIncrementX = 1.0; // ADD THIS BASE CLASS ?

        private Dictionary<double, double> xToYMap = new Dictionary<double, double>();
        private double tFactor;

        public BezierCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double cp3X, double cp3Y, double cp4X, double cp4Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
            ControlPoint3X = cp3X;
            ControlPoint3Y = cp3Y;
            ControlPoint4X = cp4X;
            ControlPoint4Y = cp4Y;

            tFactor = 1.0 / canvasSize;
        }

        private void GetValueAtT(double t, out double x, out double y)
        {
            x = Math.Pow(1 - t, 3) * ControlPoint1X +
                3 * Math.Pow(1 - t, 2) * t * ControlPoint3X +
                3 * (1 - t) * t * t * ControlPoint4X +
                t * t * t * ControlPoint2X;

            y = Math.Pow(1 - t, 3) * ControlPoint1Y +
                3 * Math.Pow(1 - t, 2) * t * ControlPoint3Y +
                3 * (1 - t) * t * t * ControlPoint4Y +
                t * t * t * ControlPoint2Y;
        }

        /// <summary>
        /// Gets interpolated Y values based on the assigned parameters and limits.
        /// </summary>
        public List<double>[] GenerateBezierCurve(int pointsCount, bool isRender)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            // Generate fine-grained samples to ensure better interpolation
            int fineSteps = (int)(pointsCount * CanvasSize);

            for (int i = 0; i <= fineSteps; i++)
            {
                double t = i / (double)fineSteps;

                GetValueAtT(t, out double x, out double y);

                valuesX.Add(x);
                valuesY.Add(y);
            }

            return [valuesX, valuesY];
        }

        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 1,2,3 };

            var values = GenerateBezierCurve(pointsCount, true)[0];

            return values;
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 1,2,3 };

            var values = GenerateBezierCurve(pointsCount, true)[1];

            return values;
        }
    }
}
