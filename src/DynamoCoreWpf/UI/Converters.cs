using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using DynamoUnits;
using RestSharp.Contrib;

namespace Dynamo.Controls
{
    public class TooltipLengthTruncater : IValueConverter
    {
        private const int MaxChars = 100;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tooltip = value as string;
            if (tooltip != null && tooltip.Length > MaxChars)
            {
                var trimIndex = tooltip.LastIndexOf(' ', MaxChars - 5);
                return tooltip.Remove(trimIndex > 0 ? trimIndex : MaxChars - 5) + " ...";
            } 
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PrettyDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateString = value as string;
            if (dateString != null) return PrettyDate(dateString);

            return "Unknown date format";
        }

        private string PrettyDate(string json_string)
        {
            var d = DateTime.Parse(json_string);

            return d.ToString("d MMM yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PackageSearchStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is PackageManagerSearchViewModel.PackageSearchState)
            {
                var st = (PackageManagerSearchViewModel.PackageSearchState) value;

                if (st == PackageManagerSearchViewModel.PackageSearchState.NORESULTS)
                {
                    return "Search returned no results!";
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.RESULTS)
                {
                    return "";
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.SEARCHING)
                {
                    return "Searching...";
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.SYNCING)
                {
                    return "Syncing with server...";
                }
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PackageUploadStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is PackageUploadHandle.State)
            {
                var st = (PackageUploadHandle.State)value;

                if (st == PackageUploadHandle.State.Compressing)
                {
                    return "Compressing...";
                }
                else if (st == PackageUploadHandle.State.Copying)
                {
                    return "Copying...";
                }
                else if (st == PackageUploadHandle.State.Error)
                {
                    return "Error!";
                }
                else if (st == PackageUploadHandle.State.Ready)
                {
                    return "Ready";
                }
                else if (st == PackageUploadHandle.State.Uploaded)
                {
                    return "Uploaded";
                }
                else if (st == PackageUploadHandle.State.Uploading)
                {
                    return "Uploading...";
                }

            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class PackageDownloadStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is PackageDownloadHandle.State )
            {
                var st = ( PackageDownloadHandle.State ) value;

                if (st == PackageDownloadHandle.State.Downloaded)
                {
                    return "Downloaded";
                }
                else if (st == PackageDownloadHandle.State.Downloading)
                {
                    return "Downloading";
                }
                else if (st == PackageDownloadHandle.State.Error)
                {
                    return "Error";
                }
                else if (st == PackageDownloadHandle.State.Installed)
                {
                    return "Installed";
                }
                else if (st == PackageDownloadHandle.State.Installing)
                {
                    return "Installing";
                }
                else  if (st == PackageDownloadHandle.State.Uninitialized)
                {
                    return "Starting";
                }

            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class NonEmptyStringToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is string && !string.IsNullOrEmpty(value as string))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class EmptyStringToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is string && !string.IsNullOrEmpty(value as string))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    // This converter expects the following properties to be bound through XAML 
    // (these properties are also to be bound in the exact order as stated here):
    // 
    //      SearchViewModel.SearchRootCategories.Count (int)
    //      SearchViewModel.SearchAddonsVisibility (bool)
    //      SearchViewModel.SearchText (string)
    //
    public class SearchResultsToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            const string message = "Wrong properties bound to SearchResultsToVisibilityConverter";

            if (values.Length != 3)
                throw new ArgumentException(message);

            if (!(values[0] is int) || !(values[1] is bool) || !(values[2] is string))
                return Visibility.Collapsed;

            var count = (int)values[0];
            var addOnVisible = (bool)values[1];
            var text = (string)values[2];

            if (count == 0 && (addOnVisible == false) && !string.IsNullOrEmpty(text))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PortToAttachmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PortType portType = ((PortType)value);
            if (((PortType)value) == PortType.INPUT)
                return DynamoToolTip.Side.Left;

            return DynamoToolTip.Side.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PortNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value is string && !string.IsNullOrEmpty(value as string))
            {
                    return value as string;   
            }

