using DynamoConversionsUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dynamo.Wpf.Converters
{
    public class ConversionDirectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var conversionDirection = (ConversionDirection) value;

            switch (conversionDirection)
            {
                case ConversionDirection.From:
                    return parameter.ToString() == "To" ? Visibility.Collapsed : Visibility.Visible;
                case ConversionDirection.To:
                    return parameter.ToString() == "From" ? Visibility.Collapsed : Visibility.Visible;
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConversionDirectionToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var conversionDirection = (ConversionDirection) value;
            var param = parameter.ToString();

            switch (conversionDirection)
            {
                case ConversionDirection.From:
                    if (param == "ConversionName")
                    {
                        return 2;
                    }
                    if (param == "ConversionType")
                    {
                        return 0;
                    }
                    break;
                case ConversionDirection.To:
                    if (param == "ConversionName")
                    {
                        return 0;
                    }

                    if (param == "ConversionType")
                    {
                        return 2;
                    }
                    break;
                default:
                    return 0;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConversionTypeToBaseUnitNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var conversionType = (ConversionUnit)value;

            switch (conversionType)
            {
                case ConversionUnit.Feet:
                case ConversionUnit.Inches:
                case ConversionUnit.Meters:
                case ConversionUnit.Millimeters:
                case ConversionUnit.Centimeters:
                    return "Meters";
                case ConversionUnit.Degrees:
                case ConversionUnit.Radians:
                    return "Radians";
                case ConversionUnit.Kilograms:
                case ConversionUnit.Pounds:
                    return "Kilogram";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}