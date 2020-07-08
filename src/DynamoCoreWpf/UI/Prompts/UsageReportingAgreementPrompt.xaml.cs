using System.Windows;
using System.Windows.Media;
using Dynamo.Logging;
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
                InstrumentationContent.File = adpAnalyticsFile;

            AcceptADPAnalyticsTextBlock.Text =
                string.Format(Wpf.Properties.Resources.ConsentFormADPAnalyticsCheckBoxContent,
                    dynamoViewModel.BrandingResourceProvider.ProductName);
            AcceptADPAnalyticsCheck.Visibility = System.Windows.Visibility.Visible;
            AcceptADPAnalyticsCheck.IsChecked = AnalyticsService.IsADPOptedIn;
        }

        private void ToggleIsADPAnalyticsChecked(object sender, RoutedEventArgs e)
        {
            AnalyticsService.IsADPOptedIn = (
                AcceptADPAnalyticsCheck.IsChecked.HasValue &&
                AcceptADPAnalyticsCheck.IsChecked.Value);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            AnalyticsService.IsADPOptedIn = AcceptADPAnalyticsCheck.IsChecked.Value;
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
