using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Selection;

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
            dynSettings.Writer.WriteLine(logText);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (dynSettings.Writer != null)
            {
                return true;
            }

            return false;
        }
    }

}
