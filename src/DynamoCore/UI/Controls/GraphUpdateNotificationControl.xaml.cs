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
            this.InstallButton.Click += new RoutedEventHandler(OnInstallButtonClicked);
        }

        private void OnInstallButtonClicked(object sender, RoutedEventArgs e)
        {
            //dynSettings.Controller.DynamoLogger.LogInfo("UpdateNotificationControl-OnInstallButtonClicked",
            //    "UpdateNotificationControl-OnInstallButtonClicked");

            //dynSettings.Controller.UpdateManager.QuitAndInstallUpdate(); // Quit application

            //Disable the update check for 0.6.3. Just send he user to the downloads page.
            //dynSettings.Controller.UpdateManager.CheckForProductUpdate();

            Process.Start("http://dyn-builds-pub.s3-website-us-west-2.amazonaws.com/");
        }
    }
}