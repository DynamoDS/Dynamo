using System.Windows;
using System.Windows.Controls;

using Dynamo.Updates;
using Dynamo.Wpf.Utilities;

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
            var um = DataContext as IUpdateManager;
            if (um == null) return;

            var result = MessageBoxService.Show(Dynamo.Wpf.Properties.Resources.UpdateMessage, 
                Dynamo.Wpf.Properties.Resources.InstallMessageCaption, 
                MessageBoxButton.OKCancel,
                MessageBoxImage.None);

            if (result == MessageBoxResult.OK)
            {
                um.QuitAndInstallUpdate();
            }
        }
    }
}