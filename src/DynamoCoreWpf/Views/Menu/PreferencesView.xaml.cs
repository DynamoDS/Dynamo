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

        public PreferencesWindow()
        {
            InitializeComponent();

            //If we want the PreferencesView window to be modal, we need to assign the owner (since we created a new Style and not following the common Style)
            this.Owner = Application.Current.MainWindow;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //When the TitleBar is clicked this method will be executed
        private void PreferencesPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}