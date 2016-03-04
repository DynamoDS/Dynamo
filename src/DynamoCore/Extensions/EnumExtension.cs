using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Dynamo.Extensions
{
    public class EnumDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;
        private readonly ResourceManager _resource;
        public EnumDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resource = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                return _resource.GetString(_resourceKey) ?? string.Empty;
            }
        }
    }

    internal static class EnumDescription
    {
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                var description = attributes[0].Description;
                return string.IsNullOrEmpty(description) ? enumValue.ToString() : description;
            }

            return enumValue.ToString();
        }
    }
}
