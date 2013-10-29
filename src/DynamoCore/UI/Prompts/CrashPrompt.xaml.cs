using System.Windows;
using Dynamo.Utilities;

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

        private void PostOnGithub_Click(object sender, RoutedEventArgs e)
        {
            dynSettings.Controller.ReportABug(null);
        }

    }
}
