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
        ///     Determines the if the given object is null.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <returns name="bool">Whether object is null.</returns>
        /// <search>is null</search>
        public static bool IsNull(object obj)
        {
            return obj == null;
        }

        /// <summary>
        ///     Returns what is passed in, doing nothing.
        /// </summary>
        /// <param name="obj">An object.</param>
        public static object Identity(object obj)
        {
            return obj;
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
