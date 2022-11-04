using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    [AttributeUsage(AttributeTargets.Class)]
    class FooClassAttribute : Attribute
    {
        public FooClassAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BarClassAttribute : Attribute
    {
        public BarClassAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class FooMethodAttribute : Attribute
    {
        public FooMethodAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class BarMethodAttribute : Attribute
    {
        public BarMethodAttribute()
        {
        }
    }

    [FooClassAttribute()]
    [BarClassAttribute()]
    public class TestCSharpAttribute
    {
        [FooMethodAttribute()]
        [BarMethodAttribute()]
        public void Test()
        {

        }

        [Obsolete]
        public string Test2()
        {
            return "this node is obsolete.";
        }

        [return: ArbitraryDimensionArrayImport]
        public static IEnumerable<object> TestReturnAttribute()
        {
            return new object[] { 1.3, new double[] { 4.5, 7.8 } };
        }
    }
}
