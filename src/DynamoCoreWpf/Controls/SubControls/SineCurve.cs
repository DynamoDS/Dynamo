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
        private double ConvertTrigoToX(double unitX) => unitX * maxWidth / Math.PI;
        private double ConvertTrigoToY(double unitY) => (unitY + 1.0) * maxHeight / (2.0);
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
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public override List<double> GetCurveYValues(double minY, double maxY, int pointCount)
        {
            if (pointCount < 1) return null;

            List<double> yValues = new List<double>();

            if (controlPoint1.Point.X == controlPoint2.Point.X) return null;

            int step = pointCount - 1;
            for (double xPos = 0.0; xPos < maxWidth; xPos += (maxWidth / step))
            {
                double normalizedY = maxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(xPos)));
                double scaledY = (maxY - minY) * normalizedY / maxHeight;
                scaledY += minY;
                yValues.Add(scaledY);
            }

            if (yValues.Count < pointCount)
            {
                double normalizedY = maxHeight - ConvertTrigoToY(CosineEquation(ConvertXToTrigo(maxWidth)));
                double scaledY = (maxY - minY) * normalizedY / maxHeight;
                scaledY += minY;
                yValues.Add(scaledY);
            }

            return yValues;
        }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public List<double> GetSineWaveXValues(double minX, double maxX, int pointCount)
        {
            if (pointCount < 1) return null;

            List<double> xValues = new List<double>();
            double step = (maxX - minX) / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                xValues.Add(minX + i * step);
            }

            return xValues;
        }
    }
}
