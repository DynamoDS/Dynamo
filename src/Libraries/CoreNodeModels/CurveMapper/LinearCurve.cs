using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class LinearCurve : CurveBase
    {
        public LinearCurve(double minX, double maxX, double minY, double maxY, int pointsCount)
            : base(minX, maxX, minY, maxY, pointsCount) { }

        public override List<double> GetCurveYValues()
        {
            var values = new List<double>();
            for (int i = 0; i < PointsCount; i++)
            {
                double t = i / (double)(PointsCount - 1);
                double y = (1 - t) * MinY + t * MaxY;
                values.Add(Math.Round(y, rounding));
            }
            return values;
        }
    }
}
