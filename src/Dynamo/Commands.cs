using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static DelegateCommand<object> writeToLogCmd;
        public static DelegateCommand<object> WriteToLogCmd
        {
            get
            {
                if (writeToLogCmd == null)
                {
                    writeToLogCmd = new DelegateCommand<object>(WriteToLog, CanWriteToLog);
                }

                return writeToLogCmd;
            }
        }

        private static void WriteToLog(object parameters)
        {
            if (parameters == null) return;
            string logText = parameters.ToString();
            DynamoLogger.Instance.Log(logText);
        }

        private static bool CanWriteToLog(object parameters)
        {
            if (DynamoLogger.Instance != null)
            {
                return true;
            }

            return false;
        }

        //private static WriteToLogCommand writeToLogCmd;
        //public static WriteToLogCommand WriteToLogCmd
        //{
        //    get
        //    {
        //        if (writeToLogCmd == null)
        //            writeToLogCmd = new WriteToLogCommand();

        //        return writeToLogCmd;
        //    }
        //}

    }

    //public class WriteToLogCommand : ICommand
    //{
    //    public void Execute(object parameters)
    //    {
    //        if (parameters == null) return;

    //        string logText = parameters.ToString();
    //        DynamoLogger.Instance.Log(logText);
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        if (DynamoLogger.Instance != null)
    //        {
    //            return true;
    //        }

    //        return false;

    //    }
    //}

}
