using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public abstract class CurveBase
    {
        protected const int rounding = 10;

        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public int PointsCount { get; set; }

        protected CurveBase(double minX, double maxX, double minY, double maxY, int pointsCount)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            PointsCount = pointsCount;
        }

        public abstract List<double> GetCurveYValues();
        public List<double> GetCurveXValues()
        {
            var values = new List<double>();
            double step = (MaxX - MinX) / (PointsCount - 1);

            for (int i = 0; i < PointsCount; i++)
            {
                values.Add(Math.Round(MinX + i * step, rounding));
            }
            return values;
        }
    }
}
