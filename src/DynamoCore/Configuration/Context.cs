using System;

namespace Dynamo.Configuration
{
    public enum DynamoAppContext
    {
        DynamoStandalone,
        DynamoRevit,
        DynamoStudio,
        DynamoVasari
    }

    public static class Context
    {        
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
