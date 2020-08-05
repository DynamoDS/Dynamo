using System.Windows;
using System.Windows.Controls;
using Dynamo.PythonMigration.MigrationAssistant;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for MigrationAssistantWarning.xaml
    /// </summary>
    internal partial class MigrationAssistantWarning : Window
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
            if (DisableMigrationAssitantWarningDialogCheck.IsChecked ?? false)
                ViewModel.DisableMigrationAssistanWarning();

            this.Close();
        }
    }
}
