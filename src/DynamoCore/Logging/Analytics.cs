using System;
using Dynamo.Models;

namespace Dynamo.Logging
{
    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    public class Analytics
    {
        protected static IAnalyticsClient client = new DynamoAnalyticsClient();
        protected static bool enabled = true;

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        /// <param name="model">DynamoModel</param>
        /// <param name="enable">Whether to enable analytics and usage reporting.</param>
        internal static void Start(DynamoModel model, bool enable)
        {
            Analytics.enabled = enable;
            if (enable) client.Start(model);
        }

        /// <summary>
        /// Shuts down the client. Application life cycle end is tracked.
        /// </summary>
        internal static void ShutDown()
        {
            if (enabled) client.ShutDown();

            IDisposable disposable = client as IDisposable;
            if (disposable != null) disposable.Dispose();
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
            if (enabled) client.TrackEvent(action, category, description, value);
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
            if(enabled) client.TrackTimedEvent(category, variable, time, description);
        }

        /// <summary>
        /// Tracks screen view, such as Node view, Geometry view, Custom workspace etc.
        /// </summary>
        /// <param name="viewName">Name of the screen</param>
        public static void TrackScreenView(string viewName)
        {
            if(enabled) client.TrackScreenView(viewName);
        }

        /// <summary>
        /// Tracks an exception. If the exception is fatal, its recorded as crash.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="isFatal">If it's fatal</param>
        public static void TrackException(Exception ex, bool isFatal)
        {
            if(enabled) client.TrackException(ex, isFatal);
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
            if (!enabled) return DynamoAnalyticsClient.Disposable;

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
        public static IDisposable CreateCommandEvent(string name, string description = "", int? value = null)
        {
            if (!enabled) return DynamoAnalyticsClient.Disposable;

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
        public static IDisposable CreateFileOperationEvent(string filepath, Actions operation, int size, string description="")
        {
            if (!enabled) return DynamoAnalyticsClient.Disposable;

            return client.CreateFileOperationEvent(filepath, operation, size, description);
        }

        /// <summary>
        /// Logs usage data
        /// </summary>
        /// <param name="tag">Usage tag</param>
        /// <param name="data">Usage data</param>
        public static void LogPiiInfo(string tag, string data)
        {
            if (enabled) client.LogPiiInfo(tag, data);
        }
    }
}
