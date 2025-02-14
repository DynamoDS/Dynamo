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
        private const double renderIncrementX = 1.0; // ADD THIS BASE CLASS ?

        public BezierCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }
        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            return new List<double> { 1,2,3 };
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            return new List<double> { 1, 2, 3 };
        }
    }
}
