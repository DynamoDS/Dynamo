using System.Windows;
using System.Windows.Controls;
using Dynamo.PythonMigration.MigrationAssistant;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for MigrationAssistantWarning.xaml
    /// </summary>
    internal partial class MigrationAssistantDisclaimer : Window
    {
        internal bool DisclaimerAccepted { get; private set; }
        private PythonMigrationAssistantViewModel ViewModel { get; set; }

        internal MigrationAssistantDisclaimer(PythonMigrationAssistantViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void DeclineBtn_Click(object sender, RoutedEventArgs e)
        {
            DisclaimerAccepted = false;
            this.Close();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            DisclaimerAccepted = true;
            if (DisableMigrationAssitantDisclaimerDialogCheck.IsChecked ?? false)
                ViewModel.DisableMigrationAssistantDisclaimer();

            this.Close();
        }
    }
}
