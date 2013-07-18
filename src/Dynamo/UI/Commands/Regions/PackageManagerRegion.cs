using System;
using Dynamo.Search.Regions;
using Dynamo.Utilities;
using Dynamo.Search.SearchElements;

namespace Dynamo.UI.Commands.Regions
{
    public static partial class DynamoCommands
    {

        private static PackageManagerRegion<object> packageManagerRegion;
        public static PackageManagerRegion<object> PackageManagerRegionCommand
        {
            get
            {
                if (packageManagerRegion == null)
                    packageManagerRegion = new PackageManagerRegion<object>(PackageManagerRegionExecute, PackageManagerRegionCanExecute);
                return packageManagerRegion;
            }
        }

        private static void PackageManagerRegionExecute(object parameters)
        {
            if ((parameters as PackageManagerRegion<object>).Loaded == true)
            {
                UI.Commands.DynamoCommands.RefreshRemotePackagesCmd.Execute();
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