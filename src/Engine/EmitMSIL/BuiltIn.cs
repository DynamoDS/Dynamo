using System;
using System.Collections.Generic;
using System.Reflection;

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

        public static dynamic Add(dynamic a, dynamic b)
        {
            return a + b;
        }

        public static dynamic Sub(dynamic a, dynamic b)
        {
            return a - b;
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
