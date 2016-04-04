using System;
using Dynamo.Interfaces;

namespace Dynamo.Logging
{
    /// <summary>
    ///     Utility methods for ILogSource.
    /// </summary>
    internal static class LogSource
    {
        public static ILogger AsLogger(Action<ILogMessage> eventInvoker)
        {
            return new DispatchedLogger(eventInvoker);
        }

        /// <summary>
        ///     Class used to convert a LogSourceBase into an ILogger, by dispatching
        ///     the ILogger methods to the MessageLogged event of ILogSource.
        /// </summary>
        private class DispatchedLogger : ILogger
        {
            private readonly Action<ILogMessage> eventInvoker;

            public DispatchedLogger(Action<ILogMessage> eventInvoker)
            {
                this.eventInvoker = eventInvoker;
            }

            public void Log(string message)
            {
                eventInvoker(LogMessage.Info(message));
            }

            public void Log(string tag, string message)
            {
                Log(string.Format("{0}:{1}", tag, message));
            }

            public void LogError(string error)
            {
                eventInvoker(LogMessage.Error(error));
            }

            public void LogWarning(string warning, WarningLevel level)
            {
                eventInvoker(LogMessage.Warning(warning, level));
            }

            public void Log(Exception e)
            {
                eventInvoker(LogMessage.Error(e));
            }
        }
    }

    /// <summary>
    ///     An object that can log messages.
    /// </summary>
    public class LogSourceBase : ILogSource
    {
        /// <summary>
        ///     Emits LogMessages.
        /// </summary>
        public event Action<ILogMessage> MessageLogged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        protected void Log(Exception ex)
        {
            Log(LogMessage.Error(ex));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="severity"></param>
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

        /// <summary>
        ///     Creates an ILogger out of this LogSourceBase; logging to the ILogger
        ///     will send messages out of the LogMessage event.
        /// </summary>
        /// <returns></returns>
        public ILogger AsLogger()
        {
            return LogSource.AsLogger(msg => Log(msg));
        }
    }
}
