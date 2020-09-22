using System;

using Resources = DSCore.Properties.Resources;

namespace DSCore
{
    /// <summary>
    ///     Generic functions that operate on all data.
    /// </summary>
    public static class Object
    {
        /// <summary>
        ///     Determines if the given object is null.
        /// </summary>
        /// <param name="object">Object to test.</param>
        /// <returns name="bool">Whether object is null.</returns>
        /// <search>is null</search>
        public static bool IsNull(object @object)
        {
            return @object == null;
        }

        /// <summary>
        ///     Returns what is passed in, doing nothing.
        /// </summary>
        /// <param name="object">An object.</param>
        /// <returns name="object">Same object</returns>
        public static object Identity(object @object)
        {
            return @object;
        }

        /// <summary>
        ///     Returns the type of object represented as string.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>Type of object.</returns>
        public static string Type(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException(Resources.ObjectArgumentExceptionMessage, "obj");
            }
            return obj.GetType().ToString();
        }
    }
}
