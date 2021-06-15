using System;
using System.Globalization;
using System.Windows.Data;

namespace Dynamo.GraphMetadata.Converters
{
    [ValueConversion(typeof(Uri), typeof(string))]
    class UriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Uri uri)) return null;

            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string str)) return null;

            Uri uri = null;
            if (!string.IsNullOrEmpty(str))
            {
                uri = new Uri(str, UriKind.RelativeOrAbsolute);
            }

            return uri;
        }
    }
}
