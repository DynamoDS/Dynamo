using System;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Search.Regions
{
    public abstract class RegionBase<T> : DelegateCommand<T>
    {
        public bool Loaded { get; set; }

        protected RegionBase(Action<T> executeMethod, Func<T,bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            Loaded = false;
        }

        //public abstract bool CanExecute(object parameter);

        //public abstract event EventHandler CanExecuteChanged;

        //public abstract void Execute(object parameter);
    }
}
