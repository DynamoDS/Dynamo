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
        ///     Generates a random double in the range of [0, 1).
        /// </summary>
        /// <param name="seed">Seed value for the random number generator.</param>
        /// <returns name="number">Random number between 0 and 1.</returns>
        /// <search>random,seed</search>
        public static double Random(int? seed = null)
        {
            var r = seed == null ? mRandom : new Random((int)seed);
            return r.NextDouble();
        }

        /// <summary>
        ///     Produce a random number in the range [lower_number, higher_number).
        /// </summary>
        /// <param name="value1">One end of the range for the random number.</param>
        /// <param name="value2">One end of the range for the random number.</param>
        /// <returns name="number">Random number in the range [lowValue, highValue).</returns>
        /// <search>random,numberrange</search>
        public static double Random(double value1, double value2)
        {
            double result = Min(value1, value2) + Abs(value2 - value1) * mRandom.NextDouble();
            return result;
        }

        /// <summary>
        ///     Produces a list containing the given amount of random doubles
        ///     in the range of [0, 1).
        /// </summary>
        /// <param name="amount">Amount of random numbers the result list will contain.</param>
        /// <returns name="number">List of random numbers between 0 and 1.</returns>
        /// <search>random,listcontains</search>
        public static IList RandomList(int amount)
        {
            var result = new ArrayList();

            foreach (var x in Enumerable.Range(0, amount).Select(_ => mRandom.NextDouble()))
                result.Add(x);

            return result;
        }

        /// <summary>
        ///     Pi Constant Multiplied by 2
        /// </summary>
        /// <returns name="2pi">2 times PI.</returns>
        /// <search>2pi,2*pi,twopi,two*pi</search>
        public static double PiTimes2
        {
            get
            {
                return CSMath.PI*2;
            }
        }

        /// <summary>
        ///     Averages a list of numbers.
        /// </summary>
        /// <param name="numbers">List of numbers to be averaged.</param>
        /// <returns name="average">Average of the list of numbers.</returns>
        /// <search>avg,mean</search>
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
        /// <search>remap range</search>
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
        /// <returns name="pi">double. The constant Pi.</returns>
        /// <search>3.141592653589793</search>
        public static double PI { get { return 3.141592653589793; } }

        /// <summary>
        ///     The mathematical constant e, 2.71828...
        /// </summary>
        /// <returns name="e">double. The constant e.</returns>
        /// <search>exp,2.718281828459045</search>
        public static double E { get { return 2.718281828459045; } }

        /// <summary>
        ///     The golden ratio, (1 + sqrt(5))/2 = 1.61803...
        /// </summary>
        /// <returns name="phi">double. The golden ratio.</returns>
        /// <search>golden,ratio,divine,phi,tau,1.61803398875</search>
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
        /// <param name="number">A number.</param>
        /// <returns name="absoluteValue">Absolute value of the number.</returns>
        /// <search>absolute value,magnitude</search>
        public static double Abs(double number)
        {
            return CSMath.Abs(number);
        }

        /// <summary>
        ///     Finds the absolute value of a number.
        /// </summary>
        /// <param name="integer">A number.</param>
        /// <returns name="absoluteValue">Absolute value of the number.</returns>
        /// <search>absolute value,magnitude</search>
        public static long Abs(long integer)
        {
            return CSMath.Abs(integer);
        }

        /// <summary>
        ///     Finds the inverse cosine, the angle whose cosine is the given ratio.
        /// </summary>
        /// <param name="ratio">The cosine of the angle, a number in the range [-1, 1].</param>
        /// <returns name="angle">The angle whose cosine is the input ratio.</returns>
        /// <search>acosine,arccosine</search>
        public static double Acos(double ratio)
        {
            return CSMath.Acos(ratio) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Finds the inverse sine, the angle whose sine is the given ratio.
        /// </summary>
        /// <param name="ratio">The sine of the angle, a number in the range [-1, 1].</param>
        /// <returns name="angle">The angle whose sine is the input ratio.</returns>
        /// <search>asine,arcsin</search>
        public static double Asin(double ratio)
        {
            return CSMath.Asin(ratio) * kRadiansToDegrees;
        }

        /// <summary>
        ///     Finds the inverse tangent, the angle whose tangent is the given ratio.
        /// </summary>
        /// <param name="ratio">The tangent of the angle.</param>
        /// <returns name="angle">The angle whose tangent is the input ratio.</returns>
        /// <search>atangent,arctangent</search>
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
        /// <search>atangent,arctangent</search>
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
        /// <returns name="cos">Cosine of the angle.</returns>
        /// <search>cosine</search>
        public static double Cos(double angle)
        {
            return CSMath.Cos(angle * kDegreesToRadians);
        }

        /// <summary>
        ///     Finds the hyperbolic cosine of an angle (radians).
        /// </summary>
        /// <param name="angle">An angle in radians.</param>
        /// <returns name="cosh">Hyperbolic cosine of the angle.</returns>
        /// <search>hyperbolic cosine</search>
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
        /// <search>remainder</search>
        public static long DivRem(long dividend, long divisor)
        {
            long remainder;
            CSMath.DivRem(dividend, divisor, out remainder);
            return remainder;
        }

        /// <summary>
        ///     Returns the exponential of the number, the constant e raised to the value number.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <returns name="e^number">The exponential of the number.</returns>
        /// <search>exponential</search>
        public static double Exp(double number)
        {
            return CSMath.Exp(number);
        }

        /// <summary>
        ///     Returns the first integer smaller than the number.
        /// </summary>
        /// <param name="number">Number to round up.</param>
        /// <returns name="integer">First integer smaller than the number.</returns>
        /// <search>round</search>
        public static long Floor(double number)
        {
            return (long)CSMath.Floor(number);
        }
        
        [IsVisibleInDynamoLibrary(false)]
        public static double IEEERemainder(double value1, double value2)
        {
            return CSMath.IEEERemainder(value1, value2);
        }
        
        /// <summary>
        ///     Finds the natural logarithm of a number in the range (0, ∞).
        /// </summary>
        /// <param name="number">Number greater than 0.</param>
        /// <returns name="log">Natural log of the number.</returns>
        /// <search>natural,logarithm,ln</search>
        public static double Log(double number)
        {
            return CSMath.Log(number);
        }
        
        /// <summary>
        ///     Finds the logarithm of a number with the specified base.
        /// </summary>
        /// <param name="number">Number greater than 0.</param>
        /// <param name="logBase">Base of the logarithm in the range [0,1),(1, ∞).</param>
        /// <returns name="log">Logarithm of the number.</returns>
        /// <search>logarithm,ld,lg</search>
        public static double Log(double number, double logBase)
        {
            return CSMath.Log(number, logBase);
        }
        
        /// <summary>
        ///     Finds the base-10 logarithm of a number.
        /// </summary>
        /// <param name="number">Number greater than 0.</param>
        /// <returns name="log">Logarithm of the number.</returns>
        /// <search>logarithm</search>
        public static double Log10(double number)
        {
            return CSMath.Log10(number);
        }

        /// <summary>
        ///     Returns the greater of two numbers.
        /// </summary>
        /// <param name="value1">Number to compare.</param>
        /// <param name="value2">Number to compare.</param>
        /// <returns name="max">Greater of the two numbers.</returns>
        /// <search>maximum,greater,larger</search>
        public static double Max(double value1, double value2)
        {
            return CSMath.Max(value1, value2);
        }

        /// <summary>
        ///     Returns the greater of two numbers.
        /// </summary>
        /// <param name="int1">Number to compare.</param>
        /// <param name="int2">Number to compare.</param>
        /// <returns name="max">Greater of the two numbers.</returns>
        /// <search>maximum,greater,larger</search>
        public static long Max(long int1, long int2)
        {
            return CSMath.Max(int1, int2);
        }

        /// <summary>
        ///     Returns the lesser of two numbers.
        /// </summary>
        /// <param name="value1">Number to compare.</param>
        /// <param name="value2">Number to compare.</param>
        /// <returns name="min">Smaler of the two numbers.</returns>
        /// <search>minimum,lesser,smaller</search>
        public static double Min(double value1, double value2)
        {
            return CSMath.Min(value1, value2);
        }

        /// <summary>
        ///     Returns the lesser of two numbers.
        /// </summary>
        /// <param name="int1">Number to compare.</param>
        /// <param name="int2">Number to compare.</param>
        /// <returns name="min">Smaler of the two numbers.</returns>
        /// <search>minimum,lesser,smaller</search>
        public static long Min(long int1, long int2)
        {
            return CSMath.Min(int1, int2);
        }

        /// <summary>
        ///     Raises a number to the specified power.
        /// </summary>
        /// <param name="number">Number to be raised to a power.</param>
        /// <param name="power">Power to raise the number to.</param>
        /// <returns name="result">Number raised to the power.</returns>
        /// <search>^,power,raise,exponent</search>
        public static double Pow(double number, double power)
        {
            return CSMath.Pow(number, power);
        }

        /// <summary>
        ///     Produce a random number in the range [0, 1).
        /// </summary>
        /// <returns name="number">Random number in the range [0, 1).</returns>
        /// <search>random,numberrange</search>
        //[IsVisibleInDynamoLibrary(false)] //Keeping for compatibility, Random() supercedes this --SJE
        public static double Rand()
        {
            return mRandom.NextDouble();
        }

        /// <summary>
        /// Rounds a number to the closest integral value.
        /// Note that this method returns a double-precision floating-point number instead of an integral type.
        /// </summary>
        /// <param name="number">Number to round.</param>
        /// <returns name="number">Integral value closes to the number.</returns>
        public static double Round(double number)
        {
            return CSMath.Round(number);
        }

        /// <summary>
        /// Rounds a number to a specified number of fractional digits. 
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="digits">Number of fractional digits in the return value.</param>
        /// <returns name="number">The number nearest to value that contains a number of fractional digits equal to digits.</returns>
        public static double Round(double number, int digits)
        {
            return CSMath.Round(number, digits);
        }

        /// <summary>
        ///     Returns the sign of the number: -1, 0, or 1.
        /// </summary>
        /// <param name="number">A number.</param>
        /// <returns name="sign">The sign of the number: -1, 0, or 1.</returns>
        public static long Sign(double number)
        {
            return CSMath.Sign(number);
        }

        /// <summary>
        ///     Returns the sign of the number: -1, 0, or 1.
        /// </summary>
        /// <param name="integer">A number.</param>
        /// <returns name="sign">The sign of the number: -1, 0, or 1.</returns>
        public static long Sign(long integer)
        {
            return CSMath.Sign(integer);
        }

        /// <summary>
        ///     Finds the sine of an angle.
        /// </summary>
        /// <param name="angle">Angle in degrees to take the cosine of.</param>
        /// <returns name="sin">Sine of the angle.</returns>
        /// <search>sine</search>
        public static double Sin(double angle)
        {
            return CSMath.Sin(angle * kDegreesToRadians);
        }

        /// <summary>
        ///     Finds the hyperbolic sine of an angle (radians).
        /// </summary>
        /// <param name="angle">An angle in radians.</param>
        /// <returns name="sinh">Hyperbolic sine of the angle.</returns>
        /// <search>hyperbolic</search>
        public static double Sinh(double angle)
        {
            return CSMath.Sinh(angle);
        }

        /// <summary>
        ///     Finds the positive square root of a number in the range [0, ∞).
        /// </summary>
        /// <param name="number">A number in the range [0, ∞).</param>
        /// <returns name="sqrt">Positive square root of the number.</returns>
        /// <search>square,root,radical</search>
        public static double Sqrt(double number)
        {
            return CSMath.Sqrt(number);
        }

        /// <summary>
        ///     Finds the tangent of an angle.
        /// </summary>
        /// <param name="angle">Angle in degrees to take the tangent of.</param>
        /// <returns name="tan">Tangent of the angle.</returns>
        /// <search>tangent</search>
        public static double Tan(double angle)
        {
            if (!(Equals(CSMath.IEEERemainder(angle, 180), 0.0) || Equals(CSMath.IEEERemainder(angle, 180), 180.0))
                && (Equals(CSMath.IEEERemainder(angle, 90), 0.0) || Equals(CSMath.IEEERemainder(angle, 90), 90.0)))
                return Double.NaN;
            return CSMath.Tan(angle * kDegreesToRadians);
        }

        /// <summary>
        ///     Finds the hyperbolic tangent of an angle (radians).
        /// </summary>
        /// <param name="angle">An angle in radians.</param>
        /// <returns name="tanh">Hyperbolic tangent of the angle.</returns>
        /// <search>hyperbolic,tanh</search>
        public static double Tanh(double angle)
        {
            return CSMath.Tanh(angle);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static double Truncate(double value)
        {
            return CSMath.Truncate(value);
        }


        /// <summary>
        ///      Find the sum of a series of numbers
        /// </summary>
        /// <param name="values">The numbers to sum</param>
        /// <returns name="sum">The sum of the values</returns>
        /// <search>mass addition,massadd</search>
        public static double Sum(IEnumerable<double> values)
        {
            return values.Sum();
        }

        /// <summary>
        ///     Finds the factorial result of a positive integer.
        /// </summary>
        /// <param name="number">A positive integer.</param>
        /// <returns name="number!">The factorial result of the integer.</returns>
        /// <search>!</search>
        public static long Factorial(long number)
        {
            if (number < 0)
            {
                return -1;
            }
            return (number > 1) ? number * Factorial(number - 1) : 1;
        }

        private static readonly Random mRandom = new Random();
    }
}
