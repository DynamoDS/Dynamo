using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using CSharpAnalytics;
using CSharpAnalytics.Protocols.Measurement;

namespace Dynamo.Services
{
    public class AnalyticsIntegration
    {
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";

        private  static MeasurementAnalyticsClient client;
 
        static AnalyticsIntegration()
        {

            CSharpAnalytics.MeasurementConfiguration mc = new MeasurementConfiguration(ANALYTICS_PROPERTY,
                "Dynamo", "0.0.X.X.TEST");
            AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);
            
            CSharpAnalytics.AutoMeasurement.Start(mc);


            client = AutoMeasurement.Client;

        }

        public static void LogTimedEvent(string category, string variable, TimeSpan time, string label = null)
        {
            client.TrackTimedEvent(category, variable, time, label);
        }

        public static void LogEvent(string action, string category, string label = null)
        {
            AutoMeasurement.Client.TrackEvent(action, category, label);
        }

        public static void LogScreen(string screenName)
        {
            AutoMeasurement.Client.TrackScreenView(screenName);
        }

        public static void LogException(string description)
        {
            AutoMeasurement.Client.TrackException(description);
        }

        public static void LogException(Exception e)
        {
            //Can only report the type of the exception not the stack trace because of PII concerns
            AutoMeasurement.Client.TrackException(e.GetType().ToString());
        }

        
        
    }
}
