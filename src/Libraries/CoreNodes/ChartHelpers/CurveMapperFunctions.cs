using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodes.ChartHelpers
{
    public class CurveMapperFunctions
    {
        //public static List<double> GenerateCurve(
        //    double limitMinX,
        //    double limitMaxX,
        //    double limitMinY,
        //    double limitMaxY,
        //    double count)
        //{
        //    return new List<double> { 1,2,3};
        //}


        //public static List<double> GenerateValues(
        //    int graphType,
        //    int count,
        //    double limitMin,
        //    double limitMax,
        //    double maxX,
        //    double maxy,
        //    List<double> fixed1,
        //    List<double> fixed2,
        //    List<double> free1,
        //    List<double> free2)
        //{
        //    List<double> retval = new List<double>();

        //    double diffx = limitMax - limitMin;
        //    double diffy = maxy;
        //    double quotx = diffx / (count - 1);
        //    double quoty = diffy / (count - 1);

        //    double fix1x = fixed1[0];
        //    double fix1y = fixed1[1];
        //    double fix2x = fixed2[0];
        //    double fix2y = fixed2[1];
        //    double free1x = free1[0];
        //    double free1y = free1[1];
        //    double free2x = free2[0];
        //    double free2y = free2[1];

        //    //  temporary
        //    switch (graphType)
        //    {
        //        case 0: //  LINEAR
        //            for (double d = limitMin; d < limitMax; d += quotx)
        //            {
        //                retval.Add(SolveLinear(d, free1[0], free1[1], free2[0], free2[1]));
        //            }
        //            retval.Add(SolveLinear(limitMax, free1[0], free1[1], free2[0], free2[1]));
        //            break;
        //        default:
        //            break;
        //    }

        //    return retval;
        //}



        public static List<double> GenerateXValues(double limitMinX, double limitMaxX, int count)
        {
            List<double> xValues = new List<double>();
            double step = (limitMaxX - limitMinX) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                xValues.Add(limitMinX + i * step);
            }

            return xValues;
        }

        public static List<double> GenerateYValues(double limitMinX, double limitMaxX, double limitMinY, double limitMaxY, int count)
        {
            List<double> yValues = new List<double>();
            double step = (limitMaxX - limitMinX) / (count - 1);
            double rangeY = limitMaxY - limitMinY;

            for (int i = 0; i < count; i++)
            {
                double x = limitMinX + i * step;
                yValues.Add(limitMinY + (rangeY * (x - limitMinX) / (limitMaxX - limitMinX)));
            }

            return yValues;
        }





        private static double SolveLinear(double xVal, double pt1X, double pt1Y, double pt2X, double pt2Y)
        {
            return xVal * (pt2Y - pt1Y) / (pt2X - pt1X);
        }

        private static double SolveParabolic(double xVal, double pt1X, double pt1Y, double pt2X, double pt2Y)
        {
            return 0;
        }
    }
}
