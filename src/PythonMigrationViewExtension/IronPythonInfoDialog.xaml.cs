using System.Windows;

namespace Dynamo.PythonMigration
{
    /// <summary>
    /// Interaction logic for IronPythonInfoDialog.xaml
    /// </summary>
    public partial class IronPythonInfoDialog : Window
    {
        PythonMigrationViewExtension ViewModel { get; set; }
        internal IronPythonInfoDialog(PythonMigrationViewExtension viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnMoreInformationButtonClicked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OpenPythonMigrationWarningDocumentation();
            this.Close();
        }
    }
}
