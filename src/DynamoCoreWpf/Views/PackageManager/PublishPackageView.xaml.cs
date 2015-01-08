using System;
using System.Windows;

using Dynamo.Controls;
using Dynamo.PackageManager.UI;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;

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
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            this.Dispatcher.BeginInvoke((Action) (Close));
        }
    }

}
