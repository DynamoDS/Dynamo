using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSCore
{
    /// <summary>
    /// Methods for performing Mathematical operations.
    /// </summary>
    public class Math
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
        public static double Average(IEnumerable<double> numbers)
        {
            return numbers.Average();
        }

        /// <summary>
        ///     Adjusts the range of a list of numbers while preserving the
        ///     distribution ratio.
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        public static IList RemapRange(IList numbers, double newMin = 0, double newMax = 1)
        {
            var nums = numbers.Cast<double>().ToList();
            var oldMax = nums.Max();
            var oldMin = nums.Min();
            var oldRange = oldMax - oldMin;
            var newRange = newMax - newMin;
            return nums.Select(oldValue => ((oldValue - oldMin)*newRange)/oldRange + newMin).ToList();
        }
    }
}
