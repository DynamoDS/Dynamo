using System;

namespace ProtoCore.Utils
{
    public static class MathUtils
    {
        public static double Tolerance = 1e-5;

        [Obsolete("This method is no longer used. Remove in Dynamo 3.0")]
        public static bool IsLessThan(double lhs, double rhs)
        {
            return (lhs<rhs) && !Equals(lhs, rhs);
        }

        [Obsolete("This method is no longer used. Remove in Dynamo 3.0")]
        public static bool IsGreaterThan(double lhs, double rhs)
        {
            return (lhs > rhs) && !Equals(lhs, rhs);
        }

        [Obsolete("This method is no longer used. Remove in Dynamo 3.0")]
        public static bool IsGreaterThanOrEquals(double lhs, double rhs)
        {
            return (lhs > rhs) || Equals(lhs, rhs);
        }

        [Obsolete("This method is no longer used. Remove in Dynamo 3.0")]
        public static bool IsLessThanOrEquals(double lhs, double rhs)
        {
            return (lhs < rhs) || Equals(lhs, rhs);
        }

        public static bool Equals(double lhs, double rhs, double tolerance)
        {
            if (double.IsPositiveInfinity(lhs) && double.IsPositiveInfinity(rhs))
                return true;

            if (double.IsNegativeInfinity(lhs) && double.IsNegativeInfinity(rhs))
                return true;

            return Math.Abs(lhs - rhs) < tolerance;
        }

        public static bool Equals(double lhs, double rhs)
        {
            return Equals(lhs, rhs, Tolerance);
        }

        public static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal
                    || value is System.Numerics.BigInteger;
        }
    }
}
