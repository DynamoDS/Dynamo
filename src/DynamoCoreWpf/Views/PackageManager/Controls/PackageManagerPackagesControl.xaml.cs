using Dynamo.PackageManager.ViewModels;
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
            PkgSearchVM?.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
        }
    }
}
