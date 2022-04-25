using System;
using System.Windows;
using System.Windows.Interop;
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
            var googleAnalyticsFile = "GoogleAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref googleAnalyticsFile))
                GoogleAnalyticsConsent.File = googleAnalyticsFile;
            var adpAnalyticsFile = "ADPAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref adpAnalyticsFile))
                ADPAnalyticsConsent.File = adpAnalyticsFile;

            //disable adp configure dialog version check fails.
            //also disabled below id all analytics disabled.
            configure_adp_button.IsEnabled = AnalyticsService.IsADPAvailable();

            AcceptGoogleAnalyticsCheck.IsChecked = UsageReportingManager.Instance.IsAnalyticsReportingApproved;

            if (Analytics.DisableAnalytics)
            {
                AcceptGoogleAnalyticsCheck.IsChecked = false;
                AcceptGoogleAnalyticsTextBlock.IsEnabled = false;
                AcceptGoogleAnalyticsCheck.IsEnabled = false;

                configure_adp_button.IsEnabled = false;
            }
           
        }

        private void ToggleIsUsageReportingChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetUsageReportingAgreement(
                AcceptUsageReportingCheck.IsChecked.HasValue &&
                AcceptUsageReportingCheck.IsChecked.Value);
            AcceptUsageReportingCheck.IsChecked = UsageReportingManager.Instance.IsUsageReportingApproved;
        }

        private void ToggleIsGoogleAnalyticsChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(
                AcceptGoogleAnalyticsCheck.IsChecked.HasValue &&
                AcceptGoogleAnalyticsCheck.IsChecked.Value);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement for GA.
            //ADP is set via the ADPSDK consent window.
            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(AcceptGoogleAnalyticsCheck.IsChecked.Value);
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

        private void configure_adp_button_Click(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new IntPtr();
            if (Owner != null)
            {
                handle = new WindowInteropHelper(Owner).Handle;
            }
            AnalyticsService.ShowADPConsetDialog(handle);
        }
    }
}