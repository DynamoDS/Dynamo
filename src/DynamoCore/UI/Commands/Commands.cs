using System;
using System.Windows.Input;

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

        // KILLDYNSETTINGS - this isn't used
        //private void OnExecute(object parameter)
        //{
        //    //http://joshsmithonwpf.wordpress.com/2007/10/25/logging-routed-commands/

        //    var msg = new StringBuilder();

        //    string paramStr = "";

        //    if (parameter == null)
        //        paramStr = "null";
        //    else if (parameter.GetType() == typeof(Dictionary<object, object>))
        //        paramStr = JsonConvert.SerializeObject(parameter).ToString();
        //    else
        //        paramStr = parameter.ToString();
            
        //    msg.AppendFormat("COMMAND: Name={0}, Parameter={1}", _execute.Method.Name, paramStr);

        //    dynamoModel.Logger.Log(msg.ToString(), LogLevel.File);
        //}
    }

}
