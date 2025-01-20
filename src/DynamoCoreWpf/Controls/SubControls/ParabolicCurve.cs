using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class ParabolicCurve : CurveBase
    {
        private CurveMapperControlPoint controlPoint1;
        private CurveMapperControlPoint controlPoint2;
        private PolyLineSegment polySegment;

        private double maxWidth;
        private double maxHeight;
        private const double parabolicIncrementX = 1.0;

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
        /// Initializes a parabolic curve with control points, dimensions, and visual properties.
        /// </summary>
        public ParabolicCurve(CurveMapperControlPoint controlPoint1, CurveMapperControlPoint controlPoint2, double maxWidth, double maxHeight)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;

            PathFigure = new PathFigure();

            GenerateParabola();

            PathFigure.Segments.Add(polySegment);

            PathGeometry = new PathGeometry()
            {
                Figures = { PathFigure }
            };

            PathCurve = new Path()
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Purple color
                StrokeThickness = 3
            };
        }

        private double SolveParabolaForX(double y, bool isNegative = false)
        {
            double a = Math.Pow(controlPoint2.Point.X - controlPoint1.Point.X, 2) / (4.0 * (controlPoint2.Point.Y - controlPoint1.Point.Y));
            double h = controlPoint1.Point.X;
            double k = controlPoint1.Point.Y;
            return ((isNegative) ? -1.0 : 1.0) * Math.Sqrt(4.0 * a * (y - k)) + h;
        }

        private double SolveParabolaForY(double x)
        {
            double a = Math.Pow(controlPoint2.Point.X - controlPoint1.Point.X, 2) / (4.0 * (controlPoint2.Point.Y - controlPoint1.Point.Y));
            double h = controlPoint1.Point.X;
            double k = controlPoint1.Point.Y;
            return (Math.Pow(x - h, 2) / (4 * a)) + k;
        }

        private void GenerateParabola()
        {
            if (polySegment == null)
            {
                polySegment = new PolyLineSegment();
                polySegment.IsStroked = true;
            }
            else
            {
                polySegment.Points.Clear();
            }

            double boundaryY = (controlPoint2.Point.Y > controlPoint1.Point.Y) ? maxHeight : 0.0;
            double startX = SolveParabolaForX(boundaryY, true);
            double endX = SolveParabolaForX(boundaryY);
            double minX = Math.Max(0, Math.Min(startX, endX));
            double maxX = Math.Min(maxWidth, Math.Max(startX, endX));

            if (PathFigure != null)
            {
                double vyy = SolveParabolaForY(minX);
                PathFigure.StartPoint = new Point(minX, vyy);
            }

            for (double d = minX; d < maxX; d += parabolicIncrementX)
            {
                double vy = SolveParabolaForY(d);
                if (vy >= 0 && vy < maxHeight)
                    polySegment.Points.Add(new Point(d, vy));
            }

            polySegment.Points.Add(new Point(maxX, SolveParabolaForY(maxX)));
        }

        /// <summary>
        /// Regenerates the curve when control points are updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (controlPoint1 == updatedControlPoint)
            {
                controlPoint1 = updatedControlPoint;
            }
            else if (controlPoint2 == updatedControlPoint)
            {
                controlPoint2 = updatedControlPoint;
            }

            GenerateParabola();
        }

        public override List<double> GetCurveYValues(double lowLimit, double highLimit, int count)
        {
            if (count < 1)
                return null;

            List<double> values = new List<double>();

            if (controlPoint1.Point.X == controlPoint2.Point.X)
                return null;

            int incount = count - 1;
            for (double d = 0.0; d < maxWidth; d += (maxWidth / incount))
            {
                double md = maxHeight - SolveParabolaForY(d);
                double rd = (highLimit - lowLimit) * md / maxHeight;
                rd += lowLimit;
                values.Add(rd);
            }

            if (values.Count < count)
            {
                double mdx = maxHeight - SolveParabolaForY(maxWidth);
                double rdx = (highLimit - lowLimit) * mdx / maxHeight;
                rdx += lowLimit;
                values.Add(rdx);
            }

            return values;
        }

        public override List<double> GetCurveXValues(double lowLimit, double highLimit, int count)
        {
            return null;
        }
    }
}
