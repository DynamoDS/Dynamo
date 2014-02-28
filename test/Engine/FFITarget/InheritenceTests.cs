using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class BaseTest
    {
        public int x;
    }

    public class DerivedTest : BaseTest
    {
        public int y;
    }

    public class InheritenceDriver
    

{
        public static BaseTest Gen()
        {
            DerivedTest dt = new DerivedTest();
            //dt.x = 1;
            dt.y = 2;

            return dt;
        }
}
}
