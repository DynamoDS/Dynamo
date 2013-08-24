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

namespace Dynamo.Nodes.Prompts
{
    /// <summary>
    /// Interaction logic for CrashPrompt.xaml
    /// </summary>
    public partial class CrashPrompt : Window
    {
        public CrashPrompt(string promptText)
        {
            InitializeComponent();
            this.CrashContent.Text = promptText;
        }

        public CrashPrompt()
        {
            InitializeComponent();
            this.CrashContent.Text = "Unknown error";
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
