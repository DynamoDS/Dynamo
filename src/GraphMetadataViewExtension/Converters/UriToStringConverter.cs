using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dynamo.GraphMetadata.Converters
{
    [ValueConversion(typeof(Uri), typeof(string))]
    class UriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Uri uri))
                return null;

            return uri.AbsoluteUri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string str))
                return null;

            var uri = new Uri(str);
            return uri;
        }
    }
}
