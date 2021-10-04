using System.Windows.Controls;

namespace Dynamo.PackageDetails
{
    public partial class PackageDetailsView : UserControl
    {
        public PackageDetailsViewModel PackageDetailsViewModel { get; }
        public PackageDetailsView(PackageDetailsViewModel packageDetailsViewModel)
        {
            InitializeComponent();

            this.PackageDetailsViewModel = packageDetailsViewModel;
        }
    }
}