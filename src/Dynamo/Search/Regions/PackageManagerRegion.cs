using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Utilities;
using Dynamo.Search.SearchElements;

namespace Dynamo.Commands
{
    public static partial class DynamoCommands
    {

        private static Dynamo.Search.Regions.PackageManagerRegion packageManagerRegion;
        public static Dynamo.Search.Regions.PackageManagerRegion PackageManagerRegionCommand
        {
            get
            {
                if (packageManagerRegion == null)
                    packageManagerRegion = new Dynamo.Search.Regions.PackageManagerRegion();
                return packageManagerRegion;
            }
        }
    }

}

namespace Dynamo.Search.Regions
{
    public class PackageManagerRegion : RegionBase
    {
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public override void Execute(object parameter)
        {
            if (Loaded)
            {
                DynamoCommands.RefreshRemotePackagesCmd.Execute(null);
            }
            else
            {
                dynSettings.Controller.SearchViewModel.SearchDictionary.Remove((value) => value is PackageManagerSearchElement);
                dynSettings.Controller.SearchViewModel.SearchAndUpdateResultsSync(dynSettings.Controller.SearchViewModel.SearchText);
            }

            dynSettings.ReturnFocusToSearch();
            
        }
    }
}
