using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class UVCoordText : TextBase, IDisposable
    {
        //Point uvpoint;
        //double maxwidth = 0.0;
        //double maxheight = 0.0;
        //double u = -1.0;
        //double v = -1.0;
        //public double OverU
        //{
        //    get; set;
        //}
        //public double OverV
        //{
        //    get; set;
        //}
        private readonly double maxWidth;
        private readonly double maxHeight;
        private double u = -1.0;
        private double v = -1.0;

        public double OverU { get; set; } = -1.0;
        public double OverV { get; set; } = -1.0;

        private const int decimalPlaces = 3;
        private const double gap = 4.0;

        public UVCoordText(Point p, double maxWidth, double maxHeight)
        {
            maxWidth = maxWidth;
            maxHeight = maxHeight;

            OverU = -1.0;
            OverV = -1.0;

            Regenerate(p);
        }

        public override void Regenerate(Point position)
        {
            ComputeUV(position);

            this.Text = string.Format("({0:0.000},{1:0.000})", (OverU < 0) ? u : OverU, (OverV < 0) ? v : OverV);
            Size textSize = MeasureString(this.Text);

            double posX = position.X + gap;
            double posY = position.Y - textSize.Height - gap;

            if ((textSize.Width + gap) > (maxWidth - position.X))
            {
                posX = posX - textSize.Width - gap * 2;
            }

            if ((textSize.Height + gap) > position.Y)
            {
                posY = position.Y + gap;
            }

            Canvas.SetLeft(this, posX);
            Canvas.SetTop(this, posY);
            Canvas.SetZIndex(this, 80);
        }

        private void ComputeUV(Point position)
        {
            u = Math.Round(position.X / maxWidth, decimalPlaces);
            v = Math.Round(1.0 - (position.Y / maxHeight), decimalPlaces);
        }

        protected Size MeasureString(string text)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                Brushes.Black,
                1.25);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public void Dispose()
        {
        }
    }
}
