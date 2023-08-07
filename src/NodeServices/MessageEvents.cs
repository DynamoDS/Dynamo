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

    /// <summary>
    /// EventArgs for LogInfoMessageEventHandler
    /// </summary>
    public class LogInfoMessageEventArgs : EventArgs
    {
        public string message { get; internal set; }

        public LogInfoMessageEventArgs(string msg)
        {
            message = msg;
        }
    }

    public delegate void LogWarningMessageEventHandler(LogWarningMessageEventArgs args);

    public static class LogWarningMessageEvents
    {
        public static event LogWarningMessageEventHandler LogWarningMessage;
        public static void OnLogWarningMessage(string message)
        {
            if (LogWarningMessage != null)
                LogWarningMessage(new LogWarningMessageEventArgs(message));
        }
    }

    public delegate void LogInfoMessageEventHandler(LogInfoMessageEventArgs args);

    public static class LogInfoMessageEvents
    {
        public static event LogInfoMessageEventHandler LogInfoMessage;
        public static void OnLogInfoMessage(string message)
        {
            if (LogInfoMessage != null)
                LogInfoMessage(new LogInfoMessageEventArgs(message));
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
}
