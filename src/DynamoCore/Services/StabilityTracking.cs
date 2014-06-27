using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dynamo.Services
{
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

        public StabilityTracking()
        {
            globalTime = new Stopwatch();
            timeSinceLastCrash = new Stopwatch();

            globalTime.Start();
            timeSinceLastCrash.Start();
        }


        public void NotifyCrash()
        {
            AnalyticsIntegration.LogTimedEvent(
                "Stability", "TimeBetweenFailure", timeSinceLastCrash.Elapsed);
            timeSinceLastCrash.Restart();



        }
    }
}
