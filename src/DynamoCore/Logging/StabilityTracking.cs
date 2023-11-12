using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Dynamo.Logging
{
    internal class StabilityUtils
    {
        private static bool isLastShutdownClean;

        /// <summary>
        /// To check whether the last shutdown is clean(no crash)
        /// </summary>
        public static bool IsLastShutdownClean
        {
            get
            {
                return isLastShutdownClean;
            }
            internal set
            {
                isLastShutdownClean = value;
            }
        }
    }

    /// <summary>
    /// The stability cookie class groups together the usage around the registry key that is written
    /// to record whether the last shutdown was performed cleanly or not.
    /// 
    /// If the Write
    /// </summary>
    internal static class StabilityCookie
    {
        // The name of the key must include a valid root.
        private const string REG_KEY = "HKEY_CURRENT_USER\\Software\\DynamoStability";
        private const string SHUTDOWN_TYPE_NAME = "CleanShutdown";
        private const string CLEAN_SHUTDOWN_VALUE = "clean";
        private const string CRASHING_SHUTDOWN_VALUE = "crashing";
        private const string ASSUMING_CRASHING_SHUTDOWN_VALUE = "assumingCrashing";

        
        /// <summary>
        /// Record that the shutdown was clean
        /// </summary>
        public static void WriteCleanShutdown()
        {
            Registry.SetValue(REG_KEY, SHUTDOWN_TYPE_NAME, CLEAN_SHUTDOWN_VALUE);
        }

        /// <summary>
        /// Record that the shutdown was as a result of a crash
        /// </summary>
        public static void WriteCrashingShutdown()
        {
            Registry.SetValue(REG_KEY, SHUTDOWN_TYPE_NAME, CRASHING_SHUTDOWN_VALUE);
        }

        /// <summary>
        /// Start up and report the status of the last shutdown
        /// </summary>
        public static void Startup()
        {
            StabilityUtils.IsLastShutdownClean = IsLastShutdownClean();

            // If we don't do anything to explicitly set the type of shutdown assume we hard-crashed
            // this is pesimistic
            Registry.SetValue(REG_KEY, SHUTDOWN_TYPE_NAME, ASSUMING_CRASHING_SHUTDOWN_VALUE);

        }

        /// <summary>
        /// To check whether the last shutdown is clean(no crash)
        /// </summary>
        private static bool IsLastShutdownClean()
        {
            var ret = Registry.GetValue(REG_KEY, SHUTDOWN_TYPE_NAME, CRASHING_SHUTDOWN_VALUE) as string;
            if (null != ret && string.CompareOrdinal(ret, CRASHING_SHUTDOWN_VALUE) == 0)
                return false;

            return true;
        }
    }
}
