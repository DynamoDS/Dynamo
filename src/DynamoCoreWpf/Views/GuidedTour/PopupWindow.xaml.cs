using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Popup
    {
        private PopupWindowViewModel popupViewModel;

        public PopupWindow(PopupWindowViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();
            
            if (viewModel != null)
                popupViewModel = viewModel;

            DataContext = popupViewModel;

            //Due that we are drawing the Direction pointers on left and right side of the Canvas (10 width each one) then we need to add 20
            RootLayout.Width = popupViewModel.Width + 20; 
            RootLayout.Height = popupViewModel.Height;

            //The BackgroundRectangle represent the tooltip and it need to left 10 at left and 10 at right to show the direction pointers
            BackgroundRectangle.Rect = new Rect(10, 0, popupViewModel.Width, popupViewModel.Height);

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //The CustomRichTextBox has a margin of 10 in left and 10 in right, also there is a indentation for drawing the PointerDirection of 10 of each side so that's why we are subtracting 40.
            ContentRichTextBox.Width = popupViewModel.Width - 40;
            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;
        }

        private void StartTourButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }
}
