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

        /// <summary>
        /// Constructor
        /// </summary>
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
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
            this.customBrowserControl.Dispose();
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
