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
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.UpdateManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.ViewModels;
using DynamoUnits;
using RestSharp.Contrib;
using System.Text;
using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Controls
{
    public class TooltipLengthTruncater : IValueConverter
    {
        private const int MaxChars = 100;
        private const double MinFontFactor = 7.0;

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

            return Resources.UnknowDateFormat;
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
                var st = (PackageManagerSearchViewModel.PackageSearchState)value;

                if (st == PackageManagerSearchViewModel.PackageSearchState.NoResults)
                {
                    return Resources.PackageSearchStateNoResult;
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.Results)
                {
                    return "";
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.Searching)
                {
                    return Resources.PackageSearchStateSearching;
                }
                else if (st == PackageManagerSearchViewModel.PackageSearchState.Syncing)
                {
                    return Resources.PackageSearchStateSyncingWithServer;
                }
            }

            return Resources.PackageStateUnknown;
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
                    return Resources.PackageUploadStateCompressing;
                }
                else if (st == PackageUploadHandle.State.Copying)
                {
                    return Resources.PackageUploadStateCopying;
                }
                else if (st == PackageUploadHandle.State.Error)
                {
                    return Resources.PackageUploadStateError;
                }
                else if (st == PackageUploadHandle.State.Ready)
                {
                    return Resources.PackageUploadStateReady;
                }
                else if (st == PackageUploadHandle.State.Uploaded)
                {
                    return Resources.PackageUploadStateUploaded;
                }
                else if (st == PackageUploadHandle.State.Uploading)
                {
                    return Resources.PackageUploadStateUploading;
                }
            }

            return Resources.PackageStateUnknown;
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
            if (value is PackageDownloadHandle.State)
            {
                var st = (PackageDownloadHandle.State)value;

                if (st == PackageDownloadHandle.State.Downloaded)
                {
                    return Resources.PackageDownloadStateDownloaded;
                }
                else if (st == PackageDownloadHandle.State.Downloading)
                {
                    return Resources.PackageDownloadStateDownloading;
                }
                else if (st == PackageDownloadHandle.State.Error)
                {
                    return Resources.PackageDownloadStateError;
                }
                else if (st == PackageDownloadHandle.State.Installed)
                {
                    return Resources.PackageDownloadStateInstalled;
                }
                else if (st == PackageDownloadHandle.State.Installing)
                {
                    return Resources.PackageDownloadStateInstalling;
                }
                else if (st == PackageDownloadHandle.State.Uninitialized)
                {
                    return Resources.PackageDownloadStateStarting;
                }
            }

            return Resources.PackageStateUnknown;
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
    //      SearchViewModel.SearchText (string)
    public class SearchResultsToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is int && (int)values[0] == 0 && !string.IsNullOrEmpty(values[1] as string))
            {
                return Visibility.Visible;
            }

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
            if (((PortType)value) == PortType.Input)
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

    public class SnapRegionMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
          CultureInfo culture)
        {
            Thickness thickness = new Thickness(0, 0, 0, 0);
            var actualWidth = (double)values[0];
            PortModel port = values[1] as PortModel;
            if (port != null)
            {
                PortType type = port.PortType;
                double left = port.MarginThickness.Left;
                double top = port.MarginThickness.Top;
                double right = port.MarginThickness.Right;
                double bottom = port.MarginThickness.Bottom;
                switch (type)
                {
                    case PortType.Input:
                        thickness = new Thickness(left - 25, top + 3, right + actualWidth, bottom + 3);
                        if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top | SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left - 25, top - 10, right + actualWidth, bottom - 10);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top))
                            thickness = new Thickness(left - 25, top - 10, right + actualWidth, bottom + 3);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left - 25, top + 3, right + actualWidth, bottom - 10);
                        break;

                    case PortType.Output:
                        thickness = new Thickness(left + actualWidth, top + 3, right - 25, bottom + 3);
                        if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top | SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left + actualWidth, top - 10, right - 25, bottom - 10);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Top))
                            thickness = new Thickness(left + actualWidth, top - 10, right - 25, bottom + 3);
                        else if (port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom))
                            thickness = new Thickness(left + actualWidth, top + 3, right - 25, bottom - 10);
                        break;
                }
            }
            return thickness;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter,
         CultureInfo culture)
        {
            return null;
        }
    }

    public class RunPreviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var runEnabled = (bool)value;
            return runEnabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class RunPreviewToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dynamicRunEnabled = (bool)value;
            return dynamicRunEnabled ? Resources.ShowRunPreviewDisableToolTip : Resources.ShowRunPreviewEnableToolTip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
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
            if (value is string && !string.IsNullOrEmpty(value as string))
            {
                // convert to path, get file name
                return Path.GetFileName((string)value);
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
        public SolidColorBrush TrueBrush { get; set; }
        public SolidColorBrush FalseBrush { get; set; }

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

    public class ConnectionStateToBrushConverter : IValueConverter
    {
        public SolidColorBrush ExecutionPreviewBrush { get; set; }
        public SolidColorBrush NoneBrush { get; set; }
        public SolidColorBrush SelectionBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (PreviewState)value;
            switch (state)
            {
                case PreviewState.ExecutionPreview:
                    return ExecutionPreviewBrush;
                case PreviewState.None:
                    return NoneBrush;
                case PreviewState.Selection:
                    return SelectionBrush;
                default:
                    return NoneBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConnectionStateToColorConverter : IValueConverter
    {
        public Color ExecutionPreview { get; set; }
        public Color None { get; set; }
        public Color Selection { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (PreviewState)value;
            switch (state)
            {
                case PreviewState.ExecutionPreview:
                    return ExecutionPreview;
                case PreviewState.None:
                    return None;
                case PreviewState.Selection:
                    return Selection;
                default:
                    return None;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConnectionStateToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (PreviewState)value;
            switch (state)
            {
                case PreviewState.ExecutionPreview:
                    return Visibility.Visible;
                case PreviewState.None:
                    return Visibility.Collapsed;
                case PreviewState.Selection:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return HeaderBackgroundWarning;
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
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return HeaderForegroundWarning;
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
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return HeaderBorderWarning;
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
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return OuterBorderWarning;
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
                case ElementState.Warning:
                case ElementState.PersistentWarning:
                    return BodyBackgroundWarning;
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
            return Math.Max(30, ports.Count * 20 + 10); //spacing for Inputs + title space + bottom space
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
                var message = Wpf.Properties.Resources.MessageFailedToAttachToRowColumn;

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

    public class NodeSearchElementVMToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NodeSearchElementViewModel)
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
            return list.Count > 0; //spacing for Inputs + title space + bottom space
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
                return Resources.DynamoViewViewMenuHideConsole;
            }
            else
            {
                return Resources.DynamoViewViewMenuShowConsole;
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
            string menuValue = Resources.DynamoViewViewMenuShowBackground3DPreview;
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
                return Resources.HideClassicNodeLibrary;
            }
            else
            {
                return Resources.ShowClassicNodeLibrary;
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
            return string.Format(Wpf.Properties.Resources.ConverterMessageZoom, value.ToString());
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
            return string.Format(Wpf.Properties.Resources.ConverterMessageTransformOrigin, p.X, p.Y);
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
            return string.Format(Wpf.Properties.Resources.ConverterMessageCurrentOffset, p.X, p.Y);
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
            //if (p == PortType.Input)
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
            if (p == PortType.Input)
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
            if (p == PortType.Input)
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
            if (p == PortType.Input)
            {
                return new Rect(0, 0, 10, 20);
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
                    return Resources.LacingDisabledToolTip;
                case LacingStrategy.CrossProduct:
                    return Resources.LacingCrossProductToolTip;
                case LacingStrategy.First:
                    return Resources.LacingFirstToolTip;
                case LacingStrategy.Longest:
                    return Resources.LacingLongestToolTip;
                case LacingStrategy.Shortest:
                    return Resources.LacingShortestToolTip;
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
            if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                return val;
            //check if the value exceeds the 32 bit maximum / minimum value
            string integerValue = value.ToString();
            if (integerValue.Length > 1)
            {
                var start = integerValue[0] == '-' ? 1 : 0;
                for (var i = start; i < integerValue.Length; i++)
                {
                    if (!char.IsDigit(integerValue[i]))
                    {
                        return 0;
                    }
                }
                val = start == 0 ? int.MaxValue : int.MinValue;
            }
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
            return value == null ? "" : value.ToString(); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            return value.ToString();
        }
    }

    public class FilePathDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source->target
            if (value == null)
                return Resources.FilePathConverterNoFileSelected;

            var str = HttpUtility.UrlDecode(value.ToString());

            if (string.IsNullOrEmpty(str))
                return Resources.FilePathConverterNoFileSelected;

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

    public sealed class WarningLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WarningLevel)
            {
                var level = (WarningLevel)value;
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
            var measure = (SIUnit)parameter;
            measure.SetValueFromString(value.ToString());
            return measure.Value;
        }
    }

    public class IsUpdateAvailableToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var um = value as IUpdateManager;
            if (um == null)
                return Resources.AboutWindowCannotGetVersion;

            if (!um.IsUpdateAvailable) 
                return Resources.AboutWindowUpToDate;
            
            var latest = um.AvailableVersion;

            return latest != null ? latest.ToString() : Resources.AboutWindowCannotGetVersion;
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

            brush = (bool)value
                ? (SolidColorBrush)
                    SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UpdateManagerUpdateAvailableBrush"]
                : (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UpdateManagerUpToDateBrush"];

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

                    int maxRowLength = Configurations.MaxLengthRowClassButtonTitle;
                    int maxRowNumbers = Configurations.MaxRowNumber;

                    var words = Dynamo.Nodes.Utilities.WrapText(text, maxRowLength);
                    if (words.Count() > maxRowNumbers)
                        words = Dynamo.Nodes.Utilities.ReduceRowCount(words.ToList(), maxRowNumbers);

                    words = Dynamo.Nodes.Utilities.TruncateRows(words, maxRowLength);
                    text = String.Join("\n", words);

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
        private static readonly string NoneString = Resources.NoneString;
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
                return String.Concat(SpaceString, ColonString, SpaceString, input);
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
            if (value is RootNodeCategoryViewModel)
            {
                var rootElement = value as RootNodeCategoryViewModel;
                return !rootElement.SubCategories.OfType<ClassesNodeCategoryViewModel>().Any();
            }
            if (value is NodeCategoryViewModel)
                return true;

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
            return (value is RootNodeCategoryViewModel);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NodeCategoryVMToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is NodeCategoryViewModel) &&
                (!(value is RootNodeCategoryViewModel)) &&
                (!(value is ClassesNodeCategoryViewModel));
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
    // E.g. Geometry -> Margin="5,0,0,0"
    // E.g. RootCategory.Namespace1.Namespace2 -> Margin="45,0,20,0"
    public class FullCategoryNameToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var incomingString = value as string;

            if (string.IsNullOrEmpty(incomingString))
                throw new ArgumentException("value string should not be empty.");

            var c = Configurations.CategoryDelimiterString[0];
            var numberOfPoints = incomingString.Count(x => x == c);
            if (numberOfPoints == 0)
                return new Thickness(5, 0, 0, 0);

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
    // for only one item, item should be of type ClassesNodeCategoryViewModel.
    public class LibraryTreeItemsHostVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClassesNodeCategoryViewModel)
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

    // This converter is used to show text label of Addon type.
    public class ElementTypeToShorthandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Implement converter. Refer to http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6197
            // for more details.
#if false
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
#endif
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MenuItemCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontsize = System.Convert.ToDouble(value);
            var param = System.Convert.ToDouble(parameter);

            return fontsize == param;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AnnotationTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value == null ? String.Empty:value.ToString();             
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value.ToString();           
            return text;
        }
    }

    internal class Watch3DBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var homeColor = (System.Windows.Media.Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundHome"];
            var customColor = (System.Windows.Media.Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundCustom"];

            //parameter will contain a true or false
            //whether this is the home space
            if ((bool)value)
            {
                return homeColor.ToColor4();
            }

            return customColor.ToColor4();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return null;
        }
    }

    public class GroupFontSizeToEditorEnabledConverter : IMultiValueConverter
    {
        private const double MinFontFactor = 7.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var zoom = System.Convert.ToDouble(values[0]);
            var fontsize = System.Convert.ToDouble(values[1]);

            var factor = zoom*fontsize;
            if (factor < MinFontFactor)
            {
                return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupTitleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Visibility.Visible;
            if (parameter.ToString() == "FlipTextblock")
            {
                if ((Visibility) value == Visibility.Collapsed)
                {
                    return Visibility.Visible;
                }
            }
            else if (parameter.ToString() == "FlipTextbox")
            {
                return (Visibility)value; 
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
        /// Converts element type of node search element in short string.
        /// E.g. ElementTypes.Packaged => PKG.
        /// </summary>
        public class ElementTypeToShortConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var type = (ElementTypes) value;

                switch (type)
                {
                    case ElementTypes.Packaged:
                        return Resources.PackageTypeShortString;

                    case ElementTypes.Packaged | ElementTypes.ZeroTouch:
                        return Resources.PackageTypeShortString;

                    case ElementTypes.Packaged | ElementTypes.CustomNode:
                        return Resources.PackageTypeShortString;

                    case ElementTypes.Packaged | ElementTypes.ZeroTouch | ElementTypes.CustomNode:
                        return Resources.PackageTypeShortString;

                    case ElementTypes.ZeroTouch:
                        return Resources.ZeroTouchTypeShortString;

                    case ElementTypes.CustomNode:
                        return Resources.CustomNodeTypeShortString;

                    case ElementTypes.BuiltIn:
                    case ElementTypes.None:
                    default:
                        return string.Empty;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converter is used in search library view. If current mode is LibraryView, then hide found members.
        /// Otherwise show found members.
        /// </summary>
        public class LibraryViewModeToBoolConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var mode = (SearchViewModel.ViewMode)value;
                return mode == SearchViewModel.ViewMode.LibraryView;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }    
}
