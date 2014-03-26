using System;
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
        ///     Generates a random double in the range of [0, 1).
        /// </summary>
        /// <param name="seed">Seed value for the random number generator.</param>
        /// <returns name="number">Random number between 0 and 1.</returns>
        /// <search>random,seed</search>
        public static double RandomSeed(int seed)
        {
            return new Random(seed).NextDouble();
        }

        /// <summary>
        ///     Produces a list containing the given amount of random doubles
        ///     in the range of [0, 1).
        /// </summary>
        /// <param name="amount">Amount of random numbers the result list will contain.</param>
        /// <returns name="number">List of random numbers between 0 and 1.</returns>
        /// <search>random</search>
        public static IList RandomList(int amount)
        {
            var result = new ArrayList();

            var r = new Random();
            foreach (var x in Enumerable.Range(0, amount).Select(_ => r.NextDouble()))
                result.Add(x);

            return result;
        }

        /// <summary>
        ///     Pi Constant Multiplied by 2
        /// </summary>
        /// <returns name="2pi">2 times PI.</returns>
        /// <search>pi,2pi,2*pi,two</search>
        public static double PiTimes2
        {
            get
            {
                return System.Math.PI*2;
            }
        }

        /// <summary>
        ///     Averages a list of numbers.
        /// </summary>
        /// <param name="numbers">List of numbers to be averaged.</param>
        /// <returns name="average">Average of the list of numbers.</returns>
        /// <search>average,avg,mean</search>
        public static double Average(IList<double> numbers)
        {
            //DS will do proper marshalling, even if integer is sent.
            return numbers.Average();
        }

        /// <summary>
        ///     Adjusts the range of a list of numbers while preserving the
        ///     distribution ratio.
        /// </summary>
        /// <param name="numbers">List of numbers to adjust range of.</param>
        /// <param name="newMin">New minimum of the range.</param>
        /// <param name="newMax">New maximum of the range</param>
        /// <returns name="list">List remapped to new range.</returns>
        /// <search>remap,range,remaprange</search>
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

        /// <summary>
        ///     The mathematical constant Pi, 3.14159...
        /// </summary>
        /// <returns name="pi">The constant Pi.</returns>
        /// <search>pi,3.14</search>
        public static double PI { get { return 3.141592653589793; } }

        /// <summary>
        ///     The mathematical constant e, 2.71828...
        /// </summary>
        /// <returns name="e">The constant e.</returns>
        /// <search>e,natural,log,logarithm,ln,exp,exponential</search>
        public static double E { get { return 2.718281828459045; } }

        /// <summary>
        ///     The golden ratio, (1 + sqrt(5))/2 = 1.61803...
        /// </summary>
        /// <returns name="phi">The golden ratio.</returns>
        /// <search>golden,ratio,divine,phi,tau</search>
        public static double GoldenRatio { get { return 1.61803398875; } }

        /// <summary>
        ///     Converts an angle in radians to an angle in degrees.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns name="degrees">Angle in degrees.</returns>
        /// <search>radians,degrees,angle</search>
        public static double RadiansToDegrees(double radians)
        {
            return radians * kRadiansToDegrees;
        }

        /// <summary>
        ///     Converts an angle in degrees to an angle in radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns name="radians">Angle in radians.</returns>
        /// <search>degrees,radians,angle</search>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * kDegreesToRadians;
        }

        /// <summary>
        ///     Finds the absolute value of a number.
        /// </summary>
        /// <param name="d_value">Number.</param>
        /// <returns name="absoluteValue">Absolute value of the number.</returns>
        /// <search>absolute,magnitude</search>
        public static double Abs(double d_value)
        {
            return CSMath.Abs(d_value);
        }

        /// <summary>
        ///     Finds the absolute value of a number.
        /// </summary>
        /// <param name="n_value">Number.</param>
        /// <returns name="absoluteValue">Absolute value of the number.</returns>
        /// <search>absolute,magnitude</search>
        public static long Abs(long n_value)
        {
            return CSMath.Abs(n_value);
        }

        /// <summary>
        ///     Finds the inverse cosine, the angle whose cosine is the given ratio.
        /// </summary>
        /// <param name="ratio">The cosine of the angle, a number in the range [-1, 1].</param>
        /// <returns name="angle">The angle whose cosine is the input ratio.</returns>
        /// <search>acos,arccos</search>
        public static double Acos(double ratio)
        {
            return CSMath.Acos(ratio) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Finds the inverse sine, the angle whose sine is the given ratio.
        /// </summary>
        /// <param name="ratio">The sine of the angle, a number in the range [-1, 1].</param>
        /// <returns name="angle">The angle whose cosine is the input ratio.</returns>
        /// <search>asin,arcsin</search>
        public static double Asin(double ratio)
        {
            return CSMath.Asin(ratio) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Finds the inverse tangent, the angle whose tangent is the given ratio.
        /// </summary>
        /// <param name="ratio">The tangent of the angle.</param>
        /// <returns name="angle">The angle whose tangent is the input ratio.</returns>
        /// <search>atan,arctan</search>
        public static double Atan(double ratio)
        {
            return CSMath.Atan(ratio) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Finds the inverse tangent of quotient of two numbers. Returns the angle
        ///     whose tangent is the ratio: numerator/denominator.
        /// </summary>
        /// <param name="numerator">The numerator of the tangent of the angle.</param>
        /// <param name="denominator">The denominator of the tangent of the angle.</param>
        /// <returns name="angle">The angle whose tangent is numerator/denominator.</returns>
        /// <search>atan,arctan</search>
        public static double Atan2(double numerator, double denominator)
        {
            return CSMath.Atan2(numerator, denominator) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Returns the first integer greater than the number
        /// </summary>
        /// <param name="number">Number to round up.</param>
        /// <returns name="integer">First integer greater than the number.</returns>
        /// <search>ceiling,round</search>
        public static long Ceiling(double number)
        {
            return (long)CSMath.Ceiling(number);
        }

        /// <summary>
        ///     Finds the cosine of an angle.
        /// </summary>
        /// <param name="angle">Angle in degrees to take the cosine of.</param>
        /// <returns name="cosine">Cosine of the angle.</returns>
        /// <search>cos,cosine</search>
        public static double Cos(double angle)
        {
            return CSMath.Cos(angle * kDegreesToRadians);
        }

        /// <summary>
        ///     Finds the hyperbolic cosine of an angle.
        /// </summary>
        /// <param name="angle">Angle in degrees to take the </param>
        /// <returns name="cosh">Hyperbolic cosine of the angle.</returns>
        /// <search>hyperbolic,cosh</search>
        public static double Cosh(double angle)
        {
            return CSMath.Cosh(angle);
        }

        /// <summary>
        ///     Finds the remainder of dividend/divisor.
        /// </summary>
        /// <param name="dividend">The number to be divided.</param>
        /// <param name="divisor">The number to be divided by.</param>
        /// <returns name="remainder">The remainder of the division.</returns>
        /// <search>divrem,remainder</search>
        public static long DivRem(long dividend, long divisor)
        {
            long remainder = 0;
            CSMath.DivRem(dividend, divisor, out remainder);
            return remainder;
        }

        /// <summary>
        ///     Returns the exponential of the number, the constant e raised to the value number.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <returns name="e^number">The exponential of the number.</returns>
        /// <search>e,exp,exponential</search>
        public static double Exp(double number)
        {
            return CSMath.Exp(number);
        }

        /// <summary>
        ///     Returns the first integer smaller than the number.
        /// </summary>
        /// <param name="number">Number to round up.</param>
        /// <returns name="integer">First integer greater than the number.</returns>
        /// <search>ceiling,round</search>
        public static long Floor(double number)
        {
            return (long)CSMath.Floor(number);
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

        /// <summary>
        ///     Produce a random number in the range [0, 1).
        /// </summary>
        /// <returns name="number">Random number in the range [0, 1).</returns>
        /// <search>random</search>
        public static double Rand()
        {
            return mRandom.NextDouble();
        }
        
        /// <summary>
        ///     Produce a random number in the range [lowValue, highValue).
        /// </summary>
        /// <param name="value1">One end of the range for the random number.</param>
        /// <param name="value2">One end of the range for the random number.</param>
        /// <returns name="number">Random number in the range [lowValue, highValue).</returns>
        /// <search>random</search>
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

        /// <summary>
        ///     Finds the sine of an angle.
        /// </summary>
        /// <param name="angle">Angle in degrees to take the cosine of.</param>
        /// <returns name="sine">Sine of the angle.</returns>
        /// <search>sin,sine</search>
        public static double Sin(double angle)
        {
            return CSMath.Sin(angle * kDegreesToRadians);
        }

        /// <summary>
        ///     Finds the hyperbolic sine of an angle.
        /// </summary>
        /// <param name="angle">Angle.</param>
        /// <returns name="sinh">Hyperbolic sine of the angle.</returns>
        /// <search>hyperbolic,sinh</search>
        public static double Sinh(double angle)
        {
            return CSMath.Sinh(angle);
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
