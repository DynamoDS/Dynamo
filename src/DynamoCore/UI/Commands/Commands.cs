using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using Newtonsoft.Json;
using Dynamo.Utilities;

namespace Dynamo.UI.Commands
{
    /// <summary>
    /// Custom implementation of DelegateCommand which prints to the log.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        //http://wpftutorial.net/DelegateCommand.html

        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute,
                       Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            //OnExecute(parameter);
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        private void OnExecute(object parameter)
        {
            //http://joshsmithonwpf.wordpress.com/2007/10/25/logging-routed-commands/

            var msg = new StringBuilder();

            string paramStr = "";

            if (parameter == null)
                paramStr = "null";
            else if (parameter.GetType() == typeof(Dictionary<object, object>))
                paramStr = JsonConvert.SerializeObject(parameter).ToString();
            else
                paramStr = parameter.ToString();
            
            msg.AppendFormat("COMMAND: Name={0}, Parameter={1}", _execute.Method.Name, paramStr);

            dynSettings.Controller.DynamoLogger.Log(msg.ToString(), LogLevel.File);
        }
    }

    public static partial class DynamoCommands
    {
        private static readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        private static bool isProcessingCommandQueue = false;
 
        public static bool IsProcessingCommandQueue
        {
            get { return isProcessingCommandQueue; }
        }

        public static Queue<Tuple<object, object>> CommandQueue
        {
            get { return commandQueue; }
        }

        #region CommandQueue

        /// <summary>
        /// Add a command to the CommandQueue and run ProcessCommandQueue(), providing null as the 
        /// command arguments
        /// </summary>
        /// <param name="command">The command to run</param>
        public static void RunCommand(DelegateCommand command)
        {
            RunCommand(command, null);
        }

        /// <summary>
        /// Add a command to the CommandQueue and run ProcessCommandQueue(), providing the given
        /// arguments to the command
        /// </summary>
        /// <param name="command">The command to run</param>
        /// <param name="args">Parameters to give to the command</param>
        public static void RunCommand(DelegateCommand command, object args)
        {
            var commandAndParams = Tuple.Create<object, object>(command, args);
            CommandQueue.Enqueue(commandAndParams);
            ProcessCommandQueue();
        }

        //private void Hooks_DispatcherInactive(object sender, EventArgs e)
        //{
        //    ProcessCommandQueue();
        //}

        /// <summary>
        ///     Run all of the commands in the CommandQueue
        /// </summary>
        public static void ProcessCommandQueue()
        {
            while (commandQueue.Count > 0)
            {
                var cmdData = commandQueue.Dequeue();
                var cmd = cmdData.Item1 as DelegateCommand;
                if (cmd != null)
                {
                    if (cmd.CanExecute(cmdData.Item2))
                    {
                        cmd.Execute(cmdData.Item2);
                    }
                }
            }
            commandQueue.Clear();

            if (dynSettings.Controller.UIDispatcher != null)
            {
                dynSettings.Controller.DynamoLogger.Log(string.Format("dynSettings.Bench Thread : {0}",
                                                       dynSettings.Controller.UIDispatcher.Thread.ManagedThreadId.ToString(CultureInfo.InvariantCulture)));
            }
        }

        #endregion
    }

}
