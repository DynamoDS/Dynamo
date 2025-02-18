using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class ControlLine
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        public ControlLine(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }

        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            return new List<double> { ControlPoint1X, ControlPoint2X };
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            return new List<double> { ControlPoint1Y , ControlPoint2Y };
        }
    }
}
