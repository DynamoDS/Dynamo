using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackageSelectPage.xaml
    /// </summary>
    public partial class PublishPackageSelectPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

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

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            string columnName = e.Column.SortMemberPath;
            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            var dataSource = (ObservableCollection<PackageItemRootViewModel>)dataGrid.ItemsSource;
            var filesAndFolders = dataSource.OrderBy(item => GetSortValue(item, columnName));

            IEnumerable<PackageItemRootViewModel> sortedData = filesAndFolders;

            if (direction == ListSortDirection.Descending)
            {
                sortedData = sortedData.Reverse();
            }   

            dataGrid.ItemsSource = sortedData.ToList();
        }

        private object GetSortValue(PackageItemRootViewModel item, string columnName)
        {
            if (columnName == "Name")
            {
                if (item.DependencyType == DependencyType.Folder)
                {
                    return -1; // Folders have a lower value, so they appear first.
                }
                else
                {
                    return 0; // Other items have a higher value.
                }
            }

            return item.DisplayName;
        }

        public void Dispose()
        {
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
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

}
