using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Dynamo.Nodes;

namespace FFITarget
{
    public class DefaultArguments
    {
        // Deprecated function for testing default argument
        //public static bool Foobar(int arg0, int arg1)
        //{
        //    return true;
        //}

        public static bool Foobar(int arg0, int arg1, bool arg2 = true)
        {
            return true;
        }

        public static bool Foobar(double arg0, double arg1)
        {
            return true;
        }

    }
}
