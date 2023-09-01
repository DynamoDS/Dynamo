using System;
using System.Linq;
using System.Windows.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewExtension : ViewExtensionBase
    {
        internal PackageManagerExtension PackageManagerExtension { get; set; }
        internal PackageDetailsView PackageDetailsView { get; set; }
        internal PackageDetailsViewModel PackageDetailsViewModel { get; set; }
        internal PackageManagerClientViewModel PackageManagerClientViewModel { get; set; }
        internal ViewLoadedParams ViewLoadedParamsReference { get; set; }
        private Grid Grid { get; set; } 

        public override string UniqueId => "C71CA1B9-BF9F-425A-A12C-53DF56770406";

        public override string Name => Properties.Resources.PackageDetailsViewExtensionName;

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            var packageManager = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            PackageManagerExtension = packageManager;
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            ViewLoadedParamsReference = viewLoadedParams;
            viewLoadedParams.ViewExtensionOpenRequestWithParameter += OnViewExtensionOpenWithParameterRequest;
            
            DynamoViewModel dynamoViewModel = viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            PackageManagerClientViewModel = dynamoViewModel.PackageManagerClientViewModel;
        }

        internal void OnViewExtensionOpenWithParameterRequest(string extensionIdentification, object obj)
        {
            // If param type does not match return
            if (!(obj is PackageManagerSearchElement pmSearchElement)) return;

            // Make sure the target view extension is the current one
            // Either view extension Guid match or name match
            // String comparison of Guid is simplified check, if desired, we can update to use Guid check
            if (UniqueId.ToString().Equals(extensionIdentification) ||
                extensionIdentification.Equals(Name))
            {
                OpenPackageDetails(pmSearchElement);
            }
            return;
        }
        
        internal void OpenPackageDetails(PackageManagerSearchElement packageManagerSearchElement)
        {
            PackageDetailsViewModel = new PackageDetailsViewModel(this, packageManagerSearchElement);

            if (PackageDetailsView == null)
            {
                PackageDetailsView = new PackageDetailsView();
                PackageDetailsView.Closed += OnPackageDetailsViewClosed;
            }

            if (PackageDetailsView == null) PackageDetailsView = new PackageDetailsView();
            PackageDetailsView.DataContext = PackageDetailsViewModel;

            if (packageManagerSearchElement.UIParent != null)
            {
                HostPackageDetailsExtension(packageManagerSearchElement);
                return;
            }

            ViewLoadedParamsReference?.AddToExtensionsSideBar(this, PackageDetailsView);
        }

        private void HostPackageDetailsExtension(PackageManagerSearchElement packageManagerSearchElement)
        {
            var column = packageManagerSearchElement.UIParent as ColumnDefinition;
            if (column == null) return;
            if (!(column.Parent is Grid) || (column.Parent as Grid).Name.Equals(Grid?.Name)) return;

            if (Grid == null)
            {
                var packageManagerView = WpfUtilities.FindUpVisualTree<PackageManagerView>(column.Parent) as PackageManagerView;
                if (packageManagerView == null) return;
                // subscribe to the closed event of the PackageManagerView
                packageManagerView.Closed += DataContext_Closed;

            }
            else
            {
                // we have changed tabs - remove the ex tension from the old grid first
                RemoveViewExtensionFromGrid();
            }

            Grid = column.Parent as Grid;

            Grid.SetColumn(PackageDetailsView, 2);
            Grid.Children.Add(PackageDetailsView);
        }

        private void OnPackageDetailsViewClosed(object sender, EventArgs e)
        {
            PackageDetailsView.Closed -= OnPackageDetailsViewClosed;

            if (Grid == null) return;

            var packageManagerView = WpfUtilities.FindUpVisualTree<PackageManagerView>(Grid) as PackageManagerView;
            if (packageManagerView != null)
            {
                packageManagerView.PackageManagerViewModel.PackageSearchViewModel.IsDetailPackagesExtensionOpened = false;
                packageManagerView.Closed -= DataContext_Closed;
            }

            RemoveViewExtensionFromGrid();
            this.Closed();
        }

        private void RemoveViewExtensionFromGrid()
        {
            if (Grid == null) return;

            var column = Grid.ColumnDefinitions[Grid.GetColumn(PackageDetailsView)];
            column.Width = new System.Windows.GridLength(0);

            Grid.Children.Remove(PackageDetailsView);
        }

        private void DataContext_Closed(object sender, EventArgs e)
        {
            var packageManagerView = sender as PackageManagerView;
            if (packageManagerView != null)
            {
                packageManagerView.Closed -= DataContext_Closed;
            }

            this.Closed();
        }


        public override void Dispose()
        {
            PackageDetailsViewModel?.Dispose();
            if (ViewLoadedParamsReference != null)
            {
                ViewLoadedParamsReference.ViewExtensionOpenRequestWithParameter -= OnViewExtensionOpenWithParameterRequest;
            }
        }

        public override void Closed()
        {
            PackageDetailsViewModel = null;
            PackageDetailsView = null;
            Grid = null;
        }
    }
}
