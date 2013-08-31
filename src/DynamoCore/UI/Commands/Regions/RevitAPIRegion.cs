using System;

namespace Dynamo.Search.Regions
{
    public class RevitAPIRegion : RegionBase
    {
        public RevitAPIRegion(Action<object> executeMethod, System.Predicate<object> canExecuteMethod) 
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
