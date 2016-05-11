using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;

namespace Dynamo.Diagnostics
{
    public class ExecutionTimeToRadiusConverter : IValueConverter
    {
        public int Offset { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int size = 0;
            if (Int32.TryParse(value.ToString(), out size))
                return GetNormalizedSize(size) + Offset;

            return 60 + Offset; //default value
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static int GetNormalizedSize(int value)
        {
            if (value < 10) return 50;
            
            return 100*value/DiagnosticsSession.MaxExecutionTime + 50;
        }
    }

    public class ExecutionPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int percent;
            if (!Int32.TryParse(value.ToString(), out percent))
                return "Invalid Data";

            percent = 100 * percent / DiagnosticsSession.TotalExecutionTime;
            return string.Format("{0}%", percent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public IntToStringConverter()
        {
            Prefix = string.Empty;
            Suffix = string.Empty;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0}{1}{2}", Prefix, value, Suffix);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NodeDataToPositionConverter : IValueConverter
    {
        public bool X { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var data = value as NodeData;
            if (data == null) return -1;

            int size = ExecutionTimeToRadiusConverter.GetNormalizedSize(data.ExecutionTime) + 10;
            if (X) return data.Node.X + (data.Node.Width- size) / 2;
            return data.Node.Y + (data.Node.Height - size) / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
                return System.Windows.Visibility.Collapsed;

            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum LegendType
    {
        Input,
        Output
    }

    public class ChartItem
    {
        public int Time { get; set; }

        public int Size { get; set; }

        public LegendType Type { get; set; }
    }

    public class PerformanceDataToChartItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var items = new List<ChartItem>();
            var dataCollection = value as IEnumerable;
            foreach (PerformanceData item in dataCollection)
            {
                if(DiagnosticsExtension.InputDiagnostics)
                    items.Add(new ChartItem { Type = LegendType.Input, Size = item.InputSize, Time = item.ExecutionTime });
                if(DiagnosticsExtension.OutputDiagnostics)
                    items.Add(new ChartItem { Type = LegendType.Output, Size = item.OutputSize, Time = item.ExecutionTime });
            }
            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BarChartVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DiagnosticsExtension.InputDiagnostics || DiagnosticsExtension.OutputDiagnostics)
                return System.Windows.Visibility.Visible;

            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
