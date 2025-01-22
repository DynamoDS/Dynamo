using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class CurveBase : UIElement
    {
        // private backing fields
        // TODO: declare in API
        private Path pathCurve;
        private PathGeometry pathGeometry;
        private PathFigure pathFigure;
        private double maxWidth;
        private double maxHeight;
        protected CurveMapperControlPoint controlPoint1;
        protected CurveMapperControlPoint controlPoint2;
        protected const int rounding = 10;

        public Path PathCurve
        {
            get => pathCurve;
            set => pathCurve = value;
        }

        public PathGeometry PathGeometry
        {
            get => pathGeometry;
            set => pathGeometry = value;
        }

        public PathFigure PathFigure
        {
            get => pathFigure;
            set => pathFigure = value;
        }

        public CurveBase()
        {
            pathGeometry = new PathGeometry();
            pathFigure = new PathFigure();
        }

        public double MaxWidth
        {
            get => maxWidth;
            set
            {
                if (maxWidth != value)
                {
                    maxWidth = value;
                    Regenerate();
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
                    Regenerate();
                }
            }
        }
        protected SolidColorBrush CurveColor { get; set; } = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2));
        protected double CurveThickness { get; set; } = 3.0;

        /// <summary>
        /// Regenerates the curve based on the updated positions of control points.
        /// </summary>
        public virtual void Regenerate() { }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public virtual List<double> GetCurveYValues(double minLimitY, double maxLimitY, int count)
        {
            return new List<double>();
        }

        /// <summary>
        /// Calculates the X-axis values for the curve based on input limits and count.
        /// </summary>
        public virtual List<double> GetCurveXValues(double minLimitX, double maxLimitX, int count)
        {
            if (count < 1)
                return null;

            var values = new List<double>();
            double step = (maxLimitX - minLimitX) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                double xMapped = Math.Round(minLimitX + i * step, rounding);
                values.Add(xMapped);
            }

            return values;
        }

        /// <summary>
        /// Maps a canvas coordinate to a specified range based on the canvas height.
        /// </summary>
        protected double MapCanvasToRange(double canvasValue, double min, double max)
        {
            double normalizedValue = MaxHeight - canvasValue;
            return min + (normalizedValue / MaxHeight) * (max - min);
        }
    }
}
