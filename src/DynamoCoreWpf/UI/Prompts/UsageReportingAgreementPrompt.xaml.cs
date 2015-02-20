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
            Title = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Title);

            ConsentFormImageRectangle.Fill = new ImageBrush(
                resourceProvider.GetImageSource(Wpf.Interfaces.ResourceNames.ConsentForm.Image));

            Message1TextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.AgreementOne);
            FeatureTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.FeatureUsage);
            NodeTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.NodeUsage);
            Message2TextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.AgreementTwo);
            ConsentTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Consent);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            UsageReportingManager.Instance.SetUsageReportingAgreement(acceptCheck.IsChecked.Value);
            Close();
        }
    }
}
