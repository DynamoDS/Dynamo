using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPackagesControl.xaml
    /// </summary>
    public partial class PackageManagerPackagesControl : UserControl
    {

        #region Properties

        private static readonly string packageManagerSearchPackagesName = "packageManagerSearchPackages";
        private static readonly string packageManagerMyPackagesName = "packageManagerMyPackages";

        /// <summary>
        ///     Allows different collections of SearchItems to be assigned per instance of the PackageManagerPackagesControl
        /// </summary>
        public IEnumerable<PackageManagerSearchElementViewModel> SearchItems
        {
            get
            {
                return (IEnumerable<PackageManagerSearchElementViewModel>)GetValue(SearchItemsProperty);
            }
            set
            {
                if (value == null)
                    return;

                SetValue(SearchItemsProperty, value);
                SelectedItem = value.FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns the currently selected SearchItems
        /// </summary>
        public PackageManagerSearchElementViewModel SelectedItem
        {
            get { return (PackageManagerSearchElementViewModel)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty SearchItemsProperty =
            DependencyProperty.Register("SearchItems", typeof(IEnumerable<PackageManagerSearchElementViewModel>),
              typeof(PackageManagerPackagesControl), new PropertyMetadata(null, SearchItemsPropertyChanged));

        private static void SearchItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var packageManagerPackagesControl = sender as PackageManagerPackagesControl;
            if (packageManagerPackagesControl == null) return;
            packageManagerPackagesControl.OnSearchItemsPropertyChanged(e.OldValue, e.NewValue);
        }

        private void OnSearchItemsPropertyChanged(object oldValue, object newValue)
        {
            var searchItems = (IEnumerable<PackageManagerSearchElementViewModel>)newValue;
            if(searchItems != null)
            {
                myPackagesListBox.ItemsSource = searchItems;
            }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(PackageManagerSearchElementViewModel),
              typeof(PackageManagerPackagesControl), new PropertyMetadata(null));

        #endregion

        public PackageManagerPackagesControl()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Executes a command that opens the package details view extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            var PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;

            var name = this.Name;
            if (name.Equals(packageManagerSearchPackagesName))
            {
                var parent = WpfUtilities.FindUpVisualTree<PackageManagerSearchControl>(this) as PackageManagerSearchControl;
                if (parent == null) return;

                packageManagerSearchElementViewModel.Model.UIParent = parent.packageDetailsGrid;
                if (parent.packageDetailsGrid.Width.Value <= 1.0)
                {
                    var width = (parent.packageDetailsGrid.Parent as Grid).ActualWidth * 0.5;
                    parent.packageDetailsGrid.Width = new GridLength(width, GridUnitType.Pixel);
                }
            }
            else if (name.Equals(packageManagerMyPackagesName))
            {
                var parent = WpfUtilities.FindUpVisualTree<PackageManagerView>(this) as PackageManagerView;
                if (parent == null) return;

                packageManagerSearchElementViewModel.Model.UIParent = parent.packageDetailsGrid;
                if (parent.packageDetailsGrid.Width.Value <= 1.0)
                {
                    var width = (parent.packageDetailsGrid.Parent as Grid).ActualWidth * 0.5;
                    parent.packageDetailsGrid.Width = new GridLength(width, GridUnitType.Pixel);
                }
            }

            PkgSearchVM.IsDetailPackagesExtensionOpened = true;
            PkgSearchVM?.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
        }

        private void DropDownInstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("installContextMenu") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }



        /// <summary>
        /// Fires when the user clicks the 'X' button to dismiss a package toast notification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseToastButton_OnClick(object sender, RoutedEventArgs e)
        {
            var PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;
            if (PkgSearchVM == null) { return; }

            Button button = sender as Button;

            if (button.DataContext is PackageDownloadHandle packageDownloadHandle)
            {
                PkgSearchVM.ClearToastNotificationCommand.Execute(packageDownloadHandle);
            }
            else if (button.DataContext is PackageManagerSearchElement packageSearchElement)
            {
                PkgSearchVM.ClearToastNotificationCommand.Execute(packageSearchElement);
            }
            return;
        }
    }
}
