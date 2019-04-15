using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DSColor = DSCore.Color;

namespace CoreNodeModelsWpf.Converters
{
    public class MediatoDSColorConverter : IValueConverter
    {
        private DSColor dscolor;
        private Color color;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            dscolor = (DSColor)value;

            if (!dscolor.Equals(null))
            {
                color = Color.FromArgb(dscolor.Alpha, dscolor.Red, dscolor.Green, dscolor.Blue);
            }
            else
            {
                color = Color.FromArgb(0, 0, 0, 0);
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            color = (Color)value;
            if (!color.Equals(null))
            {
                dscolor = DSColor.ByARGB(color.A, color.R, color.G, color.B);
            }
            else
            {
                dscolor = DSColor.ByARGB(0, 0, 0, 0);
            }

            return dscolor;
        }
    }
}