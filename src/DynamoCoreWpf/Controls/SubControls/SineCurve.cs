using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class SineCurve : CurveBase
    {
        private CurveMapperControlPoint controlPoint1;
        private CurveMapperControlPoint controlPoint2;
        private PolyLineSegment polySegment;

        private double maxWidth;
        private double maxHeight;
        public bool isCosine = false;

        // Coefficients
        private double coefA;   // Amplitude
        private double coefB;   // 2*PI/period
        private double coefC;   // Phase shift
        private double coefD;   // Vertical shift

        public double MaxWidth
        {
            get => maxWidth;
            set
            {
                if (maxWidth != value)
                {
                    maxWidth = value;
                    Regenerate(controlPoint1); // Ensure the curve regenerates if needed
                }
            }
        }
        public double MaxHeight
        {
            get => maxHeight;
            set
            {
                if (maxHeight != value)
                {
                    maxHeight = value;
                    Regenerate(controlPoint1); // Ensure the curve regenerates if needed
                }
            }
        }

        /// <summary>
        /// Initializes a sine curve with control points, dimensions, and visual properties.
        /// </summary>
        public SineCurve(CurveMapperControlPoint controlPoint1, CurveMapperControlPoint controlPoint2, double maxWidth, double maxHeight)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;

            PathFigure = new PathFigure();

            // Initialize coefficients and generate sine wave
            GetEquationCoefficients();
            GenerateSineWave();

            PathFigure.Segments.Add(polySegment);

            PathGeometry = new PathGeometry
            {
                Figures = { PathFigure }
            };

            PathCurve = new Path
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Purple color
                StrokeThickness = 3,
            };
        }

        private double ConvertXToTrigo(double x) => Math.PI * x / maxWidth;
        private double ConvertYToTrigo(double y) => 2.0 * y / maxHeight - 1.0;
        private double ConvertTrigoToX(double trix) => trix * maxWidth / Math.PI;
        private double ConvertTrigoToY(double triy) => (triy + 1.0) * maxHeight / (2.0);
        private double CosineEquation(double x) => -(coefA * Math.Cos(coefB * x - coefC)) + coefD;

        private void GetEquationCoefficients()
        {
            Point p1 = controlPoint1.Point;
            Point p2 = controlPoint2.Point;
            double p1x = ConvertXToTrigo(p1.X);
            double p2x = ConvertXToTrigo(p2.X);
            double p1y = ConvertYToTrigo(p1.Y);
            double p2y = ConvertYToTrigo(p2.Y);

            coefA = (p2y - p1y) / 2.0;
            coefB = Math.PI / (p2x - p1x);
            coefC = coefB * p1x;
            coefD = (p1y + p2y) / 2.0;
        }

        private void GenerateSineWave()
        {
            polySegment ??= new PolyLineSegment { IsStroked = true, IsSmoothJoin = true };
            polySegment.Points.Clear();

            double ud = ConvertXToTrigo(0.0);
            double udd = CosineEquation(ud);
            PathFigure.StartPoint = new Point(ConvertTrigoToX(ud), ConvertTrigoToY(udd));

            for (double d = 1.0; d < maxWidth; d += 2.0)
            {
                double vd = ConvertXToTrigo(d);
                double dd = CosineEquation(vd);
                polySegment.Points.Add(new Point(ConvertTrigoToX(vd), ConvertTrigoToY(dd)));
            }
            double vx = ConvertXToTrigo(maxWidth);
            double dy = CosineEquation(vx);
            polySegment.Points.Add(new Point(ConvertTrigoToX(vx), ConvertTrigoToY(dy)));
        }

        /// <summary>
        /// Regenerates the sine wave when a control point is updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (controlPoint1 == updatedControlPoint || controlPoint2 == updatedControlPoint)
            {
                GetEquationCoefficients();
                GenerateSineWave();
            }
        }

        /// <summary>
        /// Generates a list of values mapped to a sine curve within the specified range and count.
        /// </summary>
        public override List<double> GetValuesFromAssignedParameters(double lowLimit, double highLimit, int count)
        {
            if (count < 1)
                return null;

            List<double> livalues = new List<double>();

            if (controlPoint1.Point.X == controlPoint2.Point.X)
            {
                return null;
            }

            int incount = count - 1;
            for (double d = 0.0; d < maxWidth; d += (maxWidth / incount))
            {
                double md = maxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(d)));
                double rd = (highLimit - lowLimit) * md / maxHeight;
                rd += lowLimit;
                livalues.Add(rd);
            }

            if (livalues.Count < count)
            {
                double mdx = maxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(maxWidth)));
                double rdx = (highLimit - lowLimit) * mdx / maxHeight;
                rdx += lowLimit;
                livalues.Add(rdx);
            }

            return livalues;
        }
    }
}
