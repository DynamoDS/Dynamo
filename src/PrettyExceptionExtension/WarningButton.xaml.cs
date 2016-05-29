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

namespace Dynamo.WarningHelper
{
    /// <summary>
    /// Interaction logic for WarningButton.xaml
    /// </summary>
    public partial class WarningButton : Window
    {
        private List<Exception> exceptions;
        public WarningButton(List<Exception> exceptions)
        {
            this.exceptions = exceptions;
            InitializeComponent();
            WarningsExistMessage.Text = "Click here to see warnings";

        }

        private void DismissButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //foreach preload exception log the error or show a window
            foreach (var exception in this.exceptions)
            {
                var window = new WarningView(exception, DynamoApplications.Properties.Resources.MismatchedAssemblyVersionShortMessage);
                window.ShowDialog();
            }
            this.Close();
        }
    }
}
