using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackagePreviewPage.xaml
    /// </summary>
    public partial class PublishPackagePreviewPage : Page, INotifyPropertyChanged
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PublishPackagePreviewPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Preview Package Contents";
        }

        private void PublishPackageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PublishPackageViewModel == null) return;
            if (e.PropertyName == nameof(PublishPackageViewModel.PreviewPackageContents))
            {
                int contentFileCount = 0;
                int contentFolderCount = 0;

                foreach (var item in PublishPackageViewModel.PreviewPackageContents)
                {
                    CountFileItems(item, ref contentFileCount, ref contentFolderCount);
                }

                //set the counter text
                var counterText = string.Format(Properties.Resources.PackageManagerPackagePreviewCounter, contentFileCount, contentFolderCount);
                FilesAndFoldersCounterPreview = counterText;
            };
        }

        private void CountFileItems(PackageItemRootViewModel item, ref int contentFileCount, ref int contentFolderCount)
        {
            // Base count if the item itself is a file, custom node, or assembly
            if (item.DependencyType == DependencyType.File ||
                item.DependencyType == DependencyType.CustomNodePreview ||
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
            if (PublishPackageViewModel != null)
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
            this.PublishPackageViewModel.PropertyChanged -= PublishPackageViewModel_PropertyChanged;
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
            this.customBrowserControl?.Dispose();
        }

        private void customBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (customBrowserControl != null)
            {
                customBrowserControl.RefreshCustomTreeView();
            }
        }
    }
}
