using System.Windows;
using System.Windows.Input;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {

        public PreferencesWindow(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();         
            DataContext = dynamoViewModel;

            //For doing this window modal
            this.Owner = Application.Current.MainWindow;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();              
            }
        }
    }
}