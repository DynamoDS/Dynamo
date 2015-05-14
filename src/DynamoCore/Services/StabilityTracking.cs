using System;
using System.Diagnostics;

using Microsoft.Win32;

namespace Dynamo.Services
{
    /// <summary>
    /// Stability state tracking utils library
    /// The primary use is for reporting MTBF to instrumentation
    /// </summary>
    public class StabilityTracking
    {
        private Stopwatch globalTime;
        private Stopwatch timeSinceLastCrash;
        private static StabilityTracking instance;
        private readonly static Object mutex = new object();

        public static StabilityTracking GetInstance()
        {
            lock (mutex)
            {
                if (instance == null)
                {
                    instance = new StabilityTracking();
                }    
            }

            return instance;
        }

        private StabilityTracking()
        {
            globalTime = new Stopwatch();
            timeSinceLastCrash = new Stopwatch();

            globalTime.Start();
            timeSinceLastCrash.Start();
        }

        /// <summary>
        /// Notify the stability tracker that a crash has occured
        /// </summary>
        public void NotifyCrash()
        {
            InstrumentationLogger.LogAnonymousTimedEvent(
                "Stability", "TimeBetweenFailure", timeSinceLastCrash.Elapsed);
            timeSinceLastCrash.Restart();
        }
    }

    public class StabilityUtils
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
        private const string UPTIME_NAME = "UptimeMS";

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
        /// Record that the system has been up for the the length of the time in the timespan
        /// </summary>
        /// <param name="timespan"></param>
        public static void WriteUptimeBeat(TimeSpan timespan)
        {
            Registry.SetValue(REG_KEY, UPTIME_NAME, ((long)timespan.TotalMilliseconds).ToString());
        }

        /// <summary>
        /// Start up and report the status of the last shutdown
        /// </summary>
        public static void Startup()
        {
            StabilityUtils.IsLastShutdownClean = IsLastShutdownClean();
            String cleanShutdownValue = Registry.GetValue(REG_KEY, SHUTDOWN_TYPE_NAME, null) as String;
            String uptimeValue = Registry.GetValue(REG_KEY, UPTIME_NAME, null) as String;

            bool isUptimeSpanValid = false;
            TimeSpan uptimeSpan = TimeSpan.MinValue;


            long uptimeMs;
            if (long.TryParse(uptimeValue, out uptimeMs))
            {
                uptimeSpan = TimeSpan.FromMilliseconds(uptimeMs);
                isUptimeSpanValid = true;
            }

            if (cleanShutdownValue == null || uptimeValue == null)
                InstrumentationLogger.LogAnonymousEvent("FirstTimeStartup", "Stability");
            else
            {
                switch (cleanShutdownValue)
                {
                    case CLEAN_SHUTDOWN_VALUE:
                        InstrumentationLogger.LogAnonymousEvent("Clean shutdown", "Stability");
                        if (isUptimeSpanValid)
                            InstrumentationLogger.LogAnonymousTimedEvent("Stability", "Clean Uptime", uptimeSpan);
                        break;

                    case CRASHING_SHUTDOWN_VALUE:
                        InstrumentationLogger.LogAnonymousEvent("Crashing shutdown", "Stability");
                        if (isUptimeSpanValid)
                            InstrumentationLogger.LogAnonymousTimedEvent("Stability", "Dirty Uptime", uptimeSpan);
                        break;

                    case ASSUMING_CRASHING_SHUTDOWN_VALUE:
                        //This is the case where we don't know what happened, so we're defaulting
                        InstrumentationLogger.LogAnonymousEvent("Assumed crashing shutdown", "Stability");
                        if (isUptimeSpanValid)
                            InstrumentationLogger.LogAnonymousTimedEvent("Stability", "Assumed Dirty Uptime", uptimeSpan);
                        break;

                    default:
                        //Something went wrong, fail out with 'unknown' data.
                        InstrumentationLogger.LogAnonymousEvent("Unknown shutdown", "Stability");
                        Debug.WriteLine("Unknown shutdown key value: " + cleanShutdownValue);
                        break;
                }
            }


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
