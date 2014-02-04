using System;
using System.Linq;

namespace DSCore
{
    /// <summary>
    /// Methods for handling higher-order functions.
    /// </summary>
    public class Function
    {
        private Function() { }

        /// <summary>
        ///     Composes multiple single parameter functions into a one single parameter function.
        /// </summary>
        /// <param name="funcs">Functions to be composed.</param>
        public static Delegate Compose(params Delegate[] funcs)
        {
            if (!funcs.Any())
            {
                throw new ArgumentException("Need at least one function to perform composition.");
            }

            return funcs.Skip(1)
                        .Aggregate(
                            funcs[0],
                            (func, a) =>
                                new Func<object, object>(
                                    x => func.DynamicInvoke(a.DynamicInvoke(x))));
        }

        /// <summary>
        ///     Returns whatever is passed in.
        /// </summary>
        /// <param name="x">Anything.</param>
        public static object Identity(object x)
        {
            return x;
        }

        /// <summary>
        ///     Applies a function to arguments.
        /// </summary>
        /// <param name="func">Function to apply.</param>
        /// <param name="args">Arguments to be passed to function.</param>
        public static object Apply(Delegate func, params object[] args)
        {
            return func.DynamicInvoke(args);
        }
    }
}
