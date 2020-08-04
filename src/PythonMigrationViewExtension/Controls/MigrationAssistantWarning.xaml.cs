using System.Windows;
using System.Windows.Controls;
using Dynamo.PythonMigration.MigrationAssistant;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for MigrationAssistantWarning.xaml
    /// </summary>
    public partial class MigrationAssistantWarning : Window
    {
        internal bool WarningAccepted { get; private set; }
        private PythonMigrationAssistantViewModel ViewModel { get; set; }

        internal MigrationAssistantWarning(PythonMigrationAssistantViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void DeclineBtn_Click(object sender, RoutedEventArgs e)
        {
            WarningAccepted = false;
            this.Close();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            WarningAccepted = true;
            this.Close();
        }

        private void DisableIronPythonDialogCheck_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DisableMigrationAssistanWarning();
        }
    }
}
