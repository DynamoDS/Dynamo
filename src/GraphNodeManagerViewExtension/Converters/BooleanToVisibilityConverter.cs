using System;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.GraphNodeManager.Properties;
using Dynamo.UI;
using FontStyle = System.Drawing.FontStyle;

namespace Dynamo.GraphNodeManager.Converters
{
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
                    return Resources.Title_Information;
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return Resources.Title_Warning;
                case ElementState.Error:
                    return Resources.Title_Error;
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
    
    internal class BooleanToToolTipTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Resources.ToolTip_ExportToExcelFiltered;    
            }

            return Resources.ToolTip_ExportToExcel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    internal class BooleanToFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return System.Windows.FontStyles.Italic;
            }

            return System.Windows.FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    internal class BooleanToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return new FontFamily("Open Sans");
            }

            return SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
