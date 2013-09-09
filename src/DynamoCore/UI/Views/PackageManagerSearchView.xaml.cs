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
        private TitleBarButtons titleBarButtons;

        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.DataContext = pm;
            InitializeComponent();

            if (titleBarButtons == null)
            {
                titleBarButtons = new TitleBarButtons(this);
                titleBarButtonsGrid.Children.Add(titleBarButtons);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(((TextBox)sender).Text);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((PackageManagerSearchViewModel)DataContext).ExecuteSelected();
        }

    }
}
