using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.PackageManager.ViewModels;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchControl.xaml
    /// </summary>
    public partial class PackageManagerSearchControl : UserControl
    {
        public PackageManagerSearchViewModel PkgSearchVM { get; set; }

        public PackageManagerSearchControl()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
        }

        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;

            if (PkgSearchVM != null)
            {
                // Create the binding once the DataContext is available
                var binding = new Binding(nameof(PackageManagerSearchViewModel.InitialResultsLoaded))
                {
                    Source = PkgSearchVM,
                    Converter = new InverseBooleanToVisibilityCollapsedConverter()
                };

                this.loadingAnimationSearchControlScreen.SetBinding(UIElement.VisibilityProperty, binding);
            }

            this.Loaded -= InitializeContext;
        }

        private void OnShowFilterContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnFilterButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowFilterContextMenuFromLeftClicked(sender, e);
        }


        private void OnShowContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnSortButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

      

        /// <summary>
        /// Executes a command that opens the package details view extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            PkgSearchVM.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.SearchElementModel);
        }

        private void CompatibilityItem_OnFilter(object sender, FilterEventArgs e)
        {
            var item = e.Item as PackageManagerSearchViewModel.FilterEntry;
            e.Accepted = item.GroupName.Equals(Wpf.Properties.Resources.PackageFilterByCompatibility);
        }

        private void StatusItem_OnFilter(object sender, FilterEventArgs e)
        {
            var item = e.Item as PackageManagerSearchViewModel.FilterEntry;
            e.Accepted = item.GroupName.Equals(Wpf.Properties.Resources.PackageFilterByStatus);
        }

        private void DependencyItem_OnFilter(object sender, FilterEventArgs e)
        {
            var item = e.Item as PackageManagerSearchViewModel.FilterEntry;
            e.Accepted = item.GroupName.Equals(Wpf.Properties.Resources.PackageFilterByDependency);
        }

        internal void Dispose()
        {
            PkgSearchVM = null;

            this.Loaded -= InitializeContext;
        }
    }
}
