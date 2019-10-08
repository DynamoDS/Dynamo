using System.Collections.Generic;
using System.Linq;

namespace FFITarget
{
    public class DummyMath
    {
        public static double Sum(IEnumerable<double> values)
        {
            return values.Sum();
        }

        public int a { get; set; }

        public static DummyMath ValueCtor(int _a)
        {
            DummyMath ret = new DummyMath()
            {
                a = _a
            };
            return ret;
        }

        public static double Sin(double x1)
        {
            return x1;
        }

        public int Mul(int num)
        {
            return num * a;
        }

        public double Mul(double num)
        {
            return num * a;
        }

        public int Mul(int num1, int num2, int num3)
        {
            return (num1 + num2 + num3) * a;
        }

        public int Div(int num1, int num2)
        {
            return (num1 + num2) / a;
        }

        public int Div(int num1, int num2, int num3)
        {
            return (num1 + num2 + num3) / a;
        }

        public int Div(int num1, int num2, int num3, int num4, int num5)
        {
            return (num1 + num2 + num3 + num4 + num5) / a;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }
}
