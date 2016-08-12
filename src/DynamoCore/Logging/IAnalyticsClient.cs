using System;
using Dynamo.Models;

namespace Dynamo.Logging
{
    /// <summary>
    /// Categories for analytics tracking.
    /// </summary>
    public enum Categories
    {
        ApplicationLifecycle,
        Stability,
        NodeOperations,
        Performance,
        Command,
        FileOperation,
        SearchUX,
    }

    /// <summary>
    /// Actions for analytics tracking.
    /// </summary>
    public enum Actions
    {
        Start,
        End,
        Create,
        Delete,
        Move,
        Copy,
        Open,
        Close,
        Read,
        Write,
        Save,
        SaveAs,
        New,
        EngineFailure,
        FilterButtonClicked,
        Unresolved,
    }

    /// <summary>
    /// Implements analytics and logging functions.
    /// </summary>
    public interface IAnalyticsClient
    {
        /// <summary>
        /// Gets session object for this client
        /// </summary>
        IAnalyticsSession Session { get; }

        /// <summary>
        /// Checks if analytics reporting is ON.
        /// </summary>
        bool ReportingAnalytics { get; }

        /// <summary>
        /// Cheks if detailed usage reporting is ON.
        /// </summary>
        bool ReportingUsage { get; }

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        /// <param name="model"></param>
        void Start(DynamoModel model);

        /// <summary>
        /// Shuts down the client. Application life cycle end is tracked.
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Tracks an arbitrary event.
        /// </summary>
        /// <param name="action">Action performed</param>
        /// <param name="category">Event category</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        void TrackEvent(Actions action, Categories category, string description, int? value);

        /// <summary>
        /// Tracks a timed event, when it has completed.
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="variable">Timed variable name</param>
        /// <param name="time">Time taken by the event</param>
        /// <param name="description">Event description</param>
        void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description);

        /// <summary>
        /// Tracks screen view, such as Node view, Geometry view, Custom workspace etc.
        /// </summary>
        /// <param name="viewName">Name of the screen</param>
        void TrackScreenView(string viewName);

        /// <summary>
        /// Tracks an exception. If the exception is fatal, its recorded as crash.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="isFatal">If it's fatal</param>
        void TrackException(Exception ex, bool isFatal);

        /// <summary>
        /// Creates a new timed event with start state and tracks its start.
        /// Disposing the returnd event will record the event completion.
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="variable">Timed varaible name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        IDisposable CreateTimedEvent(Categories category, string variable, string description, int? value);

        /// <summary>
        /// Creates a new command event of the given name. Start of the 
        /// command is tracked. When the event is disposed, it's completion is tracked.
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        IDisposable CreateCommandEvent(string name, string description, int? value);

        /// <summary>
        /// Creates a new file operation event and tracks the start of the event.
        /// Disposing the returned event will record its completion.
        /// </summary>
        /// <param name="filepath">File path</param>
        /// <param name="operation">File operation</param>
        /// <param name="size">Size parameter</param>
        /// <param name="description">Event description</param>
        /// <returns>Event as IDisposable</returns>
        IDisposable TrackFileOperationEvent(string filepath, Actions operation, int size, string description);

        /// <summary>
        /// Logs usage data
        /// </summary>
        /// <param name="tag">Usage tag</param>
        /// <param name="data">Usage data</param>
        void LogPiiInfo(string tag, string data);
    }

    /// <summary>
    /// Defines analytics session interface.
    /// </summary>
    public interface IAnalyticsSession : IDisposable
    {
        /// <summary>
        /// Get unique user id.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Gets unique session id.
        /// </summary>
        string SessionId { get; }
        
        /// <summary>
        /// Starts the session for the given DynamoModel. 
        /// The Session is closed when Dispose() is called.
        /// </summary>
        /// <param name="model">DynamoModel</param>
        void Start(DynamoModel model);

        /// <summary>
        /// Returns a logger to record usage.
        /// </summary>
        ILogger Logger { get; }
    }
}
