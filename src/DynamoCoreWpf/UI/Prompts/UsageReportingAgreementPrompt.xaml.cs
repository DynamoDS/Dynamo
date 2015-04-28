using System.Windows.Media;
using Dynamo.Services;

using System.Windows;
using Dynamo.Wpf.Interfaces;
using Dynamo.ViewModels;

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

            Message1TextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.AgreementOne);
            FeatureTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.FeatureUsage);
            NodeTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.NodeUsage);
            Message2TextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.AgreementTwo);
            Message3TextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.AgreementThree);

            AcceptUsageReportingCheck.IsChecked = UsageReportingManager.Instance.IsUsageReportingApproved;
            AcceptAnalyticsReportingCheck.IsChecked = UsageReportingManager.Instance.IsAnalyticsReportingApproved;

            viewModel = dynamoViewModel;
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
