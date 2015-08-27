using System;

namespace Dynamo.Interfaces
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
    ///     Factory methods for creating log messages.
    /// </summary>
    public static class LogMessage
    {
        #region ILogMessage instances
        private class ExceptionLogMessage : ILogMessage
        {
            private readonly Exception exception;

            public ExceptionLogMessage(Exception exception)
            {
                this.exception = exception;
            }

            public void Log(ILogger logger)
            {
                logger.Log(exception);
            }
        }

        private class ErrorLogMessage : ILogMessage
        {
            private readonly string message;

            public ErrorLogMessage(string message)
            {
                this.message = message;
            }

            public void Log(ILogger logger)
            {
                logger.LogError(message);
            }
        }

        private class WarningLogMessage : ILogMessage
        {
            private readonly WarningLevel level;
            private readonly string message;

            public WarningLogMessage(WarningLevel level, string message)
            {
                this.level = level;
                this.message = message;
            }

            public void Log(ILogger logger)
            {
                logger.LogWarning(message, level);
            }
        }

        private class StandardLogMessage : ILogMessage
        {
            private readonly string message;

            public StandardLogMessage(string message)
            {
                this.message = message;
            }

            public void Log(ILogger logger)
            {
                logger.Log(message);
            }
        }
        #endregion

        /// <summary>
        ///     Creates a LogMessage representing a warning.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        public static ILogMessage Warning(string message, WarningLevel severity)
        {
            return new WarningLogMessage(severity, message);
        }

        /// <summary>
        ///     Creates a LogMessage representing an error.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ILogMessage Error(string message)
        {
            return new ErrorLogMessage(message);
        }

        /// <summary>
        ///     Creates a LogMessage representing an error.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static ILogMessage Error(Exception exception)
        {
            return new ExceptionLogMessage(exception);
        }

        /// <summary>
        ///     Creates a basic LogMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ILogMessage Info(string message)
        {
            return new StandardLogMessage(message);
        }

        /// <summary>
        ///     Logs a LogMessage.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        public static void Log(this ILogger logger, ILogMessage message)
        {
            message.Log(logger);
        }
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
