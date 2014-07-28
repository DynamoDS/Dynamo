using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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
            if (DataContext is DynamoViewModel)
            {
                var dvm = DataContext as DynamoViewModel;

                dvm.Model.Logger.Log("UpdateNotificationControl-OnInstallButtonClicked","UpdateNotificationControl-OnInstallButtonClicked");
                dvm.Model.UpdateManager.QuitAndInstallUpdate(); // Quit application
            }
        }
    }
}