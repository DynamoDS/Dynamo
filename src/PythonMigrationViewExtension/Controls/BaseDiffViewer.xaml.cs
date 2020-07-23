using System.Windows;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for BaseDiffViewer.xaml
    /// </summary>
    public partial class BaseDiffViewer : Window
    {
        private PythonMigrationAssistantViewModel ViewModel { get; set; }

        internal BaseDiffViewer(PythonMigrationAssistantViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        private void OnDiffButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentViewModel.ViewMode == Differ.ViewMode.Inline)
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.SideBySide);
                Width = 1200;
            }

            else
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.Inline);
                Width = 600;
            }
        }

        private void OnAcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeCode();
            this.Close();
        }

        private void OnRejectButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
