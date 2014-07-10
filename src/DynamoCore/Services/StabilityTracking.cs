using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
}
