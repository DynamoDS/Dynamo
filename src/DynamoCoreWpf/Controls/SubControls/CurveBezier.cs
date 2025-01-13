using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class CurveBezier : CurveBase
    {
        CurveMapperControlPoint tcp1 = null;
        CurveMapperControlPoint tcp2 = null;
        CurveMapperControlPointOrtho tco1 = null;
        CurveMapperControlPointOrtho tco2 = null;

        BezierSegment bezierSegment = null;

        List<LineSegment> lineSegments = new List<LineSegment>();
        Dictionary<double, double> dictXY = new Dictionary<double, double>();
        double tFactor = 0.01;

        double maxWidth = double.PositiveInfinity;
        double maxHeight = double.PositiveInfinity;

        public CurveBezier(CurveMapperControlPointOrtho o1, CurveMapperControlPointOrtho o2, CurveMapperControlPoint c1, CurveMapperControlPoint c2, double maxW, double maxH)
        {
            tco1 = o1;
            tco2 = o2;
            tcp1 = c1;
            tcp2 = c2;
            maxHeight = maxH;
            maxWidth = maxW;

            tFactor = 1.0 / maxWidth;

            bezierSegment = new BezierSegment(tcp1.Point, tcp2.Point, tco2.Point, true);

            PathFigure = new PathFigure()
            {
                StartPoint = tco1.Point
            };
            PathFigure.Segments.Add(bezierSegment);
            PathGeometry = new PathGeometry();
            PathGeometry.Figures.Add(PathFigure);

            PathCurve = new System.Windows.Shapes.Path()
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Centralize the color
                StrokeThickness = 3
            };
        }



        public void GetValueAtT(double t, out double x, out double y)
        {
            Point p1 = PathFigure.StartPoint;
            Point p2 = bezierSegment.Point1;
            Point p3 = bezierSegment.Point2;
            Point p4 = bezierSegment.Point3;

            x = Math.Pow(1 - t, 3) * p1.X +
                3 * Math.Pow(1 - t, 2) * t * p2.X +
                3 * (1 - t) * t * t * p3.X +
                t * t * t * p4.X;

            y = Math.Pow(1 - t, 3) * p1.Y +
                3 * Math.Pow(1 - t, 2) * t * p2.Y +
                3 * (1 - t) * t * t * p3.Y +
                t * t * t * p4.Y;
        }

        public void GetMaximumMinimumOrdinates(double maxValue, out double min, out double max)
        {
            min = double.PositiveInfinity;
            max = 0.0;

            double xx = 0.0;
            double yy = 0.0;

            double val = double.NaN;
            for (double i = 0; i < maxValue; i += 1)
            {
                GetValueAtT(i / maxValue, out xx, out yy);
                val = Math.Round(maxValue - yy, 2);
                if (max < val)
                {
                    max = val;
                }
                if (min > val)
                {
                    min = val;
                }
            }

            GetValueAtT(1.0, out xx, out yy);
            val = Math.Round(maxValue - yy, 2);
            if (max < val)
            {
                max = val;
            }
            if (min > val)
            {
                min = val;
            }
        }

        private void GenerateXYPairs()
        {
            if (dictXY == null)
            {
                dictXY = new Dictionary<double, double>();
            }
            else
            {
                dictXY.Clear();
            }

            double xx = 0.0;
            double yy = 0.0;
            double tacqfac = maxWidth * 5.0;
            for (double t = 0.0; t <= 1.0; t += 1.0 / tacqfac)
            {
                GetValueAtT(t, out xx, out yy);
                xx = Math.Round(xx, 0);
                if (!dictXY.ContainsKey(xx))
                {
                    dictXY.Add(xx, yy);
                }
            }

            GetValueAtT(1.0, out xx, out yy);
            if (dictXY.ContainsKey(xx))
            {
                dictXY.Remove(xx);
            }
            dictXY.Add(xx, yy);
        }

        public override List<double> GetValuesFromAssignedParameters(double lowLimit, double highLimit, int count)
        {
            if (count < 1)
                return null;

            GenerateXYPairs();

            List<double> livalues = new List<double>();

            for (int i = 0; i < count; i++)
            {
                double xx = 0.0;
                double yy = 0.0;
                xx = (int)((maxWidth / (count - 1.0)) * i);
                yy = dictXY[xx];

                double md = maxHeight - yy;
                double rd = (highLimit - lowLimit) * md / maxHeight;
                rd += lowLimit;
                livalues.Add(rd);
            }

            return livalues;
        }

        public void Regenerate(CurveMapperControlPointOrtho tco)
        {
            if (tco1 == tco)
            {
                tco1 = tco;
                PathFigure.StartPoint = tco.Point;
            }
            else if (tco2 == tco)
            {
                tco2 = tco;
                bezierSegment.Point3 = tco.Point;
            }
        }

        public void Regenerate(CurveMapperControlPoint tcp)
        {
            if (tcp1 == tcp)
            {
                tcp1 = tcp;
                bezierSegment.Point1 = tcp.Point;
            }
            else if (tcp2 == tcp)
            {
                tcp2 = tcp;
                bezierSegment.Point2 = tcp.Point;
            }
        }

        //public List<Point> GetPointsFromParameters(double maxValue, List<double> ts)
        //{
        //    List<Point> retvals = new List<Point>();

        //    foreach (double o in ts)
        //    {
        //        double x = 0.0;
        //        double y = 0.0;
        //        GetValueAtT(o, out x, out y);

        //        Point p = new Point(x, y);
        //        retvals.Add(p);
        //    }

        //    return retvals;
        //}

    }
}
