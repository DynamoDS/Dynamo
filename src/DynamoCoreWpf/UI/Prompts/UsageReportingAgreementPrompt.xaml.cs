using System.Windows;
using System.Windows.Media;
using Dynamo.Services;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for UsageReportingAgreementPrompt.xaml
    /// </summary>
    public partial class UsageReportingAgreementPrompt : Window
    {
        private DynamoViewModel viewModel = null;
        public UsageReportingAgreementPrompt(IBrandingResourceProvider resourceProvider, DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            Title = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Title);

            ConsentFormImageRectangle.Fill = new ImageBrush(
                resourceProvider.GetImageSource(Wpf.Interfaces.ResourceNames.ConsentForm.Image));

            viewModel = dynamoViewModel;

            var instrumentationFile = "InstrumentationConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref instrumentationFile))
                InstrumentationContent.File = instrumentationFile;

            var googleAnalyticsFile = "GoogleAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref googleAnalyticsFile))
                GoogleAnalyticsContent.File = googleAnalyticsFile;

            AcceptUsageReportingTextBlock.Text =
                string.Format(Wpf.Properties.Resources.ConsentFormInstrumentationCheckBoxContent,
                    dynamoViewModel.BrandingResourceProvider.ProductName);
            AcceptUsageReportingCheck.IsChecked = UsageReportingManager.Instance.IsUsageReportingApproved;
            AcceptAnalyticsReportingCheck.IsChecked = UsageReportingManager.Instance.IsAnalyticsReportingApproved;

        }

        private void ToggleIsUsageReportingChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetUsageReportingAgreement(
                AcceptUsageReportingCheck.IsChecked.HasValue && 
                AcceptUsageReportingCheck.IsChecked.Value);
        }

        private void ToggleIsAnalyticsReportingChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(
                AcceptAnalyticsReportingCheck.IsChecked.HasValue && 
                AcceptAnalyticsReportingCheck.IsChecked.Value);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            UsageReportingManager.Instance.SetUsageReportingAgreement(AcceptUsageReportingCheck.IsChecked.Value);
            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(AcceptAnalyticsReportingCheck.IsChecked.Value);
            Close();
        }

        private void OnLearnMoreClick(object sender, RoutedEventArgs e)
        {
            var aboutBox = viewModel.BrandingResourceProvider.CreateAboutBox(viewModel);
            aboutBox.Owner = this;
            aboutBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutBox.ShowDialog();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            viewModel = null;
        }
    }
}
