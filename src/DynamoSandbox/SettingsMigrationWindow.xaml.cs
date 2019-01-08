using System.Windows;

namespace Dynamo.DynamoSandbox
{
    /// <summary>
    /// Interaction logic for SettingsMigrationWindow.xaml
    /// </summary>
    public partial class SettingsMigrationWindow : Window
    {
        public SettingsMigrationWindow()
        {
            InitializeComponent();

            TitleBox.Text = Properties.Resources.SettingsMigrationDialogTitle;
            TextBoxPrompt.Text = Properties.Resources.SettingsMigrationDialogMessage;
        }
    }
}
