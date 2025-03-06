using System;

namespace Dynamo.Logging
{
    /// <summary>
    /// Defines analytics session interface. This interface is defined for
    /// internal use and mocking the tests only.
    /// </summary>
    internal interface IAnalyticsSession : IDisposable
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
        /// Starts the session.
        /// The Session is closed when Dispose() is called.
        /// </summary>
        void Start();
    }
}
