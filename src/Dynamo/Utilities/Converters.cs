using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;

namespace Dynamo.Controls
{

    public class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NodeTemplate { get; set; }
        public DataTemplate BrowserCategoryTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is BrowserInternalElement)
            {
                return BrowserCategoryTemplate;
            } else
            {
                return NodeTemplate;
            }
        }
    }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = (double)value;
            return new Thickness(0, -1 * height - 3, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class PathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                // convert to path, get file name
                return Path.GetFileName((string) value);
            } 

            return "Unsaved";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class WorkspaceTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is dynWorkspaceViewModel)
            {
                var val = (value as dynWorkspaceViewModel).Model.GetType();
                return val;
            }

            if (value is dynWorkspaceModel)
            {
                return value.GetType();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class BooleanToBrushConverter : IValueConverter
    {
        public SolidColorBrush TrueBrush{get;set;}
        public SolidColorBrush FalseBrush{get;set;}

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool condition = (bool)value;
            if (condition)
            {
                //return new SolidColorBrush(Colors.Cyan);
                return TrueBrush;
            }
            else
            {
                //return new SolidColorBrush(Colors.Black);
                return FalseBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BooleanToSelectionColorConverter : IValueConverter
    {
        public Color True { get; set; }
        public Color False { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool condition = (bool)value;
            if (condition)
            {
                //return new SolidColorBrush(Colors.Cyan);
                return True;
            }
            else
            {
                //return new SolidColorBrush(Colors.Black);
                return False;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        //http://stackoverflow.com/questions/3238590/accessing-colors-in-a-resource-dictionary-from-a-value-converter

        public LinearGradientBrush DeadBrush { get; set; }
        public LinearGradientBrush ActiveBrush { get; set; }
        public LinearGradientBrush ErrorBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ElementState state = (ElementState)value;
            switch (state)
            {
                case ElementState.ACTIVE:
                    return ActiveBrush;
                case ElementState.DEAD:
                    return DeadBrush;
                case ElementState.ERROR:
                    return ErrorBrush;
            }

            return DeadBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class PortCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<dynPortViewModel> ports = (ObservableCollection<dynPortViewModel>)value;
            return Math.Max(30, ports.Count * 20 + 10); //spacing for inputs + title space + bottom space
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ListHasItemsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<object> list = (List<object>)value;
            return list.Count > 0; //spacing for inputs + title space + bottom space
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    //[ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ShowHideConsoleMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Hide Console";
            }
            else
            {
                return "Show Console";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ShowHideFullscreenWatchMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Disable preview geometry";
            }
            else
            {
                return "Enable preview geometry";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ShowHideClassicNavigatorMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Hide Classic Node Library";
            }
            else
            {
                return "Show Classic Node Library";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class ZoomStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return string.Format("Zoom : {0}", value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class TransformOriginStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Point p = (Point)value;
            return string.Format("Transform origin X: {0}, Y: {1}", p.X, p.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class CurrentOffsetStatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Point p = (Point)value;
            return string.Format("Current offset X: {0}, Y: {1}", p.X, p.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // You could also directly pass an enum value using {x:Static},
            // then there is no need to parse
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }

    public class PortTypeToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //PortType p = (PortType)value;
            //if (p == PortType.INPUT)
            //{
            //    return new Thickness(20, 0, 0, 0);
            //}
            //else
            //{
            //    return new Thickness(-20, 0, 0, 0);
            //}

            return new Thickness(0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PortTypeToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PortType p = (PortType)value;
            if (p == PortType.INPUT)
            {
                return HorizontalAlignment.Left;
            }
            else
            {
                return HorizontalAlignment.Right;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PortTypeToGridColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PortType p = (PortType)value;
            if (p == PortType.INPUT)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PortTypeToClipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PortType p = (PortType)value;
            if (p == PortType.INPUT)
            {
                return new Rect(0,0,10,20);
            }
            else
            {
                return new Rect(10, 0, 10, 20);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToConsoleHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool consoleShowing = (bool) value;
            if (consoleShowing)
                return 100.0;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToFullscreenWatchVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool fullscreenWatchShowing = (bool)value;
            if (fullscreenWatchShowing)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
