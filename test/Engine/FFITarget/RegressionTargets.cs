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

    public class BaseTestMAGN3178
    {
        public int dummy = 6;


    }

    public class DerivedTestMAGN3178 : BaseTestMAGN3178
    {
        public void Foo()
        {
        }
    }


}
