using System.Windows;
using System.Windows.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : Window
    {
        public PackageManagerPublishView(PackageManagerPublishViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            this.Owner = dynSettings.Bench;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.Title.Focus();
        }
    }

}
