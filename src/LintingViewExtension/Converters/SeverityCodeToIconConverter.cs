using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Dynamo.Linting.Interfaces;

namespace Dynamo.LintingViewExtension.Converters
{
    [ValueConversion(typeof(SeverityCodesEnum), typeof(FontAwesome.WPF.FontAwesomeIcon))]
    internal class SeverityCodeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SeverityCodesEnum severityCode))
                return null;

            switch (severityCode)
            {
                case SeverityCodesEnum.Warning:
                    return FontAwesome.WPF.FontAwesomeIcon.ExclamationTriangle;
                case SeverityCodesEnum.Error:
                    return FontAwesome.WPF.FontAwesomeIcon.TimesCircle;
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
