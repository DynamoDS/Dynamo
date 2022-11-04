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
        public static event LogWarningMessageEventHandler LogWarningMessage;
        public static void OnLogWarningMessage(string message)
        {
            if (LogWarningMessage != null)
                LogWarningMessage(new LogWarningMessageEventArgs(message));
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
