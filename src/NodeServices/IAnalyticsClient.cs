using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamo.Logging
{
    /// <summary>
    /// Categories for analytics tracking.
    /// </summary>
    public enum Categories
    {
        /// XXXOperations usually means actions from Dynamo users
        /// v.s. XXX usually means actions from the Dynamo component itself

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
        /// Events Category related to the Package Manager
        /// </summary>
        PackageManager,

        /// <summary>
        /// Events Category related to Dynamo upgrade
        /// </summary>
        Upgrade,

        /// <summary>
        /// Events Category related to DesignScript VM
        /// </summary>
        Engine,

        /// <summary>
        /// Events Category related to Node Auto-Complete
        /// </summary>
        NodeAutoCompleteOperations,

        /// <summary>
        /// Events Category related to In-Canvas search
        /// </summary>
        InCanvasSearchOperations,
        
        /// <summary>
        /// Events Category related to Python operations
        /// </summary>
        PythonOperations,

        /// <summary>
        /// Events Category related to Extensions operations
        /// </summary>
        ExtensionOperations,

        /// <summary>
        /// Events Category related to View Extensions operations
        /// </summary>
        ViewExtensionOperations,

        /// <summary>
        /// Events Category related to package manager operations
        /// </summary>
        PackageManagerOperations,

        /// <summary>
        /// Events Category related to Note operations
        /// </summary>
        NoteOperations,

        /// <summary>
        /// Events Category related to Workspace References Operations
        /// </summary>
        WorkspaceReferencesOperations,

        /// <summary>
        /// Events Category related to saved Workspace References
        /// </summary>
        WorkspaceReferences,

        /// <summary>
        /// Events Category related to Groups
        /// </summary>
        GroupOperations,

        /// <summary>
        /// Events Category related to Node context menu
        /// </summary>
        NodeContextMenuOperations,

        /// <summary>
        /// Events Category related to connectors
        /// </summary>
        ConnectorOperations,

        /// <summary>
        /// Events Category related to group styles
        /// </summary>
        GroupStyleOperations,

        /// <summary>
        /// Events Category related to guided tours
        /// </summary>
        GuidedTourOperations,

        /// <summary>
        /// Events Category related to the splash screen
        /// </summary>
        SplashScreenOperations,

        /// <summary>
        /// Events Category related to DynamoMLDataPipeline
        /// </summary>
        DynamoMLDataPipelineOperations,

        /// <summary>
        /// Events Category related to DynamoHome
        /// </summary>
        DynamoHomeOperations
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
        /// Move Event, such as Node move, dialog move
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
        /// Close Event, such as Close workspace, Close Python Editor
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
        /// Save Event, such as Save workspace and save Python code
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
        /// Filter event, e.g. when package filter is active
        /// </summary>
        Filter,

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
        
        /// <summary>
        /// Select event, such as node auto-complete suggestion selection or in-canvas search selection
        /// </summary>
        Select,

        /// <summary>
        /// Migration event, such as Python migration or DYN migration
        /// </summary>
        Migration,

        /// <summary>
        /// Switch event, such as Python engine switch, dropdown node switch
        /// </summary>
        Switch,

        /// <summary>
        /// Run event, such as Python node run clicked, Graph run Clicked, generic node run during graph execution
        /// </summary>
        Run,

        /// <summary>
        /// Load event, such as extensions loaded, package loaded
        /// </summary>
        Load,

        /// <summary>
        /// Dock event, such as docking view extension
        /// </summary>
        Dock,

        /// <summary>
        /// Undock event, such as undocking view extension
        /// </summary>
        Undock,

        /// <summary>
        /// Rate event, such as rating guided tour
        /// </summary>
        Rate,

        /// <summary>
        /// Pin event, such as pinning a note to a node
        /// </summary>
        Pin,

        /// <summary>
        /// Unpin event, such as unpinning a note from a node
        /// </summary>
        Unpin,

        /// <summary>
        /// Download new event, such as downloading a new package in package reference section by user
        /// </summary>
        DownloadNew,

        /// <summary>
        /// KeepOld event, e.g. choosing to keep the old package in package reference section by user
        /// </summary>
        KeepOld,

        /// <summary>
        /// PackageReferences event, when a package reference is saved in a workspace
        /// </summary>
        PackageReferences,

        /// <summary>
        /// KeepOldPackage event, when a local reference is saved in a workspace
        /// </summary>
        LocalReferences,

        /// <summary>
        /// KeepOldPackage event, when an external reference is saved in a workspace
        /// </summary>
        ExternalReferences,

        /// <summary>
        /// Ungroup event, when an group is Ungrouped
        /// </summary>
        Ungroup,

        /// <summary>
        /// Expand event, when an group is Expanded
        /// </summary>
        Expanded,

        /// <summary>
        /// Collapse event, when an group is Collapsed
        /// </summary>
        Collapsed,

        /// <summary>
        /// AddedTo event, when a node is added to the group
        /// </summary>
        AddedTo,

        /// <summary>
        /// RemovedFrom event, when a node is removed from the group
        /// </summary>
        RemovedFrom,

        /// <summary>
        /// Preview event, when a node is Previewed
        /// </summary>
        Preview,

        /// <summary>
        /// Freeze event, when a node is Freezed
        /// </summary>
        Freeze,

        /// <summary>
        /// Rename event, when a node is Renamed
        /// </summary>
        Rename,

        /// <summary>
        /// Show event, when user wants to toggle display.
        /// </summary>
        Show,

        /// <summary>
        /// Set event, when user wants to set a property.
        /// </summary>
        Set,

        /// <summary>
        /// Dismiss event, e.g. to dismiss node alerts.
        /// </summary>
        Dismiss,

        /// <summary>
        /// Undismiss event, to show dismissed alerts.
        /// </summary>
        Undismiss,

        /// <summary>
        /// Break event, e.g. when a connection is broken by user choice
        /// </summary>
        Break,

        /// <summary>
        /// Hide event, e.g when a connection is hidden by user choice
        /// </summary>
        Hide,

        /// <summary>
        /// When a package conflict is encountered which involves at least one built-in package.
        /// </summary>
        BuiltInPackageConflict,

        /// <summary>
        /// Sort event, when user wants to sort some information
        /// </summary>
        Sort,

        /// <summary>
        /// View event, when user wants to see some information
        /// </summary>
        View,

        /// <summary>
        /// Show event, when user wants to view Documentation.
        /// </summary>
        ViewDocumentation,

        /// <summary>
        /// When the in-depth node help documentation is un-available
        /// </summary>
        MissingDocumentation,

        /// <summary>
        /// Cancel operation, e.g. cancel adding a new group style 
        /// </summary>
        Cancel,

        /// <summary>
        /// Completed event, e.g. when a user completes a guided tour
        /// </summary>
        Completed,

        /// <summary>
        /// Next event, e.g. when a user clicks next on a guided tour
        /// </summary>
        Next,

        /// <summary>
        /// Previous event, e.g. when a user goes back on a guided tour
        /// </summary>
        Previous,

        /// <summary>
        /// TimeElapsed event, e.g. tracks the time elapsed since an event
        /// </summary>
        TimeElapsed,

        /// <summary>
        /// SignIn event, e.g. tracks the SignIn event
        /// </summary>
        SignIn,

        /// <summary>
        /// SignOut event, e.g. tracks the SignOut event
        /// </summary>
        SignOut,

        /// <summary>
        /// Import event, e.g. tracks the ImportSettings event
        /// </summary>
        Import,

        /// <summary>
        /// Export event, e.g. tracks the ExportSettings event
        /// </summary>
        Export,

        /// <summary>
        /// Timed event: tracks startup time
        /// </summary>
        Startup,

        /// <summary>
        /// Timed event: tracks view startup time
        /// </summary>
        ViewStartup,

        /// <summary>
        /// Timed event: tracks graph execution time
        /// </summary>
        UpdateGraphAsyncTask
    }

    public enum HeartBeatType
    {
        User,
        Machine
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
        /// This API is used to track user/machine's activity status.
        /// </summary>
        /// <param name="activityType">Value must be: machine or user. If no value is provided the API will default to user activity type.</param>
        /// <returns>0 if successful, otherwise returns an error code.</returns>
        void TrackActivityStatus(string activityType);

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
        /// Creates a new task timed event with start state and tracks its start.
        /// After task is compoleted, disposing the returnd event will record the event completion.
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="variable">Timed varaible name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        Task<IDisposable> CreateTaskTimedEvent(Categories category, string variable, string description, int? value);

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
        /// Creates a new task command event of the given name. Start of the 
        /// command is tracked. When the task is completed and the event is disposed, it's completion is tracked.
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="description">Event description</param>
        /// <param name="value">A metric value associated with the event</param>
        /// <param name="parameters">A dictionary of (string, object) associated with the event</param>
        /// <returns>Event as IDisposable</returns>
        Task<IDisposable> CreateTaskCommandEvent(string name, string description, int? value, IDictionary<string, object> parameters = null);

        /// <summary>
        /// Waits for the given task to end so that it can dispose the event and
        /// complete the tracking.
        /// </summary>
        void EndEventTask(Task<IDisposable> taskToEnd);

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
        /// Creates a new file operation task event and tracks the start of the event.
        /// After the task is completed, disposing the returned event will record its completion.
        /// </summary>
        /// <param name="filepath">File path</param>
        /// <param name="operation">File operation</param>
        /// <param name="size">Size parameter</param>
        /// <param name="description">Event description</param>
        /// <returns>Event as IDisposable</returns>
        Task<IDisposable> TrackTaskFileOperationEvent(string filepath, Actions operation, int size, string description);
    }
}
