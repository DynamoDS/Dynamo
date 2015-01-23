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
<<<<<<< HEAD
            if (DataContext is UpdateManager)
            {
                var message = Dynamo.Wpf.Properties.Resources.UpdateMessage;

                var result = MessageBox.Show(message, Dynamo.Wpf.Properties.Resources.InstallMessageCaption, MessageBoxButton.OKCancel);
=======
            if (!(DataContext is UpdateManager)) return;

            var result = MessageBox.Show(Dynamo.Wpf.Resource1.UpdateNotificationString, 
                Dynamo.Wpf.Resource1.UpdateNotificationTitle, 
                MessageBoxButton.OKCancel);
>>>>>>> 26050bfd46c355f8ea2c00f3bbc0daa5ddd6b117

            if (result == MessageBoxResult.OK)
            {
                UpdateManager.Instance.QuitAndInstallUpdate();
            }
        }
    }
}