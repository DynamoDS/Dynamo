using System;
using CSMath = System.Math;

namespace Math
{
    //the math class is legacy, the new math class is in DSCoreNode
#if false
    public static class Math
    {
        internal static readonly double kRadiansToDegrees = 180.0 / PI;
        internal static readonly double kDegreesToRadians = PI / 180.0;

        public static double PI { get { return 3.141592653589793; } }

        public static double E { get { return 2.718281828459045; } }

        public static double GoldenRatio { get { return 1.61803398875; } }

        public static double RadiansToDegrees(double value)
        {
            return value * kRadiansToDegrees;
        }

        public static double DegreesToRadians(double value)
        {
            return value * kDegreesToRadians;
        }

        public static double Abs(double d_value)
        {
            return CSMath.Abs(d_value);
        }
        
        public static long Abs(long n_value)
        {
            return CSMath.Abs(n_value);
        }
        
        public static double Acos(double value)
        {
            return CSMath.Acos(value) * kRadiansToDegrees;
        }

        public static double Asin(double value)
        {
            return CSMath.Asin(value) * kRadiansToDegrees;
        }
        
        public static double Atan(double value)
        {
            return CSMath.Atan(value) * kRadiansToDegrees;
        }
        
        public static double Atan2(double value1, double value2)
        {
            return CSMath.Atan2(value1, value2) * kRadiansToDegrees;
        }
        
        public static long Ceiling(double value)
        {
            return (long)CSMath.Ceiling(value);
        }
        public static double Cos(double value)
        {
            return CSMath.Cos(value * kDegreesToRadians);
        }
        public static double Cosh(double value)
        {
            return CSMath.Cosh(value);
        }
        
        public static long DivRem(long value1, long value2)
        {
            long remainder = 0;
            CSMath.DivRem(value1, value2, out remainder);
            return remainder;
        }

        public static double Exp(double value)
        {
            return CSMath.Exp(value);
        }
       
        public static long Floor(double value)
        {
            return (long)CSMath.Floor(value);
        }
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

        public static double Rand()
        {
            return mRandom.NextDouble();
        }
        public static double Rand(double value1, double value2)
        {
            double result = Min(value1, value2) + Abs(value2 - value1) * mRandom.NextDouble();
            return result;
        }
        public static double Round(double value)
        {
            return CSMath.Round(value);
        }
        
        public static double Round(double value, MidpointRounding mode)
        {
            return CSMath.Round(value, mode);
        }

        public static double Round(double value, int digits)
        {
            return CSMath.Round(value, digits);
        }
        
        public static double Round(double value, int digits, MidpointRounding mode)
        {
            return CSMath.Round(value, digits, mode);
        }
        
        public static long Sign(double d_value)
        {
            return CSMath.Sign(d_value);
        }
        
        public static long Sign(long n_value)
        {
            return CSMath.Sign(n_value);
        }

        public static double Sin(double value)
        {
            return CSMath.Sin(value * kDegreesToRadians);
        }
        public static double Sinh(double value)
        {
            return CSMath.Sinh(value);
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
#endif
}
