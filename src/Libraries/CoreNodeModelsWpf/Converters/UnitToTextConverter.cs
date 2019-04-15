using System;
using System.Globalization;
using System.Windows.Data;
using Dynamo.Wpf.Properties;
using DynamoConversions;

namespace Dynamo.Controls
{
    public class UnitToTextConverter : IValueConverter
    {
        // parameter is the data context
        // value is the selection
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stUnit = string.Empty;

            if (value is ConversionUnit)
            {
                stUnit = ((ConversionUnit)value).ToString();
            }
            else if (value is ConversionMetricUnit)
            {
                stUnit = ((ConversionMetricUnit)value).ToString();
            }
            string localizedEnum = Resources.ResourceManager.GetString("Unit" + stUnit, CultureInfo.CurrentUICulture);
            return localizedEnum ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}