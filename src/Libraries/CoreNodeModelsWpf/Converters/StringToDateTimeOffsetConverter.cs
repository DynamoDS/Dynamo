using System;
using System.Globalization;
using System.Windows.Data;

namespace Dynamo.Wpf.Converters
{
    public class StringToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dtOffset;
            return DateTime.TryParseExact(value.ToString(), PreferenceSettings.DefaultDateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dtOffset) ?
                dtOffset : new DateTime();
        }
    }
}
