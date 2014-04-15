using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RevitTestFrameworkRunner
{
    public class WorkingPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? "Select a working path...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class AssemblyPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? "Select a test assembly path...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ResultsPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? "Select a results path...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // Do the conversion from string to int
            int converted = 0;
            try
            {
                converted = Int32.Parse(value.ToString());
            }
            catch { }

            return converted;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // Do the conversion from int to string
            return value.ToString();
        }
    }

    public class TestStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var status = (TestStatus)value;
            switch (status)
            {
                case TestStatus.None:
                    return new SolidColorBrush(Colors.White);
                case TestStatus.Cancelled:
                    return  new SolidColorBrush(Colors.LightGray);
                case TestStatus.Error:
                    return new SolidColorBrush(Colors.OrangeRed);
                case TestStatus.Failure:
                    return new SolidColorBrush(Colors.OrangeRed);
                case TestStatus.Ignored:
                    return new SolidColorBrush(Colors.White);
                case TestStatus.Inconclusive:
                    return new SolidColorBrush(Colors.DarkRed);
                case TestStatus.NotRunnable:
                    return new SolidColorBrush(Colors.DarkGray);
                case TestStatus.Skipped:
                    return new SolidColorBrush(Colors.White);
                case TestStatus.Success:
                    return new SolidColorBrush(Colors.GreenYellow);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
