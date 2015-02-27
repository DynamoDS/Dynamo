using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public abstract class BaseTest
    {
        public int X { get; set; }
    }

    public class DerivedTest : BaseTest
    {
        public int Y { get; set; }
    }

    public class InheritenceDriver
    

{
        public static BaseTest Gen()
        {
            DerivedTest dt = new DerivedTest();
            //dt.x = 1;
            dt.Y = 2;

            return dt;
        }
}
}