            return ">";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class SnapRegionMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            Thickness thickness = new Thickness(0, 0, 0, 0);
            PortModel port = value as PortModel;
            if (port != null)
            {
                PortType type = port.PortType;
                double left = port.MarginThickness.Left;
                double top = port.MarginThickness.Top;
                double right = port.MarginThickness.Right;
                double bottom = port.MarginThickness.Bottom;
                switch (type)
                {
                    case PortType.INPUT:
                        thickness = new Thickness(left - 25, top + 3, right + 0, bottom + 3);
                        if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top | SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left - 25, top - 10, right + 0, bottom - 10);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top))
                            thickness = new Thickness(left - 25, top - 10, right + 0, bottom + 3);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left - 25, top + 3, right + 0, bottom - 10);
                        break;

                    case PortType.OUTPUT:
                        thickness = new Thickness(left + 0, top + 3, right - 25, bottom + 3);
                        if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top | SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left + 0, top - 10, right - 25, bottom - 10);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top))
                            thickness = new Thickness(left - 25, top - 10, right + 0, bottom + 3);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left + 0, top + 3, right - 25, bottom - 10);
                        break;
                }
            }
            return thickness;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
         CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = (double)value;
            return new System.Windows.Thickness(0, -1 * height - 3, 0, 0);
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
            if (value is string && !string.IsNullOrEmpty(value as string) )
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

    public class PathToSaveStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is string && !string.IsNullOrEmpty(value as string)) ? "Saved" : "Unsaved";
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
            if (value is WorkspaceViewModel)
            {
                var val = (value as WorkspaceViewModel).Model.GetType();
                return val;
            }

            if (value is WorkspaceModel)
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

    public class WorkspaceBackgroundColorConverter : IValueConverter
    {
        public Color HomeBackgroundColor { get; set; }
        public Color CustomBackgroundColor { get; set; }

        public object Convert(object value, Type targetType, object parameter,CultureInfo culture)
        {
            //parameter will contain a true or false
            //whether this is the home space
            if ((bool)value)
                return HomeBackgroundColor;

            return CustomBackgroundColor;

        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class WorkspaceBackgroundBrushConverter : IValueConverter
    {
        public SolidColorBrush HomeBackgroundBrush { get; set; }
        public SolidColorBrush CustomBackgroundBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //parameter will contain a true or false
            //whether this is the home space
            if ((bool)value)
                return HomeBackgroundBrush;

            return CustomBackgroundBrush;

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
        // http://stackoverflow.com/questions/3238590/accessing-colors-in-a-resource-dictionary-from-a-value-converter

        public SolidColorBrush HeaderBackgroundInactive { get; set; }
        public SolidColorBrush HeaderForegroundInactive { get; set; }
        public SolidColorBrush HeaderBorderInactive { get; set; }
        public SolidColorBrush OuterBorderInactive { get; set; }
        public SolidColorBrush BodyBackgroundInactive { get; set; }
        public SolidColorBrush HeaderBackgroundActive { get; set; }
        public SolidColorBrush HeaderForegroundActive { get; set; }
        public SolidColorBrush HeaderBorderActive { get; set; }
        public SolidColorBrush OuterBorderActive { get; set; }
        public SolidColorBrush BodyBackgroundActive { get; set; }
        public SolidColorBrush HeaderBackgroundWarning { get; set; }
        public SolidColorBrush HeaderForegroundWarning { get; set; }
        public SolidColorBrush HeaderBorderWarning { get; set; }
        public SolidColorBrush OuterBorderWarning { get; set; }
        public SolidColorBrush BodyBackgroundWarning { get; set; }
        public SolidColorBrush HeaderBackgroundError { get; set; }
        public SolidColorBrush HeaderForegroundError { get; set; }
        public SolidColorBrush HeaderBorderError { get; set; }
        public SolidColorBrush OuterBorderError { get; set; }
        public SolidColorBrush BodyBackgroundError { get; set; }
        public SolidColorBrush HeaderBackgroundBroken { get; set; }
        public SolidColorBrush HeaderForegroundBroken { get; set; }
        public SolidColorBrush HeaderBorderBroken { get; set; }
        public SolidColorBrush OuterBorderBroken { get; set; }
        public SolidColorBrush BodyBackgroundBroken { get; set; }
        public SolidColorBrush OuterBorderSelection { get; set; }

        public enum NodePart
        {
            HeaderBackground,
            HeaderForeground,
            HeaderBorder,
            OuterBorder,
            BodyBackground
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ElementState elementState = ((ElementState)value);
            switch ((NodePart)Enum.Parse(typeof(NodePart), parameter.ToString()))
            {
                case NodePart.HeaderBackground:
                    return GetHeaderBackground(elementState);
                case NodePart.HeaderForeground:
                    return GetHeaderForeground(elementState);
                case NodePart.HeaderBorder:
                    return GetHeaderBorder(elementState);
                case NodePart.OuterBorder:
                    return GetOuterBorder(elementState);
                case NodePart.BodyBackground:
                    return GetBodyBackground(elementState);
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private SolidColorBrush GetHeaderBackground(ElementState elementState)
        {
            switch (elementState)
            {
                case ElementState.Dead: return HeaderBackgroundInactive;
                case ElementState.Active: return HeaderBackgroundActive;
                case ElementState.Warning: return HeaderBackgroundWarning;
                case ElementState.Error: return HeaderBackgroundError;
                case ElementState.AstBuildBroken: return HeaderBackgroundBroken;
            }

            throw new NotImplementedException();
        }

        private SolidColorBrush GetHeaderForeground(ElementState elementState)
        {
            switch (elementState)
            {
                case ElementState.Dead: return HeaderForegroundInactive;
                case ElementState.Active: return HeaderForegroundActive;
                case ElementState.Warning: return HeaderForegroundWarning;
                case ElementState.Error: return HeaderForegroundError;
                case ElementState.AstBuildBroken: return HeaderForegroundBroken;
            }

            throw new NotImplementedException();
        }

        private SolidColorBrush GetHeaderBorder(ElementState elementState)
        {
            switch (elementState)
            {
                case ElementState.Dead: return HeaderBorderInactive;
                case ElementState.Active: return HeaderBorderActive;
                case ElementState.Warning: return HeaderBorderWarning;
                case ElementState.Error: return HeaderBorderError;
                case ElementState.AstBuildBroken: return HeaderBorderBroken;
            }

            throw new NotImplementedException();
        }

        private SolidColorBrush GetOuterBorder(ElementState elementState)
        {
            switch (elementState)
            {
                case ElementState.Dead: return OuterBorderInactive;
                case ElementState.Active: return OuterBorderActive;
                case ElementState.Warning: return OuterBorderWarning;
                case ElementState.Error: return OuterBorderError;
                case ElementState.AstBuildBroken: return OuterBorderBroken;
            }

            throw new NotImplementedException();
        }

        private SolidColorBrush GetBodyBackground(ElementState elementState)
        {
            switch (elementState)
            {
                case ElementState.Dead: return BodyBackgroundInactive;
                case ElementState.Active: return BodyBackgroundActive;
                case ElementState.Warning: return BodyBackgroundWarning;
                case ElementState.Error: return BodyBackgroundError;
                case ElementState.AstBuildBroken: return BodyBackgroundBroken;
            }

            throw new NotImplementedException();
        }
    }

    public class PortCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<PortViewModel> ports = (ObservableCollection<PortViewModel>)value;
            return Math.Max(30, ports.Count * 20 + 10); //spacing for inputs + title space + bottom space
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class AttachmentToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = "0,0 6,5 0,10"; // Default, catch-all.
            DynamoToolTip tooltip = value as DynamoToolTip;
            switch (tooltip.AttachmentSide)
            {
                case DynamoToolTip.Side.Left:
                    result = "0,0 6,5 0,10";
                    break;
                case DynamoToolTip.Side.Right:
                    result = "6,0 0,5, 6,10";
                    break;
                case DynamoToolTip.Side.Top:
                    result = "0,0 5,6, 10,0";
                    break;
                case DynamoToolTip.Side.Bottom:
                    result = "0,6 5,0 10,6";
                    break;
            }

            if (parameter != null && ((parameter as string).Equals("Start")))
            {
                var index = result.IndexOf(' ');
                result = result.Substring(0, index);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AttachmentToRowColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rowColumn = parameter as string;
            if (rowColumn == null || (!rowColumn.Equals("Row") && (!rowColumn.Equals("Column"))))
            {
                var message = "'AttachmentToRowColumnConverter' expects a " + 
                    "'ConverterParameter' value to be either 'Row' or 'Column'";

                throw new ArgumentException(message);
            }

            int row = 1, column = 2;
            DynamoToolTip tooltip = value as DynamoToolTip;
            switch (tooltip.AttachmentSide)
            {
                case DynamoToolTip.Side.Left:
                    row = 1;
                    column = 2;
                    break;
                case DynamoToolTip.Side.Right:
                    row = 1;
                    column = 0;
                    break;
                case DynamoToolTip.Side.Top:
                    row = 2;
                    column = 1;
                    break;
                case DynamoToolTip.Side.Bottom:
                    row = 0;
                    column = 1;
                    break;
            }

            bool isRow = rowColumn.Equals("Row");
            return isRow ? row : column;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BrowserItemToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NodeSearchElement || value is NodeSearchElementViewModel)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
            if (targetType != typeof(bool) && (targetType != typeof(bool?)))
                throw new InvalidOperationException("The target must be a boolean");

            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return !((bool)value);
        }

        #endregion
    }

    public class BoolToCanvasCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return System.Windows.Input.Cursors.AppStarting;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ShowHideConsoleMenuItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((int)value > 0)
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
            string menuValue = "Showing Background 3D Preview";
            if ((bool)value == true)
                return menuValue;
            else
                return menuValue;
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

            return new System.Windows.Thickness(0, 0, 0, 0);
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

    public class ConsoleHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new GridLength((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((GridLength)value).Value;
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

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else if (parameter != null && parameter.ToString() == "Collapse")
                return Visibility.Collapsed;

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Hidden;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class InverseBooleanToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class NavigationToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var canNavigateBackground = ((bool)value);
            return (canNavigateBackground ? 0.1 : 1.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewButtonClipRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new System.Windows.Rect()
            {
                Width = ((double)values[0]),
                Height = ((double)values[1])
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BackgroundPreviewGestureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When "DynamoViewModel.CanNavigateBackground" is set to "true" 
            // (i.e. background 3D navigation is turned on), and "IsOrbiting"
            // is "true", then left mouse dragging will be orbiting the 3D view. 
            // Otherwise left clicking will do nothing to the view (same is 
            // applicable to "IsPanning" property).
            // 
            if ((parameter as string).Equals("IsPanning"))
            {
                bool isPanning = ((bool)value);
                return new MouseGesture(isPanning ? MouseAction.LeftClick : MouseAction.MiddleClick);
            }
            else
            {
                bool isOrbiting = ((bool)value);
                return new MouseGesture(isOrbiting ? MouseAction.LeftClick : MouseAction.None);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LacingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LacingStrategy strategy = (LacingStrategy)value;
            if (strategy == LacingStrategy.Disabled)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class LacingToAbbreviationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LacingStrategy strategy = (LacingStrategy)value;

            switch (strategy)
            {
                case LacingStrategy.Disabled:
                    return "";
                case LacingStrategy.CrossProduct:
                    return "XXX";
                case LacingStrategy.First:
                    return "|";
                case LacingStrategy.Longest:
                    return @"||\";
                case LacingStrategy.Shortest:
                    return "|";
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class LacingToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LacingStrategy strategy = (LacingStrategy)value;

            switch (strategy)
            {
                case LacingStrategy.Disabled:
                    return "Arugment lacing is disabled for this node.";
                case LacingStrategy.CrossProduct:
                    return "For two lists {a,b,c}{1,2,3} returns {a1,a2,a3}{b1,b2,b3}{c1,c2,c3}.";
                case LacingStrategy.First:
                    return "For two lists {a,b,c}{1,2,3} returns {a1}.";
                case LacingStrategy.Longest:
                    return "For two lists {a,b,c}{1,2} returns {a1,b2,c2}.";
                case LacingStrategy.Shortest:
                    return "For two lists {a,b,c}{1,2} returns {a1,b2}.";
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ZoomToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double zoom = System.Convert.ToDouble(value);

            if (zoom < .5)
                return Visibility.Hidden;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ZoomToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double number = (double)System.Convert.ChangeType(value, typeof(double));

            if (number <= .5)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PortNameToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if the port name is null or empty
            if (string.IsNullOrEmpty(value.ToString()))
                return 20;

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(double), typeof(String))]
    public class DoubleDisplay : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            string val = ((double)value).ToString("0.000", CultureInfo.InvariantCulture);
            //Debug.WriteLine(string.Format("Converting {0} -> {1}", value, val));
            return value == null ? "" : val;

        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            //return value.ToString();

            double val = 0.0;
            double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val);
            //Debug.WriteLine(string.Format("Converting {0} -> {1}", value, val));
            return val;
        }
    }

    [ValueConversion(typeof(int), typeof(String))]
    public class IntegerDisplay : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            string val = ((int)value).ToString("0", CultureInfo.InvariantCulture);
            return value == null ? "" : val;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            int val = 0;
            int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val);
            return val;
        }
    }

    public class DoubleInputDisplay : DoubleDisplay
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            //units are stored internally as culturally invariant, so we need to convert them back
            double dbl;
            if (double.TryParse(value as string, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
            {
                return (dbl.ToString(SIUnit.NumberFormat, CultureInfo.InvariantCulture));
            }
            return value ?? 0.ToString(SIUnit.NumberFormat);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            //units are entered as culture-specific, so we need to store them as invariant
            double dbl;
            if (double.TryParse(value as string, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
            {
                return dbl;
            }
            return value ?? "";
        }
    }

    public class RadianToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            //Convert radians to degrees
            double dbl;
            if (double.TryParse(value as string, NumberStyles.Any, culture, out dbl))
            {
                return dbl * 180.0 / Math.PI;
            }
            return value ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            //Convert degrees to radians
            double dbl;
            if (double.TryParse(value as string, NumberStyles.Any, culture, out dbl))
            {
                return dbl * Math.PI / 180.0;
            }
            return value ?? "";
        }
    }

    public class StringDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            return value == null ? "" : HttpUtility.HtmlDecode(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            return HttpUtility.HtmlEncode(value.ToString());
        }
    }

    public class FilePathDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source->target
            if (value == null)
                return "No file selected.";

            var str = HttpUtility.UrlDecode(value.ToString());

            if (string.IsNullOrEmpty(str))
                return "No file selected.";

            // if the number of directories deep exceeds threshold
            if (str.Length - str.Replace(@"\", "").Length >= 5)
            {
                var root = Path.GetPathRoot(str);
                var name = Path.GetFileName(str);

                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(str));

                var collapsed = new[]
                {
                    root + "...",
                    dirInfo.Parent.Parent.Name,
                    dirInfo.Parent.Name,
                    dirInfo.Name,
                    name
                };

                return string.Join(@"\", collapsed);
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return HttpUtility.UrlEncode(value.ToString());
        }
    }

    public class InverseBoolDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }
    }

    public sealed class NullToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class WarningLevelToColorConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WarningLevel)
            {
                var level = (WarningLevel) value;
                switch (level)
                {
                    case WarningLevel.Mild:
                        return new System.Windows.Media.SolidColorBrush(Colors.Gray);
                    case WarningLevel.Moderate:
                        return new System.Windows.Media.SolidColorBrush(Colors.Gold);
                    case WarningLevel.Error:
                        return new System.Windows.Media.SolidColorBrush(Colors.Tomato);
                }
            }

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {           
            TabControl tabControl = values[0] as TabControl;
            
            double tabControlActualWidth = tabControl.ActualWidth - Configurations.TabControlMenuWidth; // Need to factor in tabControlMenu

            int visibleTabsNumber = tabControl.Items.Count;

            if (visibleTabsNumber > Configurations.MinTabsBeforeClipping)
                visibleTabsNumber = Configurations.MinTabsBeforeClipping;

            double width = tabControlActualWidth / visibleTabsNumber;

            if ((tabControlActualWidth - tabControl.Items.Count * Configurations.TabDefaultWidth) >= 0 || width > Configurations.TabDefaultWidth)
                width = Configurations.TabDefaultWidth;
            
            //Subtract 1, otherwise we could overflow to two rows.
            return (width <= Configurations.TabControlMenuWidth) ? Configurations.TabControlMenuWidth : (width - 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return ScrollBarVisibility.Auto;
            }
            else if (parameter != null && parameter.ToString() == "Disabled")
                return ScrollBarVisibility.Disabled;

            return ScrollBarVisibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MeasureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var measure = (SIUnit) parameter;
            measure.SetValueFromString(value.ToString());
            return measure.Value;
        }
    }

    public class IsUpdateAvailableToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value != true) return "(Up-to-date)";

            var latest = UpdateManager.UpdateManager.Instance.AvailableVersion;

            return latest != null? latest.ToString() : "Could not get version.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsUpdateAvailableBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush;

            brush = (bool) value
                ? (SolidColorBrush)
                    SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UpdateManagerUpdateAvailableBrush"]
                : (SolidColorBrush) SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UpdateManagerUpToDateBrush"];
                
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NumberFormatToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString() == SIUnit.NumberFormat)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FullyQualifiedNameToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value.ToString();
            string typeOfInput = parameter as string;
            switch (typeOfInput)
            {
                case "ToolTip":
                    if (text.Length > Configurations.MaxLengthTooltipCode)
                        return text.Insert(text.LastIndexOf(".") + 1, "\n");
                    return text;
                case "ClassButton":
                    text = Dynamo.Nodes.Utilities.InsertSpacesToString(text);
                    if (text.Length > Configurations.MaxLengthRowClassButtonTitle)
                    {
                        if (text.IndexOf(" ") != -1)
                            text = text.Insert(text.IndexOf(" ") + 1, "\n");
                        if (text.Length > Configurations.MaxLengthClassButtonTitle)
                            // If title is too long, we can cat it.
                            text = text.Substring(0, Configurations.MaxLengthClassButtonTitle - 3) +
                                Configurations.TwoDots;
                    }
                    return text;

                // Maybe, later we need more string converters.
                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// This converter is used to format the display string for both input and output 
    /// parameters on the "TooltipWindow.xaml". If "parameter" here is "inputParam", 
    /// then this converter is invoked by input parameter related binding. A colon 
    /// character will be prefixed to the parameter type (e.g. "value : double") only 
    /// for input parameter (since an output of a function does not have a name). Also,
    /// the colon will only appended when there is actually an input parameter (for 
    /// cases without input parameter, only "none" string will be displayed so there is 
    /// no point in prefixing a colon character (e.g. we don't want ": none").
    public class InOutParamTypeConverter : IValueConverter
    {
        private static readonly string NoneString = "none";
        private static readonly string ColonString = ":";
        private static readonly string SpaceString = " ";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool shouldPrefixColon = false;

            if (parameter != null)
                shouldPrefixColon = ((parameter as string).Equals("inputParam"));

            var input = value as string;
            if (string.IsNullOrEmpty(input) || input.Equals(NoneString))
                return input;

            if (shouldPrefixColon)
                return String.Concat(ColonString, SpaceString, input);
            else
                return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// The converter switches between LibraryView and LibrarySearchView
    /// using SearchViewModel.ViewMode as value, the View as parameter.
    /// Converter is used on LibraryConatiner.
    public class ViewModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;

            if (((SearchViewModel.ViewMode)value).ToString() == parameter.ToString())
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // It's used for ClassDetails and ClassObject itself. ClassDetails should be not focusable,
    // in contrast to ClassObject.
    // Also decides, should be category underlined or not.
    public class ElementTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NodeSearchElementViewModel) 
                return false;
            if (value is BrowserInternalElementViewModel)
                return true;
            if (value is BrowserInternalElementForClassesViewModel)
                return true;

            if (value is BrowserRootElementViewModel)
            {
                var rootElement = value as BrowserRootElementViewModel;
                return !rootElement.Items.OfType<BrowserInternalElementForClasses>().Any();
            }
            return false;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // This converter is used to change color of output parameters for custom node.
    public class NodeTypeToColorConverter : IValueConverter
    {
        public SolidColorBrush TrueBrush { get; set; }
        public SolidColorBrush FalseBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CustomNodeSearchElementViewModel)
                return TrueBrush;
            return FalseBrush;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RootElementVMToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is BrowserRootElementViewModel);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BrowserInternalElementVMToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is BrowserInternalElementViewModel);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Used in addons treeview. Element, that is just under root shouldn't have dotted line at the left side.
    public class HasParentRootElement : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BrowserRootElementViewModel) return true;
            if (value is BrowserInternalElementViewModel)
            {
                return (value as BrowserInternalElementViewModel).CastedModel.Parent is BrowserRootElement;
            }
            else return false;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullValueToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Depending on the number of points in FullCategoryName margin will be done.
    // E.g. Geometry.Tesselation -> Margin="10,0,0,0"
    // E.g. RootCategory.Namespace1.Namespace2 -> Margin="20,0,0,0"
    public class FullCategoryNameToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var incomingString = value as string;

            if (string.IsNullOrEmpty(incomingString)) return new Thickness(5, 0, 0, 0);

            var numberOfPoints = incomingString.Count(x => x == Configurations.CategoryDelimiter);
            return new Thickness(5 + 20 * numberOfPoints, 0, 20, 0);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter that will be used, if number of items equals 0. Then control should be collapsed.
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value > 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter is used in LibraryView.xaml. Do not show LibraryTreeView TreviewItem ItemsHost
    // for only one item, item should be of type BrowserInternalElementForClasses.
    public class LibraryTreeItemsHostVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BrowserInternalElementForClassesViewModel)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter is used to specify Margin of highlight rectangle. 
    // This rectangle highlights first instance of search phrase.
    //
    // Input parameters:
    //     values[0] (TextBlock) - name of member. Part of this text rectangle should highlight.
    //     values[1] (SearchViewModel) - properties SearchText and RegularTypeface are used.
    public class SearchHighlightMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return new ArgumentException();

            var textBlock = values[0] as TextBlock;
            var viewModel = values[1] as SearchViewModel;
            var searchText = viewModel.SearchText;
            var typeface = viewModel.RegularTypeface;
            var fullText = textBlock.Text;

            var index = fullText.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase);
            if (index == -1)
                return new Thickness(0, 0, textBlock.ActualWidth, textBlock.ActualHeight);

            double rightMargin = textBlock.ActualWidth -
                ComputeTextWidth(fullText.Substring(0, index + searchText.Length), typeface, textBlock);

            double leftMargin = textBlock.ActualWidth - rightMargin -
                ComputeTextWidth(fullText.Substring(index, searchText.Length), typeface, textBlock);

            return new Thickness(leftMargin, 0, rightMargin, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double ComputeTextWidth(string text, Typeface typeface, TextBlock textBlock)
        {
            var formattedText = new FormattedText(text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);

            return formattedText.Width;
        }
    }

    // This converter is used to show text label of Addon type in AddonsTreeView control.
    public class ElementTypeToShorthandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var elementType = (SearchModel.ElementType)value;

            switch (elementType)
            {
                case SearchModel.ElementType.Package:
                    return Configurations.ElementTypeShorthandPackage;
                case SearchModel.ElementType.CustomDll:
                    return Configurations.ElementTypeShorthandImportedDll;
                case SearchModel.ElementType.CustomNode:
                    return Configurations.ElementTypeShorthandCategory;
                default:
                    return "NIL";
                //TODO: as logic of specifying BrowserRootElement.ElementType is implemented
                //      next line should be used.
                //throw new Exception("Incorrect value provided to converter");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }        
}
