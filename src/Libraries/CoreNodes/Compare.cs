using System;
using System.Windows.Annotations;

namespace DSCore
{
    /// <summary>
    ///     Comparison methods.
    /// </summary>
    public static class Compare
    {
        /// <summary>
        ///     Returns true if a is greater than b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>greater,larger,bigger</search>
        public static bool GreaterThan(IComparable a, IComparable b)
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
            return a.CompareTo(b) > 0;
        }

        /// <summary>
        ///     Returns true if a is greater than or equal to b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>greater,larger,bigger,equal</search>
        public static bool GreaterThanOrEqual(IComparable a, IComparable b)
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
            return a.CompareTo(b) >= 0;
        }

        /// <summary>
        ///     Returns true if a is less than b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>less,smaller</search>
        public static bool LessThan(IComparable a, IComparable b)
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
            return a.CompareTo(b) < 0;
        }

        /// <summary>
        ///     Returns true if a is less than or equal to b.
        /// </summary>
        /// <param name="a">A comparable object.</param>
        /// <param name="b">A comparable object.</param>
        /// <returns name="bool">Boolean result.</returns>
        /// <search>less,smaller,equal</search>
        public static bool LessThanOrEqual(IComparable a, IComparable b)
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
            return a.CompareTo(b) <= 0;
        }
    }
}