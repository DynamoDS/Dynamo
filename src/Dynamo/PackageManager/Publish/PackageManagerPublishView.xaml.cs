using System.Windows.Controls;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : UserControl
    {

        public PackageManagerPublishView(PackageManagerPublishViewModel viewModel)
        {

            InitializeComponent();
            this.DataContext = viewModel;
        }

    }

}
