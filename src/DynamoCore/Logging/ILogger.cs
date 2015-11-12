using System;

namespace Dynamo.Logging
{
    /// <summary>
    ///     Consumes messages to be used for logging.
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void Log(string tag, string message);
        void LogError(string error);
        void LogWarning(string warning, WarningLevel level);
        void Log(Exception e);
    }

    /// <summary>
    ///     A message that can be logged with an ILogger.
    /// </summary>
    public interface ILogMessage
    {
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
}
