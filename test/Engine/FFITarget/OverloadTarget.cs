using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
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




    }
}
