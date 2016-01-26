using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CoreNodeModelsWpf.Converters
{
    public class SelectionButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object>)
            {
                if (!(value as IEnumerable<object>).Any())
                    return Dynamo.Wpf.Properties.Resources.SelectNodeButtonSelect;
            }

            return Dynamo.Wpf.Properties.Resources.SelectNodeButtonChange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}