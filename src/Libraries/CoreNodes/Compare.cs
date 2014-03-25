using System;
using System.Windows.Annotations;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    ///     Comparison methods.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)] 
    public static class Compare
    {
        /// <summary>
        ///     Returns true if a is greater than b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>greater,larger,bigger</search>
        public static bool GreaterThan(object a, object b)
        {
            if (a is double)
            {
                if (b is int)
                    return (double)a > (int)b;
            }
            else if (a is int)
            {
                if (b is double)
                    return (int)a > (double)b;
            }
            return ((IComparable)a).CompareTo(b) > 0;
        }

        /// <summary>
        ///     Returns true if a is greater than or equal to b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>greater,larger,bigger,equal</search>
        public static bool GreaterThanOrEqual(object a, object b)
        {
            if (a is double)
            {
                if (b is int)
                    return (double)a >= (int)b;
            }
            else if (a is int)
            {
                if (b is double)
                    return (int)a >= (double)b;
            }
            return ((IComparable)a).CompareTo(b) >= 0;
        }

        /// <summary>
        ///     Returns true if a is less than b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>less,smaller</search>
        public static bool LessThan(object a, object b)
        {
            if (a is double)
            {
                if (b is int)
                    return (double)a < (int)b;
            }
            else if (a is int)
            {
                if (b is double)
                    return (int)a < (double)b;
            }
            return ((IComparable)a).CompareTo(b) < 0;
        }

        /// <summary>
        ///     Returns true if a is less than or equal to b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>less,smaller,equal</search>
        public static bool LessThanOrEqual(object a, object b)
        {
            if (a is double)
            {
                if (b is int)
                    return (double)a <= (int)b;
            }
            else if (a is int)
            {
                if (b is double)
                    return (int)a <= (double)b;
            }
            return ((IComparable)a).CompareTo(b) <= 0;
        }
    }
}