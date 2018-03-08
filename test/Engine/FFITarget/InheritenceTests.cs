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

    public class NonDerivedTest
    {
        public int Y { get; set; }
    }

    public interface InterfaceA
    {
        int Foo();
    }

    public class DerivedFromInterfaceA: InterfaceA
    {
        public int Foo()
        {
            return 1;
        }
    }
    public class NotDerivedFromInterfaceA
    {
        public int Foo()
        {
            return 2;
        }
    }

    public class ClassA
    {
        public int Bar()
        {
            return 0;
        }
    }

    public class HidesMethodFromClassA: ClassA
    {
        public int Bar()
        {
            return 3;
        }
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
