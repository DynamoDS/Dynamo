using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DummyClassA
    {
    }

    public class DummyClassB
    {
    }

    /// <summary>
    /// Test target for overloaded methods
    /// </summary>
    public class OverloadTarget
    {
        public static int StaticOverload(int a)
        {
            return 0;
        }

        public static int StaticOverload(ClassFunctionality a)
        {
            return 1;
        }

        public static int StaticOverload(DummyPoint a)
        {
            return 2;
        }

        public static int DifferentPrimitiveType(bool isTrue)
        {
            return 1;
        }

        public static int DifferentPrimitiveType(int number)
        {
            return 2;
        }

        public static int IEnumerableOfDifferentObjectType(System.Collections.Generic.IEnumerable<DummyClassA> a)
        {
            return 3;
        }

        public static int IEnumerableOfDifferentObjectType(System.Collections.Generic.IEnumerable<DummyClassB> b)
        {
            return 4;
        }
    }
}
