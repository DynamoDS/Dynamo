﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using CSMath = System.Math;

namespace DSCore
{
    /// <summary>
    /// Methods for performing Mathematical operations.
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Generates a random double in the range of [0, 1).
        /// </summary>
        /// <param name="seed">Seed value for the random number generator.</param>
        public static double RandomSeed(int seed)
        {
            return new Random(seed).NextDouble();
        }

        /// <summary>
        /// Produces a list containing the given amount of random doubles
        /// in the range of [0, 1).
        /// </summary>
        /// <param name="amount">Amount of random numbers the result list will contain.</param>
        public static IList RandomList(int amount)
        {
            var result = new ArrayList();

            var r = new Random();
            foreach (var x in Enumerable.Range(0, amount).Select(_ => r.NextDouble()))
                result.Add(x);

            return result;
        }

        /// <summary>
        /// Pi Constant Multiplied by 2
        /// </summary>
        public static double PiTimes2
        {
            get
            {
                return System.Math.PI*2;
            }
        }

        /// <summary>
        /// Averages a list of numbers.
        /// </summary>
        /// <param name="numbers">List of numbers to be averaged.</param>
        public static double Average(IList<double> numbers)
        {
            //DS will do proper marshaling, even if integer is sent.
            return numbers.Average();
        }

        /// <summary>
        ///     Adjusts the range of a list of numbers while preserving the
        ///     distribution ratio.
        /// </summary>
        /// <param name="numbers">List of numbers to adjust range of.</param>
        /// <param name="newMin">New minimum of the range.</param>
        /// <param name="newMax">New maximum of the range</param>
        public static IList RemapRange(IList<double> numbers, double newMin = 0, double newMax = 1)
        {
            var oldMax = numbers.Max();
            var oldMin = numbers.Min();
            var oldRange = oldMax - oldMin;
            var newRange = newMax - newMin;
            return numbers.Select(oldValue => ((oldValue - oldMin) * newRange) / oldRange + newMin).ToList();
        }

        /// <summary>
        /// move the functions/methods from Math.dll to this DSCoreNode.dll
        /// </summary>
        internal static readonly double kRadiansToDegrees = 180.0 / PI;
        internal static readonly double kDegreesToRadians = PI / 180.0;

        public static double PI { get { return 3.141592653589793; } }

        public static double E { get { return 2.718281828459045; } }

        public static double GoldenRatio { get { return 1.61803398875; } }

        public static double RadiansToDegrees(double value)
        {
            return value * kRadiansToDegrees;
        }

        public static double DegreesToRadians(double value)
        {
            return value * kDegreesToRadians;
        }

        public static double Abs(double d_value)
        {
            return CSMath.Abs(d_value);
        }

        public static long Abs(long n_value)
        {
            return CSMath.Abs(n_value);
        }

        public static double Acos(double value)
        {
            return CSMath.Acos(value) * kRadiansToDegrees;
        }

        public static double Asin(double value)
        {
            return CSMath.Asin(value) * kRadiansToDegrees;
        }

        public static double Atan(double value)
        {
            return CSMath.Atan(value) * kRadiansToDegrees;
        }

        public static double Atan2(double value1, double value2)
        {
            return CSMath.Atan2(value1, value2) * kRadiansToDegrees;
        }

        public static long Ceiling(double value)
        {
            return (long)CSMath.Ceiling(value);
        }
        public static double Cos(double value)
        {
            return CSMath.Cos(value * kDegreesToRadians);
        }
        public static double Cosh(double value)
        {
            return CSMath.Cosh(value);
        }

        public static long DivRem(long value1, long value2)
        {
            long remainder = 0;
            CSMath.DivRem(value1, value2, out remainder);
            return remainder;
        }

        public static double Exp(double value)
        {
            return CSMath.Exp(value);
        }

        public static long Floor(double value)
        {
            return (long)CSMath.Floor(value);
        }
        [IsVisibleInDynamoLibrary(false)]
        public static double IEEERemainder(double value1, double value2)
        {
            return CSMath.IEEERemainder(value1, value2);
        }
        public static double Log(double value)
        {
            return CSMath.Log(value);
        }
        public static double Log(double value1, double value2)
        {
            return CSMath.Log(value1, value2);
        }
        public static double Log10(double value)
        {
            return CSMath.Log10(value);
        }

        public static double Max(double d_value1, double d_value2)
        {
            return CSMath.Max(d_value1, d_value2);
        }

        public static long Max(long n_value1, long n_value2)
        {
            return CSMath.Max(n_value1, n_value2);
        }

        public static double Min(double d_value1, double d_value2)
        {
            return CSMath.Min(d_value1, d_value2);
        }

        public static long Min(long n_value1, long n_value2)
        {
            return CSMath.Min(n_value1, n_value2);
        }

        public static double Pow(double value1, double value2)
        {
            return CSMath.Pow(value1, value2);
        }

        public static double Rand()
        {
            return mRandom.NextDouble();
        }
        public static double Rand(double value1, double value2)
        {
            double result = Min(value1, value2) + Abs(value2 - value1) * mRandom.NextDouble();
            return result;
        }
        public static double Round(double value)
        {
            return CSMath.Round(value);
        }

        /* DISABLE - LC - 070 Pre-release
        public static double Round(double value, MidpointRounding mode)
        {
            return CSMath.Round(value, mode);
        }
        */

        public static double Round(double value, int digits)
        {
            return CSMath.Round(value, digits);
        }

        /* DISABLE - LC - 070 Pre-release
        public static double Round(double value, int digits, MidpointRounding mode)
        {
            return CSMath.Round(value, digits, mode);
        }
        */

        public static long Sign(double d_value)
        {
            return CSMath.Sign(d_value);
        }

        public static long Sign(long n_value)
        {
            return CSMath.Sign(n_value);
        }

        public static double Sin(double value)
        {
            return CSMath.Sin(value * kDegreesToRadians);
        }
        public static double Sinh(double value)
        {
            return CSMath.Sinh(value);
        }
        public static double Sqrt(double value)
        {
            return CSMath.Sqrt(value);
        }
        public static double Tan(double value)
        {
            if (!(Double.Equals(CSMath.IEEERemainder(value, 180), 0.0) || Double.Equals(CSMath.IEEERemainder(value, 180), 180.0))
                && (Double.Equals(CSMath.IEEERemainder(value, 90), 0.0) || Double.Equals(CSMath.IEEERemainder(value, 90), 90.0)))
                return Double.NaN;
            return CSMath.Tan(value * kDegreesToRadians);
        }
        public static double Tanh(double value)
        {
            return CSMath.Tanh(value);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static double Truncate(double value)
        {
            return CSMath.Truncate(value);
        }
        public static long Factorial(long value)
        {
            if (value < 0)
            {
                return -1;
            }
            return (value > 1) ? value * Factorial(value - 1) : 1;
        }

        private static Random mRandom = new Random();
    }
}
