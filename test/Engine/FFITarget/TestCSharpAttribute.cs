using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FooClassAttribute : Attribute
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
    }
}
