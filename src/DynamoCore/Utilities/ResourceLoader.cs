using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dynamo.Utilities
{
    static internal class ResourceLoader
    {
        /// <summary>
        /// Loads resource string from resource type.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceName">resource name</param>
        /// <returns>resource value</returns>
        public static string Load(Type resourceType, string resourceName)
        {
            if ((resourceType == null) || (resourceName == null))
            {
                return String.Empty;
            }

            var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static);

            if (property == null)
            {
                throw new InvalidOperationException("Resource Type does not have Property");
            }
            if (property.PropertyType != typeof(string))
            {
                throw new InvalidOperationException("Resource Property is not String Type");
            }
            return (string)property.GetValue(null, null);
        }

        /// <summary>
        /// Loads several resource strings from resource type.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        /// <returns>resource values</returns>
        public static IEnumerable<string> Load(Type resourceType, IEnumerable<string> resourceNames)
        {
            if ((resourceType == null) || (resourceNames == null))
            {
                return new List<string>();
            }

            var titles = new List<string>();
            foreach (var resourceName in resourceNames)
            {
                var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static);

                if (property == null)
                {
                    throw new InvalidOperationException("Resource Type Does Not Have Property");
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException("Resource Property is Not String Type");
                }
                titles.Add((string)property.GetValue(null, null));
            }

            return titles;
        }
    }
}
