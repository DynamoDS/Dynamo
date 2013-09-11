using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.UI.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        //private TitleBarButtons titleBarButtons;

        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.DataContext = pm;
            InitializeComponent();

            //if (titleBarButtons == null)
            //{
            //    titleBarButtons = new TitleBarButtons(this);
            //    titleBarButtonsGrid.Children.Add(titleBarButtons);
            //}
        }

        private void SearchTextBox_TextChanged(object sender, KeyEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchText = this.SearchTextBox.Text;

            if (e.Key == Key.Enter)
            {
                (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults();
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((PackageManagerSearchViewModel)DataContext).ExecuteSelected();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults();
        }
    }
}
