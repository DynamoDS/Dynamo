using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;

namespace Views.PackageManager.Pages
{
    /// <summary>
    /// Interaction logic for PublishPackageFinishPage.xaml
    /// </summary>
    public partial class PublishPackageFinishPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public PublishPackageFinishPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishFinishPage_DataContextChanged;
            this.Tag = "Publish Finished Page";
        }

        internal void LoadEvents()
        {
            if ( PublishPackageViewModel == null ) { return; }

            var uploadType = PublishPackageViewModel.UploadType;
            var publishedFiles = PackageItemRootViewModel.GetFiles(PublishPackageViewModel.PackageContents.ToList());
            var count = publishedFiles.Count(x => x.DependencyType != DependencyType.Folder
                                          || x.DependencyType != DependencyType.CustomNodePreview);
            var message = uploadType.Equals(PackageUploadHandle.UploadType.Local) ?
                Dynamo.Wpf.Properties.Resources.PackageManagerFinishedPackageFilesPublishedMessage :
                Dynamo.Wpf.Properties.Resources.PackageManagerFinishedPackageFilesUploadedMessage;

            this.filesUploadedMessage.Text = String.Format("{0} {1}", count.ToString(), message);
            this.packagePathTextBlox.Text = PublishPackageViewModel.RootFolder;
        }

        private void PublishFinishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }

        private void uploadFilesDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.packagePathTextBlox is TextBox pathTextBox && !String.IsNullOrEmpty(pathTextBox.Text)){
                var directory = pathTextBox.Text;
                if (!Directory.Exists(directory)) { return; }

                Process.Start("explorer.exe", directory);
            }
        }

        public void Dispose()
        {
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishFinishPage_DataContextChanged;
        }
    }
}
