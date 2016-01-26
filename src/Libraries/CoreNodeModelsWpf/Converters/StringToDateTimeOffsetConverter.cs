using System;
using System.Globalization;
using System.Windows.Data;
using Dynamo.Configuration;

namespace CoreNodeModelsWpf.Converters
{
    public class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt;
            return DateTime.TryParseExact(value.ToString(), PreferenceSettings.DefaultDateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ?
                dt : PreferenceSettings.DynamoDefaultTime; 
        }
    }
}
