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
            Title = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormTitle);

            ConsentFormImageRectangle.Fill = new ImageBrush(
                resourceProvider.GetImageSource(ResourceName.UsageConsentFormImage));

            Message1TextBlock.Text = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormMessage1);
            FeatureTextBlock.Text = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormFeatureUsage);
            NodeTextBlock.Text = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormNodeUsage);
            Message2TextBlock.Text = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormMessage2);
            ConsentTextBlock.Text = resourceProvider.GetUsageConsentDialogString(UsageConsentFormStringResource.UsageConsentFormConsent);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            UsageReportingManager.Instance.SetUsageReportingAgreement(acceptCheck.IsChecked.Value);
            Close();
        }
    }
}
