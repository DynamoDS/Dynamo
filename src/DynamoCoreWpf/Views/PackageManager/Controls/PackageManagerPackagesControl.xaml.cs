using Dynamo.PackageManager.ViewModels;
using Dynamo.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        public IEnumerable<PackageManagerSearchElementViewModel> SearchItemsCollection
        {
            get
            {
                return (IEnumerable<PackageManagerSearchElementViewModel>)GetValue(SearchItemsCollectionProperty);
            }
            set
            {
                if (value == null)
                    return;

                SetValue(SearchItemsCollectionProperty, value);
                CurrentItem = value.FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns the currently selected SearchItems
        /// </summary>
        public PackageManagerSearchElementViewModel CurrentItem
        {
            get { return (PackageManagerSearchElementViewModel)GetValue(CurrentItemProperty); }
            private set { SetValue(CurrentItemProperty, value); }
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty SearchItemsCollectionProperty =
            DependencyProperty.Register("SearchItemsCollection", typeof(IEnumerable<PackageManagerSearchElementViewModel>),
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
                this.packagesListBox.ItemsSource = searchItems;
            }
        }

        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(PackageManagerSearchElementViewModel),
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

                packageManagerSearchElementViewModel.SearchElementModel.UIParent = parent.packageDetailsGrid;
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

                packageManagerSearchElementViewModel.SearchElementModel.UIParent = parent.packageDetailsGrid;
                if (parent.packageDetailsGrid.Width.Value <= 1.0)
                {
                    var width = (parent.packageDetailsGrid.Parent as Grid).ActualWidth * 0.5;
                    parent.packageDetailsGrid.Width = new GridLength(width, GridUnitType.Pixel);
                }
            }

            PkgSearchVM.IsDetailPackagesExtensionOpened = true;
            PkgSearchVM?.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.SearchElementModel);
        }


        /// <summary>
        /// Fires when the user clicks the 'X' button to dismiss a package toast notification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseToastButton_OnClick(object sender, RoutedEventArgs e)
        {
            var PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;
            if (PkgSearchVM != null) { return; }

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
