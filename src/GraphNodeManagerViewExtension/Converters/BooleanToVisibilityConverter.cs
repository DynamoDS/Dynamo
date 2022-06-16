using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.GraphNodeManager.Properties;
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

    internal class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value != 0)
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

    internal class StateToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ElementState) value)
            {
                case ElementState.Info:
                    return SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue400Brush"];
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return SharedDictionaryManager.DynamoColorsAndBrushesDictionary["YellowOrange500Brush"];
                case ElementState.Error:
                    return SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Red500Brush"];
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class StateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ElementState)value)
            {
                case ElementState.Info:
                    return Resources.ToolTip_Information;
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return Resources.ToolTip_Warning;
                case ElementState.Error:
                    return Resources.ToolTip_Error;
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class StateToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ElementState)value)
            {
                case ElementState.Info:
                    return "/GraphNodeManagerViewExtension;component/Images/Info.png";
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return "/GraphNodeManagerViewExtension;component/Images/Alert.png";
                case ElementState.Error:
                    return "/GraphNodeManagerViewExtension;component/Images/Error.png";
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class StateToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var infoType = parameter as string;
            if (infoType == null) return Visibility.Collapsed;

            if ((ElementState)value == ElementState.Info && parameter.Equals("Info"))
            {
                return Visibility.Visible;
            }
            else if ((ElementState) value == ElementState.Warning ||
                     (ElementState) value == ElementState.PersistentWarning ||
                     (ElementState) value == ElementState.Error)
            {
                if (parameter.Equals("WarningOrError")) return Visibility.Visible;
            }

            return Visibility.Collapsed;
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
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue400Brush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkGreyBrush"];

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class BooleanToBackgroundHoverColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush;

            brush = (bool)value
                ? (SolidColorBrush)
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue300Brush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"];

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    internal class BooleanToBackgroundPressedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush;

            brush = (bool)value
                ? (SolidColorBrush)
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue300Brush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"];

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
            {
                return Visibility.Collapsed;    // or hidden?
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
