using Dynamo.PythonServices;
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
            return (string)value == PythonEngineManager.IronPython2EngineName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
