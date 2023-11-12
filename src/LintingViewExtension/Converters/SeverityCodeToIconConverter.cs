using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Dynamo.Linting.Interfaces;
using FontAwesome5;

namespace Dynamo.LintingViewExtension.Converters
{
    [ValueConversion(typeof(SeverityCodesEnum), typeof(EFontAwesomeIcon))]
    internal class SeverityCodeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SeverityCodesEnum severityCode))
                return null;

            switch (severityCode)
            {
                case SeverityCodesEnum.Warning:
                    return EFontAwesomeIcon.Solid_ExclamationTriangle;
                case SeverityCodesEnum.Error:
                    return EFontAwesomeIcon.Solid_TimesCircle;
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
