using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Experimental
{
    public static class _RandTemp
    {
        private static Random random = new Random();

        public static double Next(double min, double max)
        {
            return random.NextDouble()*(max - min) + min;
        }


    }
}
