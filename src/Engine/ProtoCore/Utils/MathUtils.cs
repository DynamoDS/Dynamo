using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Utils
{
    public static class MathUtils
    {
        public static double Tolerance = 1e-5; 

        public static bool IsLessThan(double lhs, double rhs)
        {
            return (lhs < rhs) && !Equals(lhs, rhs);
        }

        public static bool IsGreaterThan(double lhs, double rhs)
        {
            return (lhs > rhs) && !Equals(lhs, rhs);
        }

        public static bool IsGreaterThanOrEquals(double lhs, double rhs)
        {
            return (lhs > rhs) || Equals(lhs, rhs); 
        }

        public static bool IsLessThanOrEquals(double lhs, double rhs)
        {
            return (lhs < rhs) || Equals(lhs, rhs);
        }

        public static bool Equals(double lhs, double rhs)
        {
            return Math.Abs(lhs - rhs) < Tolerance;
        }
    }
}
