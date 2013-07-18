using System;
using Dynamo.Search.Regions;
using Dynamo.Utilities;
using Dynamo.Search.SearchElements;

namespace Dynamo.Commands
{
    public static partial class DynamoCommands
    {

        private static Dynamo.Search.Regions.PackageManagerRegion<object> packageManagerRegion;
        public static Dynamo.Search.Regions.PackageManagerRegion<object> PackageManagerRegionCommand
        {
            get
            {
                if (packageManagerRegion == null)
                    packageManagerRegion = new Dynamo.Search.Regions.PackageManagerRegion<object>(PackageManagerRegionExecute, PackageManagerRegionCanExecute);
                return packageManagerRegion;
            }
        }

        private static void PackageManagerRegionExecute(object parameters)
        {
            if ((parameters as PackageManagerRegion<object>).Loaded == true)
            {
                DynamoCommands.RefreshRemotePackagesCmd.Execute();
            }
            else
            {
                dynSettings.Controller.SearchViewModel.SearchDictionary.Remove((value) => value is PackageManagerSearchElement);
                dynSettings.Controller.SearchViewModel.SearchAndUpdateResultsSync(dynSettings.Controller.SearchViewModel.SearchText);
            }

            dynSettings.ReturnFocusToSearch();
        }

        private static bool PackageManagerRegionCanExecute(object parameter)
        {
            return true;
        }
    }

}

namespace Dynamo.Search.Regions
{

    public class PackageManagerRegion<T> : RegionBase<T>
    {
        public PackageManagerRegion(Action<T> executeMethod, Func<T,bool> canExecuteMethod ):base(executeMethod, canExecuteMethod){}

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
        //    if (Loaded)
        //    {
        //        DynamoCommands.RefreshRemotePackagesCmd.Execute();
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
