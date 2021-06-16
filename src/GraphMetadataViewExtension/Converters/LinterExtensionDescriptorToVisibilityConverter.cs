using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dynamo.Linting;

namespace Dynamo.GraphMetadata.Converters
{
    /// <summary>
    /// Converter to control visibility based on a LinterExtensionDescriptor
    /// </summary>
    public class LinterExtensionDescriptorToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LinterExtensionDescriptor extensionDescriptor) || extensionDescriptor == LinterExtensionDescriptor.DefaultDescriptor)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
