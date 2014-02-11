using System;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : Window
    {
        public PackageManagerPublishView(PublishPackageViewModel packageViewModel)
        {

            DataContext = packageViewModel;
            packageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            //this.Owner = DynamoSettings.Bench;
            Owner = WPF.FindUpVisualTree<DynamoView>(this);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            Dispatcher.BeginInvoke((Action) (Close));
        }
    }

}
