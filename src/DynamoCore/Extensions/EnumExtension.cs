using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Provides description for enum member.
    /// </summary>
    public class EnumDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;
        private readonly ResourceManager _resource;

        /// <summary>
        /// Creates EnumDescriptionAttribute.
        /// </summary>
        /// <param name="resourceKey">Resource name.</param>
        /// <param name="resourceType">Resource type, i.e. type of resource class. E.g. typeof(Resources)</param>
        public EnumDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resource = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        /// <summary>
        /// Description of enum item.
        /// </summary>
        public override string Description
        {
            get
            {
                return _resource.GetString(_resourceKey) ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Extension class, that allows adding description to Enum members.
    /// </summary>
    internal static class EnumDescription
    {
        /// <summary>
        /// Extension method. Allows getting description of Enum member provided by EnumDescriptionAttribute.
        /// </summary>
        /// <param name="enumValue">Enum member.</param>
        /// <returns>Description</returns>
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
