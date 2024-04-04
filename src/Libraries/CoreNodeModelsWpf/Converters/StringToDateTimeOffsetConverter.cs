using System;
using System.Globalization;
using System.Windows.Data;
using Dynamo.Configuration;

namespace CoreNodeModelsWpf.Converters
{
    /// <summary>
    /// DateTime converter used for Date Time node value conversion
    /// </summary>
    public class StringToDateTimeConverter : IValueConverter
    {
        /// <summary>
        /// Convert Date Time object to string using DefaultDateFormat
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert input string to Date Time value, try to apply DefaultDateFormat.
        /// If conversion fails, use default time.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.TryParseExact(value.ToString(), PreferenceSettings.DefaultDateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ?
                dt : PreferenceSettings.DynamoDefaultTime;
        }
    }
}
