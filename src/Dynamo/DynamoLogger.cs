using System;
using System.IO;
using System.Reflection;
using System.Text;
using Dynamo.Models;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public enum LogLevel{Console, File}

    public class DynamoLogger:NotificationObject
    {
        private static DynamoLogger instance;

        public TextWriter FileWriter { get; set; }
        public StringBuilder ConsoleWriter { get; set; }

        private string _logPath;
        public string LogPath 
        {
            get { return _logPath; }
        }

        public string LogText
        {
            get
            {
                if (ConsoleWriter != null)
                    return ConsoleWriter.ToString();
                return "";
            }
        }

        /// <summary>
        /// The singelton instance.
        /// </summary>
        public static DynamoLogger Instance
        {
            get
            {
                if(instance == null)
                    instance = new DynamoLogger();
                return instance;
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        private DynamoLogger()
        {
            
        }

        /// <summary>
        /// Log the message to the the correct path
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message, LogLevel level)
        {
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
                        }
                        catch
                        {
                            // likely caught if the writer is closed
                        }
                    }  
                    break;
            }

            RaisePropertyChanged("LogText");
              
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
        /// Log some node info
        /// </summary>
        /// <param name="node"></param>
        public void Log(dynNodeModel node)
        {
            string exp = node.PrintExpression();
            Log("> " + exp, LogLevel.Console);
        }

        /// <summary>
        /// Log an expression
        /// </summary>
        /// <param name="expression"></param>
        public void Log(FScheme.Expression expression)
        {
            Instance.Log(FScheme.printExpression("\t", expression), LogLevel.Console);
        }

        //public void Log(string message)
        //{
        //    _sw.WriteLine(message);
        //    LogText = _sw.ToString();

        //    if (CanWriteToLog(null))
        //    {
        //        WriteToLog(message);
        //    }
        //}

        public void ClearLog()
        {
            ConsoleWriter.Clear();
            RaisePropertyChanged("LogText");
        }

        /// <summary>
        /// Begin logging.
        /// </summary>
        public void StartLogging()
        {
            //create log files in a directory 
            //with the executing assembly
            string log_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dynamo_logs");
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

        /// <summary>
        /// Finish logging.
        /// </summary>
        public void FinishLogging()
        {
            if (FileWriter != null)
            {
                try
                {
                    FileWriter.Flush();
                    Log("Goodbye", LogLevel.Console);
                    FileWriter.Close();
                }
                catch
                {
                }
            }

            if (ConsoleWriter != null)
                ConsoleWriter = null;
        }
    }
}
