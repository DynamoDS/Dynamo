using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

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
