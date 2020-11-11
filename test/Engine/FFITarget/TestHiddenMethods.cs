using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class BaseTestHiddenMethods
    {
        public static int SomeVariable = 50;
        public BaseTestHiddenMethods()
        {
        }

        public static string SomeStaticMethod()
        {
            return "Base";
        }
        public static string SomeStaticMethod(int[] x)
        {
            return "Base";
        }
        public static string SomeStaticMethod(int[] x, int[] y)
        {
            return "Base";
        }
        public static string SomeMethod()
        {
            return "Base";
        }
    }

    public class DerivedTestHiddenMethods : BaseTestHiddenMethods
    {
        public static new int SomeVariable = 200;
        public DerivedTestHiddenMethods()
        {
        }
        public static new string SomeStaticMethod()
        {
            return "Derived";
        }
        public static new string SomeStaticMethod(int[] x)
        {
            return "Derived";
        }
        public static new string SomeStaticMethod(int[] x, int[] y)
        {
            return "Derived";
        }
        public static new string SomeMethod()
        {
            return "Derived";
        }
    }
}
