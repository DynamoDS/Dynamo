﻿using System;
using System.IO;
using System.Text;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public enum LogLevel{Console, File, Warning}
    public enum WarningLevel{Mild, Moderate, Error}

    public class DynamoLogger:NotificationObject, ILogger, IDisposable
    {
        private readonly Object guardMutex = new Object();


        const string DYNAMO_LOG_DIRECTORY = @"Autodesk\Dynamo\Logs\";

        //private static DynamoLogger instance;
        private string _logPath;
        private string _warning;
        private WarningLevel _warningLevel;
        private bool _isDisposed;

        private TextWriter FileWriter { get; set; }
        private StringBuilder ConsoleWriter { get; set; }

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

        public string LogPath 
        {
            get { return _logPath; }
        }

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

        /// <summary>
        /// The default constructor.
        /// </summary>
        public DynamoLogger()
        {
            lock (this.guardMutex)
            {
                _isDisposed = false;

                WarningLevel = WarningLevel.Mild;
                Warning = "";

                StartLogging();
            }
        }

        public void Log(string message, LogLevel level)
        {
            Log(message, level, true);
        }

        /// <summary>
        /// Log the message to the the correct path
        /// </summary>
        /// <param name="message"></param>
        private void Log(string message, LogLevel level, bool reportModification)
        {
            lock (this.guardMutex)
            {
                //Don't overwhelm the logging system
                if (dynSettings.Controller != null && !dynSettings.Controller.DebugSettings.VerboseLogging)
                    InstrumentationLogger.LogInfo("LogMessage-" + level.ToString(), message);

                switch (level)
                {
                        //write to the console
                    case LogLevel.Console:
                        if (ConsoleWriter != null)
                        {
                            try
                            {
                                ConsoleWriter.AppendLine(string.Format("{0}", message));
                                FileWriter.WriteLine(string.Format("{0} : {1}", DateTime.Now, message));
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
                                FileWriter.WriteLine(string.Format("{0} : {1}", DateTime.Now, message));
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

        public void LogWarning(string message, WarningLevel level)
        {
            Warning = message;
            WarningLevel = level;

            Log(message, LogLevel.Console);
        }

        public void LogError(string error)
        {
            Warning = error;
            WarningLevel = WarningLevel.Error;
            Log(error);
        }

        public void LogError(string tag, string error)
        {
            Warning = error;
            WarningLevel = WarningLevel.Error;
            Log(tag, error);
        }

        public void LogInfo(string tag, string info)
        {
            Log(tag, LogLevel.File);
        }

        public void ResetWarning()
        {
            lock (this.guardMutex)
            {
                Warning = "";
                WarningLevel = WarningLevel.Mild;
            }
        }

        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            Log(message, LogLevel.Console);
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="e"></param>
        public void Log(Exception e)
        {
            Log(e.GetType() + ":", LogLevel.Console);
            Log(e.Message, LogLevel.Console);
            Log(e.StackTrace, LogLevel.Console);
        }

        /// <summary>
        /// Log some data with an associated tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public void Log(string tag, string data)
        {
            Log(string.Format("{0}:{1}", tag, data));
        }

        public void ClearLog()
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
        private void StartLogging()
        {
            lock (this.guardMutex)
            {
                //create log files in a directory 
                //with the executing assembly
                string log_dir = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                log_dir = Path.Combine(log_dir, DYNAMO_LOG_DIRECTORY);

                if (!Directory.Exists(log_dir))
                {
                    Directory.CreateDirectory(log_dir);
                }

                _logPath = Path.Combine(log_dir, string.Format("dynamoLog_{0}.txt", Guid.NewGuid().ToString()));

                FileWriter = new StreamWriter(_logPath);
                FileWriter.WriteLine("Dynamo log started " + DateTime.Now.ToString());

                ConsoleWriter = new StringBuilder();
                ConsoleWriter.AppendLine("Dynamo log started " + DateTime.Now.ToString());
            }

        }

        /// <summary>
        /// Dispose of the logger and finish logging.
        /// </summary>
        public void Dispose(bool isDisposed)
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
                    Log("Goodbye", LogLevel.Console, false);
                    FileWriter.Close();
                }
                catch
                {
                }
            }

            if (ConsoleWriter != null)
                ConsoleWriter = null;
        }

        public void Dispose()
        {
            Dispose(_isDisposed);

            _isDisposed = true;
        }
    }
}
