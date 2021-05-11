using System;
using System.Reflection;
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
            return _canExecute == null || _canExecute(parameter);
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

    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> executeMethod;
        private readonly Func<T, bool> canExecuteMethod;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, (o) => true)
        { }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "blabl");

            TypeInfo genericTypeInfo = typeof(T).GetTypeInfo();

            // DelegateCommand allows object or Nullable<>.  
            // note: Nullable<> is a struct so we cannot use a class constraint.
            if (genericTypeInfo.IsValueType)
            {
                if ((!genericTypeInfo.IsGenericType) || (!typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(genericTypeInfo.GetGenericTypeDefinition().GetTypeInfo())))
                {
                    throw new InvalidCastException("blbalb");
                }
            }

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(T parameter)
        {
            return canExecuteMethod(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        public void Execute(T parameter)
        {
            executeMethod(parameter);
        }

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }
    }

}
