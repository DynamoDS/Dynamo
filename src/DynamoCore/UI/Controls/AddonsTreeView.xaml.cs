using System.Windows;
using System.Windows.Controls;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for AddonsTreeView.xaml
    /// </summary>
    public partial class AddonsTreeView : UserControl
    {
        public AddonsTreeView()
        {
            InitializeComponent();
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            // Logic of original TreeView should be saved until
            // new design is not implemented.
#if false
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var element = menuItem.DataContext as CustomNodeSearchElement;
                if (element != null)
                {
                    if (dynamoViewModel.OpenCommand.CanExecute(element.Path))
                        dynamoViewModel.OpenCommand.Execute(element.Path);
                }
            }
#endif
        }
    }
}
