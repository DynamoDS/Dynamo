using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
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
            dynSettings.DynamoLogger.Log("UpdateNotificationControl-OnInstallButtonClicked",
                "UpdateNotificationControl-OnInstallButtonClicked");

            dynSettings.Controller.UpdateManager.QuitAndInstallUpdate(); // Quit application

        }
    }
}