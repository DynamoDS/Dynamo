using System;
using Dynamo.Models;

namespace Dynamo.Logging
{
    /// <summary>
    /// Defines analytics session interface. This interface is defined for
    /// internal use and mocking the tests only.
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
