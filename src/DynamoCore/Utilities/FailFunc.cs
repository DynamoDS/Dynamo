using System;

namespace Dynamo.Utilities
{
    public static class FailFunc
    {
        /// <summary>
        ///     Attempt to run a computation.  If the function throws, return the second 
        ///     argument unmodified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function</param>
        /// <param name="failureResult">The result</param>
        /// <returns></returns>
        public static T TryExecute<T>(Func<T> func, T failureResult)
        {
            try
            {
                return func();
            }
            catch
            {
                return failureResult;
            }
        }
    }
}
