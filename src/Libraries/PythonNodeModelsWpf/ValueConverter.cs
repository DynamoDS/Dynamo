using PythonNodeModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PythonNodeModelsWpf
{
    public class SelectedPythonEngineToMigrationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            return (PythonEngineVersion)value == PythonEngineVersion.IronPython2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
