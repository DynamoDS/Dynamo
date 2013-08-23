using System;
using Dynamo.UI.Commands;

namespace Dynamo.Search.Regions
{
    public abstract class RegionBase : DelegateCommand
    {
        public bool Loaded { get; set; }

        protected RegionBase(Action<object> executeMethod, System.Predicate<object> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            Loaded = false;
        }

        //public abstract bool CanExecute(object parameter);

        //public abstract event EventHandler CanExecuteChanged;

        //public abstract void Execute(object parameter);
    }
}
