using System;

namespace ProtoCore.Utils
{
    public static class MathUtils
    {
        public static double Tolerance = 1e-5;

        public static bool IsLessThan(double lhs, double rhs)
        {
            if (lhs.CompareTo(rhs) < 0) return true;

            return false;
        }

        public static bool IsGreaterThan(double lhs, double rhs)
        {
            if (lhs.CompareTo(rhs) > 0) return true;

            return false;
        }

        public static bool IsGreaterThanOrEquals(double lhs, double rhs)
        {
            return IsGreaterThan(lhs, rhs) || Equals(lhs, rhs); 
        }

        public static bool IsLessThanOrEquals(double lhs, double rhs)
        {
            return IsLessThan(lhs, rhs) || Equals(lhs, rhs);
        }

        public static bool Equals(double lhs, double rhs)
        {
            if (Double.IsInfinity(lhs) && Double.IsInfinity(rhs))
                return true;

            return Math.Abs(lhs - rhs) < Tolerance;
        }
    }
}
