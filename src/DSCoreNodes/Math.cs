using System;
using System.Collections;
using System.Linq;

namespace DSCoreNodes
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
    }
}
