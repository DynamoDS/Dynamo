using CoreNodeModelsWpf.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace CoreNodeModelsWpf.Converters
{
    internal class EnumGraphTypeConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GraphTypes ct = (GraphTypes)value;
            GraphTypes pt = (GraphTypes)parameter;

            if (ct.Equals(pt))
                return Visibility.Visible;

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GraphTypes)parameter;
        }
    }
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;

        public Type EnumType
        {
            get
            {
                return _enumType;
            }

            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumtype = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumtype.IsEnum)
                            throw new ArgumentException("Type must be for an Enum");
                    }

                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension()
        {

        }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == _enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualenumtype = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            Array enumvalues = Enum.GetValues(actualenumtype);

            if (actualenumtype == _enumType)
            {
                return enumvalues;
            }

            Array temparray = Array.CreateInstance(actualenumtype, enumvalues.Length + 1);
            enumvalues.CopyTo(temparray, 1);
            return temparray;
        }
    }

    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type) { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
