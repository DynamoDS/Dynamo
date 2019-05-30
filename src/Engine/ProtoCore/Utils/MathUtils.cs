using System;

namespace ProtoCore.Utils
{
    public static class MathUtils
    {
        public static double Tolerance = 1e-5;

        public static bool Equals(double lhs, double rhs)
        {
            if (double.IsPositiveInfinity(lhs) && double.IsPositiveInfinity(rhs))
                return true;

            if (double.IsNegativeInfinity(lhs) && double.IsNegativeInfinity(rhs))
                return true;

            return Math.Abs(lhs - rhs) < Tolerance;
        }
    }
}
