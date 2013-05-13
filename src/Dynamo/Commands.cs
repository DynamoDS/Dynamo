using System;
using System.Windows.Input;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static WriteToLogCommand writeToLogCmd;
        public static WriteToLogCommand WriteToLogCmd
        {
            get
            {
                if (writeToLogCmd == null)
                    writeToLogCmd = new WriteToLogCommand();

                return writeToLogCmd;
            }
        }

    }

    public class WriteToLogCommand : ICommand
    {
        public void Execute(object parameters)
        {
            if (parameters == null) return;

            string logText = parameters.ToString();
            DynamoLogger.Instance.Log(logText);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (DynamoLogger.Instance != null)
            {
                return true;
            }

            return false;

        }
    }

}
