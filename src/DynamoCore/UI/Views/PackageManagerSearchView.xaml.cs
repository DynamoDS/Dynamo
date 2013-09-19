using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {

            this.DataContext = pm;
            InitializeComponent();

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(this.SearchTextBox.Text);
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
