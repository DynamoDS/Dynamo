using System.Windows;
using System.Windows.Controls;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackagePreviewPage.xaml
    /// </summary>
    public partial class PublishPackagePreviewPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public PublishPackagePreviewPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Preview Package Contents";
        }

        private void PublishPackagePublishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }

        public void LoadEvents()
        {
            this.IsEnabled = true;

            var treeView = this.customBrowserControl.customTreeView;

            var firstItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromIndex(0);
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }

            this.customBrowserControl.customTreeView_SelectedItemChanged(treeView, null);
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
    }
}
