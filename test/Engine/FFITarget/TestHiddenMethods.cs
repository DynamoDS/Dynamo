using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class BaseTestHiddenMethods
    {
        public static int SomeVariable = 100;
        public BaseTestHiddenMethods()
        {
        }

        public static int SomeStaticMethod()
        {
            return 100;
        }
        public static int SomeStaticMethod(int[] x)
        {
            return 100;
        }
        public static int SomeStaticMethod(int[] x, int[] y)
        {
            return 100;
        }
        public static int SomeMethod()
        {
            return 100;
        }
    }

    public class DerivedTestHiddenMethods : BaseTestHiddenMethods
    {
        public static new int SomeVariable = 200;
        public DerivedTestHiddenMethods()
        {
        }
        public static new int SomeStaticMethod()
        {
            return 200;
        }
        public static new int SomeStaticMethod(int[] x)
        {
            return 200;
        }
        public static new int SomeStaticMethod(int[] x, int[] y)
        {
            return 200;
        }
        public static new int SomeMethod()
        {
            return 200;
        }
    }
}
