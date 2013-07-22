using System;
using Dynamo.Search.Regions;
using Dynamo.Utilities;
using Dynamo.Search.SearchElements;

namespace Dynamo.UI.Commands.Regions
{
    public static partial class DynamoCommands
    {

        private static PackageManagerRegion packageManagerRegion;
        public static PackageManagerRegion PackageManagerRegionCommand
        {
            get
            {
                if (packageManagerRegion == null)
                    packageManagerRegion = new PackageManagerRegion(PackageManagerRegionExecute, PackageManagerRegionCanExecute);
                return packageManagerRegion;
            }
        }

        private static void PackageManagerRegionExecute(object parameters)
        {
            if ((parameters as PackageManagerRegion).Loaded == true)
            {
                UI.Commands.DynamoCommands.RefreshRemotePackagesCmd.Execute(null);
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

    public class PackageManagerRegion : RegionBase
    {
        public PackageManagerRegion(Action<object> executeMethod, System.Predicate<object> canExecuteMethod ):base(executeMethod, canExecuteMethod){}

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