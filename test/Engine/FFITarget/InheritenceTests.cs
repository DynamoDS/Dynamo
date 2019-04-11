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

        public static int Baz()
        {
            return 234;
        }

        public virtual int Foo()
        {
            return 99;
        }
    }

    public class HidesMethodFromClassA: ClassA
    {
        public int Bar()
        {
            
            return 3; 
        }

        public static int Baz()
        {
            return 23;
        }

        public override int Foo()
        {
            return base.Foo() + 1;
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
