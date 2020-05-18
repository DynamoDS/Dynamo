using System;

namespace Dynamo.Logging
{
    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    public static class Analytics
    {
        /// <summary>
        /// A dummy IDisposable class
        /// </summary>
        class Dummy : IDisposable
        {
            public void Dispose() { }
        }

        private static IAnalyticsClient client = null;
        
        /// <summary>
        /// Starts analytics client
        /// </summary>
        /// <param name="client"></param>
        internal static void Start(IAnalyticsClient client)
        {
            Analytics.client = client;
            client.Start();
        }

        /// <summary>
        /// Shuts down the client. Application life cycle end is tracked.
        /// </summary>
        internal static void ShutDown()
        {
            if (client != null) client.ShutDown();

            IDisposable disposable = client as IDisposable;
            if (disposable != null) disposable.Dispose();
            client = null;
        }

        /// <summary>
        /// Checks if anlytics tracking is enabled or not. Required for testing.
        /// </summary>
        internal static bool IsEnabled
        {
            get { return client != null; }
        }

        /// <summary>
        /// Returns if any analytics reporting is ON (Google, ADP etc.)
        /// </summary>
        public static bool ReportingAnalytics { get { return client != null && client.ReportingAnalytics; } }

        /// <summary>
        /// Tracks application startup time
        /// </summary>
        /// <param name="productName">Dynamo product name</param>
        /// <param name="time">Elapsed time</param>
        /// <param name="description">Optional description</param>
        public static void TrackStartupTime(string productName, TimeSpan time, string description = "")
        {
            if (client != null)
            {
                var desc = string.IsNullOrEmpty(description)
                    ? productName : string.Format("{0}: {1}", productName, description);
                client.TrackTimedEvent(Categories.Performance, "Startup", time, desc);
            }
        }

        /// <summary>
        /// Tracks an arbitrary event.
        /// </summary>
        /// <param name="action">Action performed</param>
        /// <param name="category">Event category</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        public static void TrackEvent(Actions action, Categories category, string description = "", int? value = null)
        {
            if (client != null) client.TrackEvent(action, category, description, value);
        }

        /// <summary>
        /// Tracks user preference setting and its value.
        /// </summary>
        /// <param name="name">Name of the preference</param>
        /// <param name="stringValue">Preference value as string</param>
        /// <param name="metricValue">Metric value of the preference</param>
        public static void TrackPreference(string name, string stringValue, int? metricValue)
        {
            if (client != null) client.TrackPreference(name, stringValue, metricValue);
        }

        /// <summary>
        /// Tracks a timed event, when it has completed.
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="variable">Timed variable name</param>
        /// <param name="time">Time taken by the event</param>
        /// <param name="description">Event description</param>
        public static void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            if (client != null) client.TrackTimedEvent(category, variable, time, description);
        }

        /// <summary>
        /// Tracks screen view, such as Node view, Geometry view, Custom workspace etc.
        /// </summary>
        /// <param name="viewName">Name of the screen</param>
        public static void TrackScreenView(string viewName)
        {
            if (client != null) client.TrackScreenView(viewName);
        }

        /// <summary>
        /// Tracks an exception. If the exception is fatal, its recorded as crash.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="isFatal">If it's fatal</param>
        public static void TrackException(Exception ex, bool isFatal)
        {
            if (client != null) client.TrackException(ex, isFatal);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("Type: " + ex.GetType());
            sb.AppendLine("Fatal: " + isFatal);
            sb.AppendLine("Message: " + ex.Message);
            sb.AppendLine("StackTrace: " + ex.StackTrace);

            LogPiiInfo("Analytics.TrackException", sb.ToString());
        }

        /// <summary>
        /// Creates a new timed event with start state and tracks its start.
        /// Disposing the returnd event will record the event completion.
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="variable">Timed varaible name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        public static IDisposable CreateTimedEvent(Categories category, string variable, string description = "", int? value = null)
        {
            if (client == null) return new Dummy();

            return client.CreateTimedEvent(category, variable, description, value);
        }

        /// <summary>
        /// Creates a new command event of the given name. Start of the 
        /// command is tracked. When the event is disposed, it's completion is tracked.
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        public static IDisposable TrackCommandEvent(string name, string description = "", int? value = null)
        {
            if (client == null) return new Dummy();

            return client.CreateCommandEvent(name, description, value);
        }

        /// <summary>
        /// Creates a new file operation event and tracks the start of the event.
        /// Disposing the returned event will record its completion.
        /// </summary>
        /// <param name="filepath">File path</param>
        /// <param name="operation">File operation</param>
        /// <param name="size">Size parameter</param>
        /// <param name="description">Event description</param>
        /// <returns>Event as IDisposable</returns>
        public static IDisposable TrackFileOperationEvent(string filepath, Actions operation, int size, string description = "")
        {
            if (client == null) return new Dummy();

            return client.TrackFileOperationEvent(filepath, operation, size, description);
        }

        /// <summary>
        /// Logs usage data
        /// </summary>
        /// <param name="tag">Usage tag</param>
        /// <param name="data">Usage data</param>
        public static void LogPiiInfo(string tag, string data)
        {
            if (client != null) client.LogPiiInfo(tag, data);
        }
    }
}
