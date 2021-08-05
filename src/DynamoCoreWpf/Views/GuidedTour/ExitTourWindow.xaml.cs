using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for ExitTourWindow.xaml
    /// </summary>
    public partial class ExitTourWindow : Popup
    {

        /// <summary>
        /// This property hold a reference to the Step that was created (can be Welcome, Survey, Tooltip, ExitTour). 
        /// </summary>
        public Step Step { get; set; }

        public ExitTourWindow(Step popupType, HostControlInfo hInfo )
        {
            InitializeComponent();

            Step = popupType;

            DataContext = Step;

            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;         
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }
}
