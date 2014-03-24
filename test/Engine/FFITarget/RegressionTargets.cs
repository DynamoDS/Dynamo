using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class RegressionTargets
    {
        public static double AverageIList(IList<double> numbers)
        {
            return numbers.Average();
        }

        public static double AverageList(List<double> numbers)
        {
            return numbers.Average();
        }

    }
}
