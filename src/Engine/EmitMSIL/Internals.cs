using ProtoCore.DSASM;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EmitMSIL
{
    public class Internals
    {
        private static bool ToBoolean(dynamic a)
        {
            switch (a)
            {
                case bool ab:
                    return ab;

                case long al:
                    return al != 0;

                case null:
                    return false;

                case double ad:
                    return !Double.IsNaN(ad) && !ad.Equals(0.0);

                case string str:
                    return !string.IsNullOrEmpty(str);

                case char ac:
                    return ac != 0;
            }

            if (a is Pointer)
            {
                return true;
            }

            return false;
        }

        public static dynamic add(dynamic a, dynamic b)
        {
            return a + b;
        }

        public static dynamic sub(dynamic a, dynamic b)
        {
            return a - b;
        }

        public static dynamic mul(dynamic a, dynamic b)
        {
            return a * b;
        }

        public static dynamic div(dynamic a, dynamic b)
        {
            //division is always carried out as a double
            double lhs = a;
            double rhs = b;

            return lhs / rhs;
        }

        public static dynamic mod(dynamic a, dynamic b)
        {
            var intMod = a % b;

            // In order to follow the conventions of Java, Python, Scala and Google's calculator,
            // the returned modulo will follow the sign of the divisor (not the dividend). 

            if (intMod < 0 && b > 0 || intMod > 0 && b < 0)
            {
                intMod += b;
            }

            return intMod;
        }

        public static dynamic Neg(dynamic a)
        {
            return -a;
        }

        public static bool and(dynamic a, dynamic b)
        {
            bool lhs = ToBoolean(a);
            bool rhs = ToBoolean(b);

            return lhs && rhs;
        }

        public static bool or(dynamic a, dynamic b)
        {
            bool lhs = ToBoolean(a);
            bool rhs = ToBoolean(b);

            return lhs || rhs;
        }

        public static bool Not(dynamic a)
        {
            bool data = ToBoolean(a);

            return !data;
        }

        public static bool eq(dynamic a, dynamic b)
        {
            if (a is bool || b is bool)
            {
                bool ab = ToBoolean(a);
                bool bb = ToBoolean(b);
                return ab == bb;
            }

            if (a is double || b is double || a is long || b is long)
            {
                if (a is double || b is double)
                {
                    double ad = a;
                    double bd = b;
                    return MathUtils.Equals(ad, bd);
                }

                return a == b;
            }

            if (a is string astr && b is string bstr)
            {
                return String.Compare(astr, bstr, StringComparison.InvariantCulture) == 0;
            }

            return a.Equals(b);
        }

        public static bool nq(dynamic a, dynamic b)
        {
            if (a is bool || b is bool)
            {
                bool ab = ToBoolean(a);
                bool bb = ToBoolean(b);
                return ab != bb;
            }

            if (a is double || b is double || a is long || b is long)
            {
                if (a is double || b is double)
                {
                    double ad = a;
                    double bd = b;
                    return !MathUtils.Equals(ad, bd);
                }

                return a != b;
            }

            if (a is string astr && b is string bstr)
            {
                return String.Compare(astr, bstr, StringComparison.InvariantCulture) != 0;
            }

            return !a.Equals(b);
        }

        public static bool gt(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a > b;
            }

            return false;
        }

        public static bool lt(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a < b;
            }

            return false;
        }

        public static bool ge(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a >= b;
            }

            return false;
        }

        public static bool le(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a <= b;
            }

            return false;
        }

        public static IEnumerable<MethodBase> GetInternalMethod(string name)
        {
            string methodName = "";
            if (CoreUtils.TryGetOperator(name, out Operator op))
            {
                methodName = op.ToString();
            }
            else if (CoreUtils.TryGetUnaryOperator(name, out UnaryOperator un))
            {
                methodName = un.ToString();
            }

            if (methodName.Length > 0)
            {
                var ret = new List<MethodBase>();
                var mi = typeof(Internals)?.GetMethod(methodName);
                if (mi != null)
                {
                    ret.Add(mi);
                    return ret;
                }
            }

            throw new MissingMethodException("No matching operator method found");
        }
    }
}