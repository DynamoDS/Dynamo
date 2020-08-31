using System;
using System.Windows;
using System.Windows.Data;

namespace Dynamo.PythonMigration.Differ
{
    public enum State
    {
        NoChanges,
        HasChanges,
        Error
    }

    public class DiffStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var state = (State) value;
                if (state == State.Error)
                    return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public interface IDiffViewViewModel
    {
        ViewMode ViewMode { get; }
        State DiffState { get; set; }
    }

    public enum ViewMode
    {
        Inline,
        SideBySide
    }
}
