using Dynamo.Controls;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPackagesControl.xaml
    /// </summary>
    public partial class PackageManagerPackagesControl : UserControl
    {
        public PackageManagerSearchViewModel PkgSearchVM { get; set; }

        #region Properties

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

            this.Loaded += InitializeContext;
        }

        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;
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

            // new
            var name = this.Name;
            if (name.Equals(Dynamo.Wpf.Properties.Resources.PackageManagerSearchPackagesControlName))
            {
                var parent = WpfUtilities.FindUpVisualTree<PackageManagerSearchControl>(this) as PackageManagerSearchControl;
                if (parent == null) return;

                packageManagerSearchElementViewModel.Model.UIParent = parent.packageDetailsGrid;
                if(parent.packageDetailsGrid.Width.Value <= 1.0)
                {
                    var width = (parent.packageDetailsGrid.Parent as Grid).ActualWidth * 0.5;
                    parent.packageDetailsGrid.Width = new GridLength(width, GridUnitType.Pixel);
                }
            }
            else if (name.Equals(Dynamo.Wpf.Properties.Resources.PackageManagerMyPackagesControlName))
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
            // new

            PkgSearchVM.IsDetailPackagesExtensionOpened = true;
            PkgSearchVM.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
        }
    }
}
