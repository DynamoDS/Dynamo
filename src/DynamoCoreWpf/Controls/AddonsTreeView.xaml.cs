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

            // In future DataContext will be set SearchViewModel in binding, but for now set it to null. 
            // Therefore we can escape errors in VS Output.
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
