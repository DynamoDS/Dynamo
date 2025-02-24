using System.Collections.Generic;

namespace CoreNodeModels.CurveMapper
{
    public class ControlLine : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;

        public ControlLine(double cp1X, double cp1Y, double cp2X, double cp2Y, double canvasSize)
            : base(canvasSize)
        {
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
        }
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(int pointsCount, bool isRender = false)
        {
            return (new List<double> { ControlPoint1X, ControlPoint2X }, new List<double> { ControlPoint1Y, ControlPoint2Y });
        }
    }
}
