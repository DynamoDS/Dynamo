using System.Windows;
using System.Windows.Controls;

using Dynamo.UpdateManager;

namespace DynamoCore.UI.Controls
{
    /// <summary>
    /// Interaction logic for GraphUpdateNotificationControl.xaml
    /// </summary>
    public partial class GraphUpdateNotificationControl : UserControl
    {
       public GraphUpdateNotificationControl()
        {
            InitializeComponent();
            InstallButton.Click += OnInstallButtonClicked;
        }

        private void OnInstallButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is UpdateManager)
            {
                var message = string.Format("An update is available for {0}.\n\n" +
                    "Click OK to close {0} and install\nClick CANCEL to cancel the update.", "Dynamo");

                var result = MessageBox.Show(message, "Install Dynamo", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    UpdateManager.Instance.QuitAndInstallUpdate();
                }
            }
        }
    }
}