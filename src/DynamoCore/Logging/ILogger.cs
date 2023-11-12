using System;

namespace Dynamo.Logging
{
    /// <summary>
    ///     Consumes messages to be used for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Logs data with an associated tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <param name="message">Message to be logged.</param>
        void Log(string tag, string message);

        /// <summary>
        /// Logs error.
        /// </summary>
        /// <param name="error">Error message.</param>
        void LogError(string error);

        /// <summary>
        /// Logs warning.
        /// </summary>
        /// <param name="warning">Warning message.</param>
        /// <param name="level">Warning level.</param>
        void LogWarning(string warning, WarningLevel level);

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="e">Exception.</param>
        void Log(Exception e);
    }

    /// <summary>
    ///     A message that can be logged with an ILogger.
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// Makes logger log message.
        /// </summary>
        /// <param name="logger">Logger.</param>
        void Log(ILogger logger);
    }

    /// <summary>
    ///     An object that emits log messages.
    /// </summary>
    public interface ILogSource
    {
        /// <summary>
        ///     Emits LogMessages.
        /// </summary>
        event Action<ILogMessage> MessageLogged;
    }
    internal interface INotificationSource
    {
        /// <summary>
        ///     Emits Notifications.
        /// </summary>
        event Action<NotificationMessage> NotificationLogged;
    }
}
