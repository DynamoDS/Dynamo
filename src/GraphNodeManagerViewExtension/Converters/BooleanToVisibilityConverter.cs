using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.UI;

namespace Dynamo.GraphNodeManager.Converters
{
    internal class BooleanToVisibilityConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class BooleanToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush;

            brush = (bool)value
                ? (SolidColorBrush)
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal100Brush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DefaultFontColorBrush"];

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class BooleanToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush;

            brush = (bool)value
                ? (SolidColorBrush)
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkGreyBrush"];

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
