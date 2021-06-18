using System;
using System.Globalization;
using System.Windows.Data;

namespace Dynamo.GraphMetadata.Converters
{
    [ValueConversion(typeof(Boolean), typeof(bool))]
    public class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Boolean boolean))
            {
                return false;
            }

            return !boolean;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Boolean boolean))
            {
                return false;
            }

            return !boolean;
        }
    }
}