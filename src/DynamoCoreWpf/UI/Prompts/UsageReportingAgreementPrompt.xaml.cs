using System.Windows;
using System.Windows.Media;
using Dynamo.Logging;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceProvider"></param>
        /// <param name="dynamoViewModel"></param>
        public UsageReportingAgreementPrompt(IBrandingResourceProvider resourceProvider, DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            if (resourceProvider != null)
            {
                Title = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Title);
                ConsentFormImageRectangle.Fill = new ImageBrush(
                    resourceProvider.GetImageSource(Wpf.Interfaces.ResourceNames.ConsentForm.Image));
            }
            viewModel = dynamoViewModel;

            var adpAnalyticsFile = "ADPAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref adpAnalyticsFile))
                ADPAnalyticsContent.File = adpAnalyticsFile;

            var googleAnalyticsFile = "GoogleAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref googleAnalyticsFile))
                GoogleAnalyticsContent.File = googleAnalyticsFile;

            AcceptADPReportingCheck.IsChecked = Analytics.ReportingADPAnalytics;
            if (!Configuration.DebugModes.IsEnabled("ADPAnalyticsTracker"))
            {
                AcceptADPReportingCheck.Visibility = Visibility.Hidden;
                ADPAnalyticsViewer.Visibility = Visibility.Hidden;
            }
            
            AcceptAnalyticsReportingCheck.IsChecked = UsageReportingManager.Instance.IsAnalyticsReportingApproved;
        }

        private void ToggleIsADPReportingChecked(object sender, RoutedEventArgs e)
        {
            Analytics.ReportingADPAnalytics = (
                AcceptADPReportingCheck.IsChecked.HasValue &&
                AcceptADPReportingCheck.IsChecked.Value);
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
            Analytics.ReportingADPAnalytics = AcceptADPReportingCheck.IsChecked.Value;
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
