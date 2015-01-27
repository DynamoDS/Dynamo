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
            if (!(DataContext is UpdateManager)) return;

            var result = MessageBox.Show(Dynamo.Wpf.Resource1.UpdateNotificationString, 
                Dynamo.Wpf.Resource1.UpdateNotificationTitle, 
                MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                UpdateManager.Instance.QuitAndInstallUpdate();
            }
        }
    }
}