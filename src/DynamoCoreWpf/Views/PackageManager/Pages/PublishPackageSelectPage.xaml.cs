using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class PublishPackageSelectPage : Page, INotifyPropertyChanged
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// A selection of PackageItemRootViewModel keeping track of currently seleted items
        /// Used for removing files and folders from the current Package contents
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> ItemSelection { get; set; } = new ObservableCollection<PackageItemRootViewModel>();


        private string _filesAndFoldersCounterPreview;

        public string FilesAndFoldersCounterPreview
        {
            get { return _filesAndFoldersCounterPreview; }
            set
            {
                if (_filesAndFoldersCounterPreview != value)
                {
                    _filesAndFoldersCounterPreview = value;
                    RaisePropertyChanged(nameof(FilesAndFoldersCounterPreview));
                }
            }
        }


        private bool _allItemsSelected;
        /// <summary>
        /// Indicates if all preview items are currently selected
        /// </summary>
        public bool AllItemsSelected
        {
            get { return _allItemsSelected; }
            set
            {
                if (_allItemsSelected != value)
                {
                    _allItemsSelected = value;
                    RaisePropertyChanged(nameof(AllItemsSelected));
                }
            }
        }
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///  Constructor
        /// </summary>
        public PublishPackageSelectPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Select Package Contents";
        }

        private void PublishPackageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PublishPackageViewModel == null) return;
            if (e.PropertyName == nameof(PublishPackageViewModel.PackageContents))  
            {
                int contentFileCount = 0;
                int contentFolderCount = 0;

                foreach (var item in PublishPackageViewModel.PackageContents)
                {
                    CountFileItems(item, ref contentFileCount, ref contentFolderCount);
                }

                //set the counter text
                var counterText = string.Format(Properties.Resources.PackageManagerPackageSelectCounter, contentFileCount, contentFolderCount);
                FilesAndFoldersCounterPreview = counterText;
            };
        }

        private void CountFileItems(PackageItemRootViewModel item, ref int contentFileCount, ref int contentFolderCount)
        {
            // Base count if the item itself is a file, custom node, or assembly
            if (item.DependencyType == DependencyType.File ||
                item.DependencyType == DependencyType.CustomNode ||
                item.DependencyType == DependencyType.Assembly)
            {
                contentFileCount++;
            }

            // If the item is a folder, recursively count its children
            if (item.DependencyType == DependencyType.Folder)
            {
                contentFolderCount++;
                foreach (var child in item.ChildItems)
                {
                    CountFileItems(child, ref contentFileCount, ref contentFolderCount);
                }
            }
        }

        private void PublishPackagePublishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(PublishPackageViewModel != null)
            {
                PublishPackageViewModel.PropertyChanged -= PublishPackageViewModel_PropertyChanged;
            }

            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
            PublishPackageViewModel.PropertyChanged += PublishPackageViewModel_PropertyChanged;
        }

        internal void LoadEvents()
        {
            this.IsEnabled = true;

            if (customBrowserControl != null)
            {
                var treeView = customBrowserControl.customTreeView;

                customBrowserControl.RefreshCustomTreeView();
                customBrowserControl.customTreeView_SelectedItemChanged(treeView, null);
            }
        }

        public void Dispose()
        {
            this.ItemSelection?.Clear();
            this.PublishPackageViewModel.PropertyChanged -= PublishPackageViewModel_PropertyChanged;
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
            this.customBrowserControl?.Dispose();
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
            AllItemsSelected = false;
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

            if(ItemSelection.Count == PublishPackageViewModel.RootContents.Count)
            {
                AllItemsSelected = true;
            }
            else { AllItemsSelected = false; }
        }

        // Clears the current item selection
        private void DeselectButton_Click(object sender, RoutedEventArgs e)
        {
            ItemSelection.ToList()
                         .ForEach(item => { item.IsSelected = false; });

            ItemSelection.Clear();
            AllItemsSelected = false;
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

    /// <summary>
    /// Assembly can either have an Assembly or File DependencyType. We need to check for both.
    /// </summary>
    public class AsselmblyPackageItemRootViewModelToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is PackageItemRootViewModel item)
            {
                if (item.DependencyType.Equals(DependencyType.Assembly))
                {
                    // Return visible if the item is an Assembly
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
