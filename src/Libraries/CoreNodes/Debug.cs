using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCore
{
    public class Debug
    {
        public static void WriteOut(System.Object x)
        {
            System.Diagnostics.Debug.WriteLine(x.ToString());

        }

    }
}
