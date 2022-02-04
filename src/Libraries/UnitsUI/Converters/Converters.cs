using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DynamoUnits;

namespace UnitsUI.Converters
{
    public class ForgeUnitToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stUnit = string.Empty;

            if (value is Unit unit)
            {
                stUnit = unit.Name;
            }
            return stUnit;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ForgeQuantityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stQauntity = string.Empty;

            if (value is Quantity quantity)
            {
                stQauntity = quantity.Name;
            }
            return stQauntity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
