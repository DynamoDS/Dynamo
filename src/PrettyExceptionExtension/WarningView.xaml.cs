using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for AssemblyLoadWarning.xaml
    /// </summary>
    public partial class WarningView : Window
    {
        private readonly Exception exception;
        private readonly string shortMessage;
        public bool doNotShowNextTime { get; set; }

        public WarningView(Exception exception, string shortMessage)
        {
            this.exception = exception;
            this.shortMessage = shortMessage;
            this.Title = exception.GetType().ToString();
            InitializeComponent();
            ShortErrorMessage.Text = shortMessage;
        }

        private void ShowDetails_ButtonClick(object sender, RoutedEventArgs e)
        {
            Details.Text = this.exception.Message;
            if (Details.Visibility == Visibility.Collapsed)
            {
                Details.Visibility = Visibility.Visible;
            }
            else
            {
                Details.Visibility = Visibility.Collapsed;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = this.doNotShowNextTime;
            this.Close();
        }

       
    }
}
