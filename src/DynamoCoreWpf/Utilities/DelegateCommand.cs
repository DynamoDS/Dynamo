using System;
using System.Windows.Input;
using System.Windows.Threading;

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
        private readonly Dispatcher _dispatcher;

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
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            //OnExecute(parameter);
            _execute(parameter);
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/>, marshalling to the UI thread if necessary.
        /// Calling from a non-UI thread without this guard causes
        /// <see cref="System.Threading.SynchronizationLockException"/> inside WPF's
        /// CommandManager.FindCommandBinding (DYN-10409).
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler == null) return;

            if (!_dispatcher.CheckAccess())
                _dispatcher.BeginInvoke(new Action(() => handler(this, EventArgs.Empty)));
            else
                handler(this, EventArgs.Empty);
        }

    }
}
