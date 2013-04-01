using System.Windows.Controls;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishUI.xaml
    /// </summary>
    public partial class PackageManagerPublishUI : UserControl
    {

        public PackageManagerPublishUI(PackageManagerPublishViewModel viewModel)
        {

            InitializeComponent();
            this.DataContext = viewModel;
        }

    }

}
