using Dynamo.Services;
using System.Windows;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for UsageReportingAgreementPrompt.xaml
    /// </summary>
    public partial class UsageReportingAgreementPrompt : Window
    {
        public UsageReportingAgreementPrompt()
        {
            InitializeComponent();
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
            UsageReportingManager.Instance.SetUsageReportingAgreement(acceptCheck.IsChecked.Value);
            Close();
        }
    }
}
