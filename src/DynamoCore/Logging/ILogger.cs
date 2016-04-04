using System;

namespace Dynamo.Logging
{
    /// <summary>
    ///     Consumes messages to be used for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        void Log(string tag, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        void LogError(string error);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="warning"></param>
        /// <param name="level"></param>
        void LogWarning(string warning, WarningLevel level);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void Log(Exception e);
    }

    /// <summary>
    ///     A message that can be logged with an ILogger.
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
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
