using System;

namespace DynamoServices
{
    public class LogWarningMessageEventArgs : EventArgs
    {
        public string message { get; internal set; }

        public LogWarningMessageEventArgs(string msg)
        {
            message = msg;
        }
    }

    public delegate void LogWarningMessageEventHandler(LogWarningMessageEventArgs args);

    public static class LogWarningMessageEvents
    {
        /// <summary>
        /// event handler for logging info message
        /// </summary>
        internal static event LogWarningMessageEventHandler LogInfoMessage;

        /// <summary>
        /// event handler for logging warning message
        /// </summary>
        public static event LogWarningMessageEventHandler LogWarningMessage;

        /// <summary>
        /// Log node warning message
        /// </summary>
        /// <param name="message">warning message</param>
        public static void OnLogWarningMessage(string message)
        {
            if (LogWarningMessage != null)
                LogWarningMessage?.Invoke(new LogWarningMessageEventArgs(message));
        }

        /// <summary>
        /// Log node info message
        /// </summary>
        /// <param name="message">info message</param>
        public static void OnLogInfoMessage(string message)
        {
            LogInfoMessage?.Invoke(new LogWarningMessageEventArgs(message));
        }
    }

    internal static class LoadLibraryEvents
    {
        /// <summary>
        /// Raised when loading of a library fails. A failure message is passed as a parameter.
        /// </summary>
        internal static event Action<string, string> LoadLibraryFailure;

        internal static void OnLoadLibraryFailure(string failureMessage, string messageBoxTitle)
        {
            LoadLibraryFailure?.Invoke(failureMessage, messageBoxTitle);
        }
    }
    /// <summary>
    /// Used to log to the dynamo console from places where we do not have a reference
    /// to the Dynamo console.
    /// </summary>
    internal static class DynamoConsoleLogger
    {
        public static event Action<string> LogMessageToDynamoConsole;

        public static void OnLogMessageToDynamoConsole(string message)
        {
            LogMessageToDynamoConsole?.Invoke(message);
        }
    }
}
