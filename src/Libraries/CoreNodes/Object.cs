using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
