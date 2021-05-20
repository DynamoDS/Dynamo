using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Linting.Interfaces;

namespace Dynamo.LintingViewExtension.Converters
{
    [ValueConversion(typeof(SeverityCodesEnum), typeof(SolidColorBrush))]
    internal class SeverityCodeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SeverityCodesEnum severityCode))
                return null;

            switch (severityCode)
            {
                case SeverityCodesEnum.Warning:
                    return new SolidColorBrush(Colors.Yellow);
                case SeverityCodesEnum.Error:
                    return new SolidColorBrush(Colors.Red);
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
