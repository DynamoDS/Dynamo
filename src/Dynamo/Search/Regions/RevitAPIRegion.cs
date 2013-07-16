using System;
using System.Windows.Input;
using Dynamo.Utilities;

namespace Dynamo.Search.Regions
{
    public class RevitAPIRegion<T> : RegionBase<T>
    {
        public RevitAPIRegion(Action<T> executeMethod, Func<T,bool> canExecuteMethod) 
            : base(executeMethod, canExecuteMethod) { }

        //public override bool CanExecute(object parameter)
        //{
        //    return true;
        //}

        //public override event EventHandler CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        //public override void Execute(object parameter)
        //{
        //    dynSettings.Controller.SearchViewModel.IncludeRevitAPIElements = !dynSettings.Controller.SearchViewModel.IncludeRevitAPIElements;
        //    dynSettings.ReturnFocusToSearch();
        //}

    }
}
