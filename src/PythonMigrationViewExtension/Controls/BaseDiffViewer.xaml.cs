using Dynamo.PythonMigration.MigrationAssistant;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for BaseDiffViewer.xaml
    /// </summary>
    public partial class BaseDiffViewer : Window
    {
        private PythonMigrationAssistantViewModel ViewModel { get; set; }

        // we would like to match initial text wrapping of code inside diff view
        // to that of the code inside the script editor window as much as possible
        private int scriptEditorWindowDefaultWidth = 600;
        private int differAdditionalWidthPerPanel = 20;

        internal BaseDiffViewer(PythonMigrationAssistantViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            // The diff viewer is initialized with the SideBySide view
            SetSideBySideWindowWidth();
            InitializeComponent();
        }

        private void OnDiffButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentViewModel.ViewMode == Differ.ViewMode.Inline)
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.SideBySide);
                SetSideBySideWindowWidth();
            }
            else
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.Inline);
                SetInlineWindowWidth();
            }
        }

        private void SetSideBySideWindowWidth()
        {
            Width = scriptEditorWindowDefaultWidth * 2 + differAdditionalWidthPerPanel * 1.5;
        }

        private void SetInlineWindowWidth()
        {
            Width = scriptEditorWindowDefaultWidth + differAdditionalWidthPerPanel;
        }

        private void OnAcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.MigrationAssistantSettingsFileExists())
            {
                var warningMessage = new MigrationAssistantWarning(ViewModel);
                warningMessage.ShowDialog();
                if (!warningMessage.WarningAccepted)
                {
                    this.Close();
                    return;
                }
            }

            ViewModel.ChangeCode();
            this.Close();
        }

        private void OnRejectButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
