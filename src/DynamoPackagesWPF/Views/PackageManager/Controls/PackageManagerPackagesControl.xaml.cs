using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI;
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
                packagesListBox.ItemsSource = searchItems;
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

            var pkgSearchVM = this.DataContext as PackageManagerSearchViewModel;

            ExecuteOpenPackageDetails(packageManagerSearchElementViewModel, pkgSearchVM);
        }

        private void PackageName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender is TextBlock textBlock)) return;
            if (!(textBlock.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            var pkgSearchVM = this.DataContext as PackageManagerSearchViewModel;

            ExecuteOpenPackageDetails(packageManagerSearchElementViewModel, pkgSearchVM);
        }

        private void ExecuteOpenPackageDetails(PackageManagerSearchElementViewModel packageManagerSearchElementViewModel, PackageManagerSearchViewModel pkgSearchVM)
        {
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

            pkgSearchVM.IsDetailPackagesExtensionOpened = true;
            pkgSearchVM?.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.SearchElementModel);
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


        private void DropDownInstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || button.DataContext == null) { return; }

            var contextMenu = new ContextMenu();
            var commandBinding = new Binding("DownloadLatestToCustomPathCommand");
            commandBinding.Source = button.DataContext;
            var commandParameterBinding = new Binding("LatestVersion");
            commandParameterBinding.Source = button.DataContext;

            var contextMenuStyle = new Style(typeof(ContextMenu));
            contextMenuStyle.BasedOn = (Style)SharedDictionaryManager.DynamoModernDictionary["DarkContextMenuStyle"];

            //Apply the Style to the ContextMenu
            contextMenu.Style = contextMenuStyle;
            contextMenu.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));

            // Create and add menu items to the ContextMenu
            var menuItem = new MenuItem 
            {
                Header = Dynamo.Wpf.Properties.Resources.PackageSearchViewInstallLatestVersionTo,
                MinWidth = 60,
                MinHeight = 30,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            contextMenu.Items.Add(menuItem);
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty, commandBinding);
            BindingOperations.SetBinding(menuItem, MenuItem.CommandParameterProperty, commandParameterBinding);
                
            // Attach the ContextMenu to the button
            button.ContextMenu = contextMenu;

            // Open the context menu when the left mouse button is pressed
            contextMenu.IsOpen = true;            
        }

        // Attach the 'context menu' button to the parent button
        private void DropDownInstallButton_Loaded(object sender, RoutedEventArgs e)
        {
            var installButton = sender as Button;
            if (installButton != null)
            {
                var dropDownInstallButton = installButton.Template.FindName("dropDownInstallButton", installButton) as Button;
                if (dropDownInstallButton != null)
                {
                    dropDownInstallButton.Click -= DropDownInstallButton_OnClick;

                    // Now that the parent button is loaded, we can assign the Click event to the context menu button
                    dropDownInstallButton.Click += DropDownInstallButton_OnClick;
                }
            }
        }

        // Dispose of DropDownInstallButton_OnClick when the parent button is unloaded
        private void DropDownInstallButton_Unloaded(object sender, RoutedEventArgs e)
        {
            var installButton = sender as Button;
            if (installButton != null)
            {
                var dropDownInstallButton = installButton.Template.FindName("dropDownInstallButton", installButton) as Button;
                if (dropDownInstallButton != null)
                {
                    dropDownInstallButton.Click -= DropDownInstallButton_OnClick;
                }
            }
        }
    }
}
