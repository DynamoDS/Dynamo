using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodes
{
    public class SanityCheck
    {
        public static double ANumber()
        {
            return 42.0;
        }
    }

    namespace insanity
    {
        public class SanityCheck
    {
        public static double ANumber()
        {
            return -999;
        }
    }


    }
}
