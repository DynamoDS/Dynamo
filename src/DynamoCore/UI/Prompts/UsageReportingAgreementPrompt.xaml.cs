using Dynamo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
