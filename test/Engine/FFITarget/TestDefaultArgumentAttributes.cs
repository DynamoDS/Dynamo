using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class ArgumentClass
    {
        public static float GetFloat()
        {
            return 2.5F;
        }    
    }

    public class TestDefaultArgumentAttributes
    {
        public static double GetCircleArea([DefaultArgument("ArgumentClass.GetFloat()")]double radius)
        {
            return radius * radius * Math.PI;
        }
    }
}
