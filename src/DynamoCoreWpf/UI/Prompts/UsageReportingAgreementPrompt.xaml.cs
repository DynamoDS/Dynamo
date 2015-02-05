using System.Windows.Media;
using Dynamo.Services;

using System.Windows;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for UsageReportingAgreementPrompt.xaml
    /// </summary>
    public partial class UsageReportingAgreementPrompt : Window
    {
        public UsageReportingAgreementPrompt(IBrandingResourceProvider resourceProvider)
        {
            InitializeComponent();
            ConsentFormImageRectangle.Fill = new ImageBrush(
                resourceProvider.GetImageSource(ResourceName.UsageConsentFormImage));
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            UsageReportingManager.Instance.SetUsageReportingAgreement(acceptCheck.IsChecked.Value);
            Close();
        }
    }
}
