﻿using System;
using System.Windows;

using Dynamo.Controls;
using Dynamo.PackageManager.UI;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PublishPackageView.xaml
    /// </summary>
    public partial class PublishPackageView : Window
    {
        public PublishPackageView(PublishPackageViewModel packageViewModel)
        {
            this.DataContext = packageViewModel;
            packageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            InitializeComponent();

            Title = string.Format(Wpf.Properties.Resources.PublishPackageViewTitle,
                packageViewModel.DynamoViewModel.BrandingResourceProvider.ProductName);
            packageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            this.Dispatcher.BeginInvoke((Action) (Close));
        }

        private void OnRequestShowFolderBrowserDialog(object sender, PackagePathEventArgs e)
        {
            e.Cancel = true;
            // Handle for the case, initialPath does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(e.Path);
            if(errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    SelectedPath = e.Path,
                    Owner = this
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    e.Cancel = false;
                    e.Path = dialog.SelectedPath;
                }
            }
           
        }
    }

}
