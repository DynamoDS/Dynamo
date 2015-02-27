using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DummyMath
    {
        public static double Sum(IEnumerable<double> values)
        {
            return values.Sum();
        }
    }
}
