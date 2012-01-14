using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Dynamo.Elements
{
    public class Shrinker : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            //convert the value to a double
            double d = Double.Parse(value.ToString());
            return d - 4;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class Halfer : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            //convert the value to a double
            double d = Double.Parse(value.ToString());
            return d / 2;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class Quarter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            //convert the value to a double
            double d = Double.Parse(value.ToString());
            return d / 4;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
