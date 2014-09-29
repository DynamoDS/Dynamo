using System;
using System.Windows;

using Dynamo.Controls;
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

            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            this.Dispatcher.BeginInvoke((Action) (Close));
        }
    }

}
