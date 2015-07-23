using System;
using System.Globalization;
using System.Windows.Data;
using DynamoConversions;
using Dynamo.Wpf.Properties;

namespace Dynamo.Controls
{
    public class UnitToTextConverter : IValueConverter
    {
        // parameter is the data context
        // value is the selection
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {         
            if (value is ConversionUnit)
            {
                ConversionUnit st = (ConversionUnit) value;
                string localizedEnum = Resources.ResourceManager.GetString("Unit" + st.ToString(), CultureInfo.CurrentUICulture);
                return localizedEnum ?? string.Empty;
            }
            else if (value is ConversionMetricUnit)
            {
                ConversionMetricUnit st = (ConversionMetricUnit)value;
                string localizedEnum = Resources.ResourceManager.GetString("Unit" + st.ToString(), CultureInfo.CurrentUICulture);
                return localizedEnum ?? string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}