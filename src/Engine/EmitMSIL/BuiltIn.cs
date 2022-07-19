using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ProtoCore.Utils;

namespace EmitMSIL
{
    public class BuiltIn
    {
        private const string typeName = "EmitMSIL.BuiltIn";

        private static string GetBuiltInMethodName(string name)
        {
            if (name[0] == '%')
            {
                name = name.Remove(0, 1);
            }

            return name[0].ToString().ToUpper() + name.Substring(1);
        }

        private static bool ToBoolean(dynamic a)
        {
            if (a is bool ab)
            {
                return ab;
            }

            if (a is long al)
            {
                return al != 0;
            }

            if (a is null)
            {
                return false;
            }

            if (a is double ad)
            {
                return !Double.IsNaN(ad) && !ad.Equals(0.0);
            }

            if (a is Pointer)
            {
                return true;
            }

            if (a is string str)
            {
                return !string.IsNullOrEmpty(str);
            }

            if (a is char ac)
            {
                return ac != 0;
            }

            return false;
        }

        public static dynamic Add(dynamic a, dynamic b)
        {
            return a + b;
        }

        public static dynamic Sub(dynamic a, dynamic b)
        {
            return a - b;
        }

        public static dynamic Mul(dynamic a, dynamic b)
        {
            return a * b;
        }

        public static dynamic Div(dynamic a, dynamic b)
        {
            //division is always carried out as a double
            double lhs = a;
            double rhs = b;

            return lhs / rhs;
        }

        public static dynamic Mod(dynamic a, dynamic b)
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

        public static bool And(dynamic a, dynamic b)
        {
            bool lhs = ToBoolean(a);
            bool rhs = ToBoolean(b);

            return lhs && rhs;
        }

        public static bool Or(dynamic a, dynamic b)
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
        public static bool Eq(dynamic a, dynamic b)
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

        public static bool Nq(dynamic a, dynamic b)
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

        public static bool Gt(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a > b;
            }

            return false;
        }

        public static bool Lt(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a < b;
            }

            return false;
        }

        public static bool Ge(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a >= b;
            }

            return false;
        }

        public static bool Le(dynamic a, dynamic b)
        {
            if (a is double || b is double || a is long || b is long)
            {
                return a <= b;
            }

            return false;
        }

        public static bool IsBuiltIn(string name)
        {
            if (name.Length > 0)
            {
                return name[0] == '%';
            }
            return false;
        }
        public static IEnumerable<MethodBase> GetBuiltIn(string name)
        {
            var ret = new List<MethodBase>();
            var mi = Type.GetType(typeName)?.GetMethod(GetBuiltInMethodName(name));
            if (mi != null)
            {
                ret.Add(mi);
                return ret;
            }

            throw new MissingMethodException("No matching built in method found");
        }
    }
}
