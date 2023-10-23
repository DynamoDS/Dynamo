using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            var publishedFiles = PackageItemRootViewModel.GetFiles(PublishPackageViewModel.PackageContents.ToList());
            var count = publishedFiles.Count(x => x.DependencyType != DependencyType.Folder);

            this.filesUploadedMessage.Text = String.Format("{0} {1}", count.ToString(), Dynamo.Wpf.Properties.Resources.PackageManagerFinishedPackageFilesUploadedMessage);
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
    }
}
