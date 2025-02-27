using Dynamo.PackageManager;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class SineCurve : CurveBase
    {
        private PolyLineSegment polySegment;

        // Coefficients
        private double coefA;   // Amplitude
        private double coefB;   // 2*PI/period
        private double coefC;   // Phase shift
        private double coefD;   // Vertical shift

        /// <summary>
        /// Initializes a sine curve with control points, dimensions, and visual properties.
        /// </summary>
        public SineCurve(CurveMapperControlPoint controlPoint1, CurveMapperControlPoint controlPoint2, double maxWidth, double maxHeight)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

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
                Stroke = CurveColor,
                StrokeThickness = CurveThickness
            };
        }

        private double ConvertXToTrigo(double x) => Math.PI * x / MaxWidth;
        private double ConvertYToTrigo(double y) => 2.0 * y / MaxHeight - 1.0;
        private double ConvertTrigoToX(double unitX) => unitX * MaxWidth / Math.PI;
        private double ConvertTrigoToY(double unitY) => (unitY + 1.0) * MaxHeight / (2.0);
        private double CosineEquation(double x) => -(coefA * Math.Cos(coefB * x - coefC)) + coefD;

        private void GetEquationCoefficients()
        {
            coefA = (ConvertYToTrigo(controlPoint2.Point.Y) - ConvertYToTrigo(controlPoint1.Point.Y)) / 2.0;
            coefB = Math.PI / (ConvertXToTrigo(controlPoint2.Point.X) - ConvertXToTrigo(controlPoint1.Point.X));
            coefC = coefB * ConvertXToTrigo(controlPoint1.Point.X);
            coefD = (ConvertYToTrigo(controlPoint1.Point.Y) + ConvertYToTrigo(controlPoint2.Point.Y)) / 2.0;
        }

        private void GenerateSineWave()
        {
            polySegment ??= new PolyLineSegment { IsStroked = true, IsSmoothJoin = true };
            polySegment.Points.Clear();

            double ud = ConvertXToTrigo(0.0);
            double udd = CosineEquation(ud);
            PathFigure.StartPoint = new Point(ConvertTrigoToX(ud), ConvertTrigoToY(udd));

            for (double d = 1.0; d < MaxWidth; d += 2.0)
            {
                double vd = ConvertXToTrigo(d);
                double dd = CosineEquation(vd);
                polySegment.Points.Add(new Point(ConvertTrigoToX(vd), ConvertTrigoToY(dd)));
            }
            double vx = ConvertXToTrigo(MaxWidth);
            double dy = CosineEquation(vx);
            polySegment.Points.Add(new Point(ConvertTrigoToX(vx), ConvertTrigoToY(dy)));
        }

        /// <summary>
        /// Regenerates the sine wave when a control point is updated.
        /// </summary>
        public override void Regenerate()
        {
            GetEquationCoefficients();
            GenerateSineWave();
        }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public override List<double> GetCurveYValues(double minY, double maxY, int pointCount)
        {
            if (pointCount < 1) return null;

            List<double> yValues = new List<double>();

            if (controlPoint1.Point.X == controlPoint2.Point.X) return null;

            int step = pointCount - 1;
            for (double xPos = 0.0; xPos < MaxWidth; xPos += (MaxWidth / step))
            {
                double normalizedY = MaxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(xPos)));
                double scaledY = (maxY - minY) * normalizedY / MaxHeight;
                scaledY += minY;
                scaledY = Math.Round(scaledY, rounding);
                yValues.Add(scaledY);
            }

            if (yValues.Count < pointCount)
            {
                double normalizedY = MaxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(MaxWidth)));
                double scaledY = (maxY - minY) * normalizedY / MaxHeight;
                scaledY += minY;
                scaledY = Math.Round(scaledY, rounding);
                yValues.Add(scaledY);
            }

            return yValues;
        }
    }
}
