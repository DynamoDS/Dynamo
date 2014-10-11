using System;

namespace Dynamo.Interfaces
{
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
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        public static ILogMessage Warning(string message, WarningLevel severity)
        {
            return new WarningLogMessage(severity, message);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ILogMessage Error(string message)
        {
            return new ErrorLogMessage(message);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static ILogMessage Error(Exception exception)
        {
            return new ExceptionLogMessage(exception);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ILogMessage Info(string message)
        {
            return new StandardLogMessage(message);
        }

        public static void Log(this ILogger logger, ILogMessage message)
        {
            message.Log(logger);
        }
    }

    /// <summary>
    ///     An object that can log messages.
    /// </summary>
    public interface ILogSource
    {
        event Action<ILogMessage> MessageLogged;
    }

    /// <summary>
    ///     An object that can log messages.
    /// </summary>
    public class LogSourceBase : ILogSource
    {
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        protected void Log(Exception ex)
        {
            Log(LogMessage.Error(ex));
        }

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }

        private class DispatchedLogger : ILogger
        {
            private readonly LogSourceBase source;

            public DispatchedLogger(LogSourceBase source)
            {
                this.source = source;
            }

            public void Log(string message)
            {
                source.Log(message);
            }

            public void Log(string tag, string message)
            {
                Log(string.Format("{0}:{1}", tag, message));
            }

            public void LogError(string error)
            {
               source.Log(error, WarningLevel.Error);
            }

            public void LogWarning(string warning, WarningLevel level)
            {
                source.Log(warning, level);
            }

            public void Log(Exception e)
            {
                source.Log(e);
            }
        }

        public ILogger AsLogger()
        {
            return new DispatchedLogger(this);
        }
    }
}
