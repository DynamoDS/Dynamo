using System;

namespace Dynamo.Logging
{
    /// <summary>
    /// Categories for analytics tracking.
    /// </summary>
    public enum Categories
    {
        /// <summary>
        /// Events Category related to application lifecycle
        /// </summary>
        ApplicationLifecycle,

        /// <summary>
        /// Events Category related to product stability
        /// </summary>
        Stability,

        /// <summary>
        /// Events Category related to Node operations
        /// </summary>
        NodeOperations,

        /// <summary>
        /// Events Category related to Performance
        /// </summary>
        Performance,

        /// <summary>
        /// Events Category related to Dynamo commands
        /// </summary>
        Command,

        /// <summary>
        /// Events Category related to File operations
        /// </summary>
        FileOperation,

        /// <summary>
        /// Events Category related to Search UX
        /// </summary>
        SearchUX,

        /// <summary>
        /// Events Category related to user preferences
        /// </summary>
        Preferences,

        /// <summary>
        /// Events Category related to Dynamo upgrade
        /// </summary>
        Upgrade,

        /// <summary>
        /// Events Category related to DesignScript VM
        /// </summary>
        Engine,
    }

    /// <summary>
    /// Actions for analytics tracking.
    /// </summary>
    public enum Actions
    {
        /// <summary>
        /// Start of an event
        /// </summary>
        Start,

        /// <summary>
        /// End of an event
        /// </summary>
        End,

        /// <summary>
        /// Create Event, such as File create or Node create etc.
        /// </summary>
        Create,

        /// <summary>
        /// Delete Event, such as Node delete
        /// </summary>
        Delete,

        /// <summary>
        /// Move Event, such as Node move
        /// </summary>
        Move,

        /// <summary>
        /// Copy Event, such as Node copy
        /// </summary>
        Copy,

        /// <summary>
        /// Open Event, such as Open workspace
        /// </summary>
        Open,

        /// <summary>
        /// Close Event, such as Close workspace
        /// </summary>
        Close,

        /// <summary>
        /// Read Event, such as File read
        /// </summary>
        Read,

        /// <summary>
        /// Write Event, such as File write
        /// </summary>
        Write,

        /// <summary>
        /// Save Event, such as Save workspace
        /// </summary>
        Save,

        /// <summary>
        /// SaveAs Event, such as Save workspace as.
        /// </summary>
        SaveAs,

        /// <summary>
        /// New Event, such as New workspace
        /// </summary>
        New,

        /// <summary>
        /// Engine Failure event
        /// </summary>
        EngineFailure,

        /// <summary>
        /// Search Filter Button Clicked event
        /// </summary>
        FilterButtonClicked,

        /// <summary>
        /// Unresolved Node found event
        /// </summary>
        Unresolved,

        /// <summary>
        /// Update Downloaded event
        /// </summary>
        Downloaded,

        /// <summary>
        /// Update Installed event
        /// </summary>
        Installed,
    }

    /// <summary>
    /// Implements analytics and logging functions. This interface is defined 
    /// for internal use only to implement analytics functions and mock the tests.
    /// </summary>
    public interface IAnalyticsClient
    {
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
        void Start();

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
        /// Tracks a preference setting and its value.
        /// </summary>
        /// <param name="name">Name of the preference</param>
        /// <param name="stringValue">Preference value as string</param>
        /// <param name="metricValue">Metric value of the preference</param>
        void TrackPreference(string name, string stringValue, int? metricValue);

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
}
