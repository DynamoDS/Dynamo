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
        /// <param name="o"></param>
        public static bool IsNull(object o)
        {
            return o == null;
        }
    }
}
