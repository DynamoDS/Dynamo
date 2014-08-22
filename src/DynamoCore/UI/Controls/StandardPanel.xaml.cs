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

        private void OnListBoxItemMouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem from_sender = sender as ListBoxItem;
            libraryToolTipPopup.PlacementTarget = from_sender;
            libraryToolTipPopup.DataContext = from_sender.DataContext;
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Dynamo.UI.Controls.LibraryToolTipPopup.isMouseOver)
            libraryToolTipPopup.DataContext = null;
        }

        private void OnListBoxItemMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point point = Mouse.GetPosition(sender as IInputElement);
            FrameworkElement nodeSearchButton = sender as FrameworkElement;
            // We need to know whether mouse is inside tooltip.
            // But we need to know it before mouse will leave nodeSearchButton, that's why
            // we check where mouse pointer  is.
            if (point.X >= (nodeSearchButton.ActualWidth - 50)) // 50 is transparent width of tooltip at the left side.
            {
                if (point.Y <= (nodeSearchButton.ActualHeight - 3))
                // Leave 3 pixels for case, when user move mouse to the bottom, not to the right.
                {
                    Dynamo.UI.Controls.LibraryToolTipPopup.isMouseOver = true;
                }
            }
        }

    }
}
