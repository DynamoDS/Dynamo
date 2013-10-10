using System;
using System.Windows.Input;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Commands
{
    public static partial class DynamoCommands
    {

        private static Dynamo.Search.Regions.PackageManagerRegion packageManagerRegion;
        public static Dynamo.Search.Regions.PackageManagerRegion PackageManagerRegionCommand
        {
            get
            {
                //if (packageManagerRegion == null)
                //    packageManagerRegion = new Dynamo.Search.Regions.PackageManagerRegion(SearchViewModel.PackageManagerRegionExecute, SearchViewModel.PackageManagerRegionCanExecute);
                return packageManagerRegion;
            }
        }
    }

}

namespace Dynamo.Search.Regions
{
    public class PackageManagerRegion : RegionBase
    {
        public PackageManagerRegion(Action<object> executeMethod, System.Predicate<object> canExecuteMethod) 
            : base(executeMethod, canExecuteMethod) { }

        //public bool CanExecute(object parameter)
        //{
        //    return true;
        //}

        //public event EventHandler CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        //public void Execute(object parameter)
        //{
        //    if (Loaded)
        //    {
        //        //DynamoCommands.RefreshRemotePackagesCmd.Execute(null);
        //    }
        //    else
        //    {
        //        dynSettings.Controller.SearchViewModel.SearchDictionary.Remove((value) => value is PackageManagerSearchElement);
        //        dynSettings.Controller.SearchViewModel.SearchAndUpdateResultsSync(dynSettings.Controller.SearchViewModel.SearchText);
        //    }

        //    dynSettings.ReturnFocusToSearch();
            
        //}
    }
}
