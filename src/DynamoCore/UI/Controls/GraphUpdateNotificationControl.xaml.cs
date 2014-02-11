using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Core;
using Dynamo.Utilities;

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
            InstallButton.Click += new RoutedEventHandler(OnInstallButtonClicked);
        }

        private void OnInstallButtonClicked(object sender, RoutedEventArgs e)
        {
            DynamoLogger.Instance.LogInfo("UpdateNotificationControl-OnInstallButtonClicked",
                "UpdateNotificationControl-OnInstallButtonClicked");

            DynamoSettings.Controller.UpdateManager.QuitAndInstallUpdate(); // Quit application
        }
    }
}