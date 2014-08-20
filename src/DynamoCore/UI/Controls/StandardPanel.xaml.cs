using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Nodes.Search;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for StandardPanel.xaml
    /// </summary>
    public partial class StandardPanel : UserControl
    {
        public StandardPanel()
        {
            InitializeComponent();
        }
        private void OnActionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;
            action.FontWeight = FontWeights.UltraBold;
            query.FontWeight = FontWeights.Normal;
        }

        private void OnQueryMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;
            action.FontWeight = FontWeights.Normal;
            query.FontWeight = FontWeights.UltraBold;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;
        }

        private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem from_sender = sender as ListBoxItem;
            libraryToolTipPopup.PlacementTarget = from_sender;
            libraryToolTipPopup.DataContext = from_sender.DataContext;
        }

        private void StandardPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Dynamo.UI.Controls.LibraryToolTipPopup.MouseInside)
            {
                Dynamo.UI.Controls.LibraryToolTipPopup.MouseInside = false;
            }
            else
            {
                libraryToolTipPopup.DataContext = null;
            }
        }
    }
}
