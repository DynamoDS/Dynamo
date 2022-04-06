using System.Collections.Generic;
using System.Linq;

namespace FFITarget
{
    public class AtLevelTestClass
    {
        public int V { get; set; }

        public AtLevelTestClass()
        {
        }

        public AtLevelTestClass(int v)
        {
            V = v;
        }

        public static int Sum(List<int> values)
        {
            return values.Sum();
        }

        public int sum(List<int> values)
        {
            return Sum(values);
        }

        public static string SumAndConcat(List<int> values, string str)
        {
            return values.Sum().ToString() + str;
        }

        public string sumAndConcat(List<int> values, string str)
        {
            return SumAndConcat(values, str);
        }

        public static int Inc(int value)
        {
            return value + 1;
        }

        public int inc(int value)
        {
            return Inc(value);
        }
    }
}
