using System;

namespace Dynamo.Core
{
    public static class Context
    {
        // TODO: MAGN-7242
        public const string NONE = "None";
        public const string REVIT_2014 = "Revit 2014";
        public const string REVIT_2015 = "Revit 2015";
        public const string VASARI_2014 = "Vasari 2014";

        /// <summary>
        ///     Check if the operating system running Dynamo is 
        ///     a Unix-based one or not.  This will return true 
        ///     for both OS X and Linux.  
        /// 
        ///      https://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-windows-or-linux-without-using-a-con
        /// </summary>
        internal static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}
