using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search.SearchElements;

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

            // Invalidate the DataContext here because it will be set at a later 
            // time through data binding expression. This way debugger will not 
            // display warnings for missing properties.
            this.DataContext = null;
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        private void OnMemberMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement fromSender = sender as FrameworkElement;
            if (fromSender.DataContext is NodeSearchElement)
            {
                libraryToolTipPopup.PlacementTarget = fromSender;
                libraryToolTipPopup.SetDataContext(fromSender.DataContext);
            }
        }
    }
}
