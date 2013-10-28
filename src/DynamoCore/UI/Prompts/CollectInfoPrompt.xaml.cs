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
    /// Interaction logic for DataConsentPrompt.xaml
    /// </summary>
    public partial class CollectInfoPrompt : Window
    {
        public bool CollectDataConsent { get; private set; }

        public CollectInfoPrompt()
        {
            InitializeComponent();
            this.CollectDataConsent = false;
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Close();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.CollectDataConsent = false;
            if (acceptCheck.IsChecked.HasValue)
                this.CollectDataConsent = acceptCheck.IsChecked.Value;
        }
    }
}
