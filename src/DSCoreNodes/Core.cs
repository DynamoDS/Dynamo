using System;
using System.Linq;
using System.Security.Policy;

namespace DSCoreNodes
{
    /// <summary>
    /// 
    /// </summary>
    class Function
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Func<object, object> Compose(params Func<object, object>[] funcs)
        {
            if (!funcs.Any())
            {
                throw new ArgumentException("Need at least one function to perform composition.");
            }

            return funcs.Skip(1).Aggregate(funcs[0], (func, a) => x => func(a(x)));
        }

        /// <summary>
        /// Returns whatever is passed in.
        /// </summary>
        /// <param name="x">Anything.</param>
        /// <returns>The input.</returns>
        public static object Identity(object x)
        {
            return x;
        }
    }
}
