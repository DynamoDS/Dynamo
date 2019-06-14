using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Exceptions;
using Dynamo.Interfaces;

namespace Dynamo.Logging
{
    /// <summary>
    /// Specifies the level for log messages. A log message could be a console or file or warning.
    /// </summary>
    public enum LogLevel{Console, File, Warning}

    /// <summary>
    /// Specifies the warning level for log messages.
    /// </summary>
    public enum WarningLevel{Mild, Moderate, Error}

    /// <summary>
    /// This is a delegate used by Log events.
    /// </summary>
    /// <param name="args">Log event arguments.</param>
    public delegate void LogEventHandler(LogEventArgs args);

    /// <summary>
    /// Represents Event arguments that are passed to log event handler.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// The message to be logged.
        /// </summary>
        public string Message { get; set; }
    
        /// <summary>
        /// The log level at which to log the message.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Creates LogEventArgs based on log message.
        /// </summary>
        /// <param name="message">String message.</param>
        /// <param name="level">Level, where message can be logged.</param>
        public LogEventArgs(string message, LogLevel level)
        {
            Message = message;
            Level = level;
        }

        /// <summary>
        /// Creates LogEventArgs based on exception.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <param name="level">Level, where exception message and stack trace can be logged.</param>
        public LogEventArgs(Exception e, LogLevel level)
        {
            Message = e.Message + "\n" + e.StackTrace;
            Level = level;
        }
    }

    /// <summary>
    /// This class contains methods and properties used for logging in Dynamo,
    /// </summary>
    public class DynamoLogger: NotificationObject, ILogger, IDisposable
    {
        private readonly Object guardMutex = new Object();

        private readonly DebugSettings debugSettings;
        private string _logPath;
        private string _warning;
        private WarningLevel _warningLevel;
        private bool _isDisposed;

        private TextWriter FileWriter { get; set; }
        private StringBuilder ConsoleWriter { get; set; }

        /// <summary>
        /// event that is raised when a notification is logged
        /// </summary>
        public event Action<NotificationMessage> NotificationLogged;

        /// <summary>
        /// Returns warning level for log messages.
        /// </summary>
        public WarningLevel WarningLevel
        {
            get { return _warningLevel; }
            set
            {
                lock (this.guardMutex)
                {
                    _warningLevel = value;
                    RaisePropertyChanged("WarningLevel");
                }
            }
        }

        /// <summary>
        /// Returns warning message text
        /// </summary>
        public string Warning
        {
            get { return _warning; }
            set
            {
                lock (this.guardMutex)
                {

                    _warning = value;
                    RaisePropertyChanged("Warning");
                }
            }
        }

        /// <summary>
        /// Returns full path to log file
        /// </summary>
        public string LogPath 
        {
            get { return _logPath; }
        }

        /// <summary>
        /// Contains all message which have been logged
        /// </summary>
        public string LogText
        {
            get
            {
                lock (this.guardMutex)
                {
                    if (ConsoleWriter != null)
                        return ConsoleWriter.ToString();
                    return "";
                }
            }
        }

        private List<NotificationMessage> notifications;
        /// <summary>
        /// Notifications logged during startup such as library load failures
        /// that need to be displayed to user.
        /// </summary>
        public IEnumerable<NotificationMessage> StartupNotifications
        {
            get { return notifications; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DynamoLogger"/> class
        /// with specified debug settings and directory where to write logs
        /// </summary>
        /// <param name="debugSettings">Debug settings</param>
        /// <param name="logDirectory">Directory path where log file will be written</param>
        public DynamoLogger(DebugSettings debugSettings, string logDirectory)
        {
            lock (guardMutex)
            {
                this.debugSettings = debugSettings;
                _isDisposed = false;

                WarningLevel = WarningLevel.Mild;
                Warning = "";

                notifications = new List<NotificationMessage>();

                StartLogging(logDirectory);
            }
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="level">The level.</param>
        internal void Log(string message, LogLevel level)
        {
            Log(message, level, true);
        }

        /// <summary>
        /// Log the message to the the correct path
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="reportModification"></param>
        private void Log(string message, LogLevel level, bool reportModification)
        {
            lock (this.guardMutex)
            {
                //Don't overwhelm the logging system
                if (debugSettings.VerboseLogging)
                    Analytics.LogPiiInfo("LogMessage-" + level.ToString(), message);

                switch (level)
                {
                        //write to the console
                    case LogLevel.Console:
                        if (ConsoleWriter != null)
                        {
                            try
                            {
                                ConsoleWriter.AppendLine(string.Format("{0}", message));
                                FileWriter.WriteLine(string.Format("{0} : {1}", DateTime.UtcNow.ToString("u"), message));
                                FileWriter.Flush();
                                RaisePropertyChanged("ConsoleWriter");
                            }
                            catch
                            {
                                // likely caught if the writer is closed
                            }
                        }
                        break;

                        //write to the file
                    case LogLevel.File:
                        if (FileWriter != null)
                        {
                            try
                            {
                                FileWriter.WriteLine(string.Format("{0} : {1}", DateTime.UtcNow.ToString("u"), message));
                                FileWriter.Flush();
                            }
                            catch
                            {
                                // likely caught if the writer is closed
                            }
                        }
                        break;
                }

                if (reportModification)
                {
                    RaisePropertyChanged("LogText");
                }
            }
        }

        /// <summary>
        /// Logs a tagged notification to the console, also
        /// fires an event that a notification was logged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="title"></param>
        /// <param name="shortMessage"></param>
        /// <param name="detailedMessage"></param>
        public void LogNotification(string sender,string title, string shortMessage, string detailedMessage)
        {
            var notificationMessage = string.Format("{0}:{3} {1}: {3} {2}", title, shortMessage, detailedMessage,Environment.NewLine);
            Log("notification",notificationMessage );
            if (NotificationLogged != null)
            {
                NotificationLogged(new NotificationMessage(sender, shortMessage, detailedMessage, title));
            }
            else
            {
                notifications.Add(new NotificationMessage(sender, shortMessage, detailedMessage, title));
            }
        }


        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="level">The level.</param>
        public void LogWarning(string message, WarningLevel level)
        {
            Warning = message;
            WarningLevel = level;

            Log(message, LogLevel.Console);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="error">The error.</param>
        public void LogError(string error)
        {
            Warning = error;
            WarningLevel = WarningLevel.Error;
            Log(error);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="error">The error.</param>
        public void LogError(string tag, string error)
        {
            Warning = error;
            WarningLevel = WarningLevel.Error;
            Log(tag, error);
        }

        /// <summary>
        /// Log an information message
        /// </summary>
        /// <param name="tag">Tag of the message to log</param>
        /// <param name="info">Message to log</param>
        public void LogInfo(string tag, string info)
        {
            Log(tag, LogLevel.File);
        }

        /// <summary>
        /// Resets the warning.
        /// </summary>
        internal void ResetWarning()
        {
            lock (this.guardMutex)
            {
                Warning = "";
                WarningLevel = WarningLevel.Mild;
            }
        }

        /// <summary>
        /// Clear any notifications after displaying them.
        /// </summary>
        public void ClearStartupNotifications()
        {
            notifications.Clear();
        }

        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Log(string message)
        {
            Log(message, LogLevel.Console);
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="e">Exception to log</param>
        public void Log(Exception e)
        {
            var le = e as LibraryLoadFailedException;
            if (le != null)
            {
                LogNotification(le.Source, le.ToString(), le.Message, le.Reason);
            }
            Log(e.GetType() + ":", LogLevel.Console);
            Log(e.Message, LogLevel.Console);
            Log(e.StackTrace, LogLevel.Console);
        }

        /// <summary>
        /// Log some data with an associated tag
        /// </summary>
        /// <param name="tag">Tag of the message to log</param>
        /// <param name="data">Message to log</param>
        public void Log(string tag, string data)
        {
            Log(string.Format("{0}:{1}", tag, data));
        }

        internal void ClearLog()
        {
            lock (this.guardMutex)
            {
                ConsoleWriter.Clear();
                RaisePropertyChanged("LogText");
            }
        }

        /// <summary>
        /// Begin logging.
        /// </summary>
        private void StartLogging(string logDirectory)
        {
            lock (this.guardMutex)
            {
                // We use a guid to uniquely identify the log name. This disambiguates log files
                // so that parallel testing which needs to access the log files can be done, and
                // so that services like Cloud Watch can match the dynamoLog_* pattern.
                _logPath = Path.Combine(logDirectory, string.Format("dynamoLog_{0}.txt", Guid.NewGuid()));

                var date = DateTime.UtcNow.ToString("u");
                FileWriter = new StreamWriter(_logPath);
                FileWriter.WriteLine("Dynamo log started " + date);

                ConsoleWriter = new StringBuilder();
                ConsoleWriter.AppendLine("Dynamo log started " + date);
            }

        }

        /// <summary>
        /// Dispose of the logger and finish logging.
        /// </summary>
        internal void Dispose(bool isDisposed)
        {
            //Don't lock here as it risks deadlocking the collector

            if (isDisposed)
            {
                return;
            }

            if (FileWriter != null)
            {
                try
                {
                    FileWriter.Flush();
                    Log(Properties.Resources.GoodByeLog, LogLevel.Console, false);
                    FileWriter.Close();
                }
                catch
                {
                }
            }

            if (ConsoleWriter != null)
                ConsoleWriter = null;
        }

        /// <summary>
        /// Disposes the logger and finishes logging.
        /// </summary>
        public void Dispose()
        {
            Dispose(_isDisposed);

            _isDisposed = true;
        }
    }
}
