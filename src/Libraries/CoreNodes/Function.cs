using System;
using System.Linq;

namespace DSCore
{
    /// <summary>
    /// TODO: Move contents somewhere else.
    /// </summary>
    public class Function
    {
        private Function() { }

        /// <summary>
        ///     Returns whatever is passed in.
        /// </summary>
        /// <param name="x">Anything.</param>
        public static object Identity(object x)
        {
            return x;
        }
    }
}
