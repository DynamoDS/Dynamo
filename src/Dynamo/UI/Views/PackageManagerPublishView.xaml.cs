using System.Windows;
using System.Windows.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : UserControl
    {
        private PackageManagerPublishViewModel viewModel;

        public PackageManagerPublishView()
        {
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(PackageManagerPublishView_Loaded);
        }

        void PackageManagerPublishView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel = dynSettings.Controller.PackageManagerPublishViewModel;
            DataContext = viewModel;

            viewModel.RequestHidePackageManagerPublish += new System.EventHandler(viewModel_RequestHidePackageManagerPublish);
        }

        void viewModel_RequestHidePackageManagerPublish(object sender, System.EventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

    }

}
