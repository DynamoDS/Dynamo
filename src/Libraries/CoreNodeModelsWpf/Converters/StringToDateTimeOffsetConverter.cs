using System;
using System.Globalization;
using System.Windows.Data;

namespace Dynamo.Wpf.Converters
{
    public class StringToDateTimeOffsetConverter : IValueConverter
    {
        private const string format = "dd MMMM yyyy h:mm tt zzz";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTimeOffset)value).ToString(format, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimeOffset dtOffset;
            return DateTimeOffset.TryParseExact(value.ToString(), format,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dtOffset) ?
                dtOffset : new DateTimeOffset();
        }
    }
}
