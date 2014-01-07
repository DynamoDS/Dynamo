using System;
using System.Linq;

namespace DSCoreNodes
{
    /// <summary>
    /// Methods for handling higher-order functions.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Type of all functions that are used for Mapping operations.
        /// </summary>
        /// <param name="args">Variable number of arguments to be mapped/combined.</param>
        /// <returns>Combined result of the input arguments.</returns>
        public delegate object MapDelegate(params object[] args);

        /// <summary>
        /// Composes multiple single parameter functions into a one single parameter function.
        /// </summary>
        /// <param name="funcs">Functions to be composed.</param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Apply(MapDelegate func, params object[] args)
        {
            return func(args);
        }
    }
}
