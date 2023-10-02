using System.Windows.Controls;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackageSelectPage.xaml
    /// </summary>
    public partial class PublishPackageSelectPage : Page
    {
        public PublishPackageSelectPage()
        {
            InitializeComponent();

            this.Tag = "Select Package Contents";
        }
    }
}
