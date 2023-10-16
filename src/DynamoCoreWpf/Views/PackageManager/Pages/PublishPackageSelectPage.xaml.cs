using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackageSelectPage.xaml
    /// </summary>
    public partial class PublishPackageSelectPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        /// <summary>
        /// A selection of PackageItemRootViewModel keeping track of currently seleted items
        /// Used for removing files and folders from the current Package contents
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> ItemSelection { get; set; } = new ObservableCollection<PackageItemRootViewModel>();

        public PublishPackageSelectPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Select Package Contents";
        }

        private void PublishPackagePublishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }

        public void LoadEvents()
        {
            var firstItem = (TreeViewItem)this.customBrowserControl.customTreeView.ItemContainerGenerator.ContainerFromIndex(0);
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }
        }

        public void Dispose()
        {
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
        }

        private void customBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var firstItem = (TreeViewItem)this.customBrowserControl.customTreeView.ItemContainerGenerator.ContainerFromIndex(0);
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }
        }

        // Removes all currently selected PackageItemRootViewModel items
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var item in ItemSelection)
            {
                PublishPackageViewModel.RemoveItemCommand.Execute(item);
            }

            ItemSelection.Clear();
        }

        // Adds/removes (selects/deselects) item to the ItemSelection collection
        private void SelectItemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) { return; }

            var row = WpfUtilities.FindUpVisualTree<DataGridRow>(checkBox);
            if (row == null) { return; }

            var item = row.DataContext as PackageItemRootViewModel;
            if (item == null) { return; }

            if (checkBox.IsChecked == true)
            {
                item.IsSelected = true;
                ItemSelection.Add(item);
            }
            else
            {
                item.IsSelected = false;
                ItemSelection.Remove(item);
            }
        }

        // Clears the current item selection
        private void DeselectButton_Click(object sender, RoutedEventArgs e)
        {
            ItemSelection.ToList()
                         .ForEach(item => { item.IsSelected = false; });

            ItemSelection.Clear();
        }

        // Selects/deselects all items inside the current RootContents collection
        private void SelectDeselectButton_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (sender as CheckBox);
            if (checkBox == null) { return; }

            ItemSelection.Clear();

            if (checkBox.IsChecked == true)
            {
                PublishPackageViewModel.RootContents.ToList().ForEach(item => { item.IsSelected = true; });
                ItemSelection.AddRange(PublishPackageViewModel.RootContents);
            }
            else
            {
                PublishPackageViewModel.RootContents.ToList().ForEach(item => { item.IsSelected = false; });
            }
        }
    }


    public class DependencyTypeToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DependencyType.Folder)
            {
                // Return visible only if the item is a Folder
                return "/DynamoCoreWpf;component/UI/Images/file-generic-16px.png";
            }

            // If the item is anything else (Assembly, File, Custom Node) return collapsed
            return "/DynamoCoreWpf;component/UI/Images/folder-generic-16px.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DependencyTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is DependencyType dependencyType)
            {
                if (dependencyType.ToString() == parameter?.ToString())
                {
                    // Return visible if the item matches the specified dependency type
                    return Visibility.Visible;
                }
            }
            // If the item does not match the specified dependency type or value is null, return collapsed
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
