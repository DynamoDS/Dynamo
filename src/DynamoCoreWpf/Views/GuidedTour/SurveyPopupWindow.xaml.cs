using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for SurveyPopupWindow.xaml
    /// </summary>
    public partial class SurveyPopupWindow : Popup
    {
        private SurveyPopupViewModel surveyViewModel;

        public SurveyPopupWindow(SurveyPopupViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();
            if (viewModel != null)
                surveyViewModel = viewModel;

            DataContext = surveyViewModel;

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //When setting a value for offset it will move the popup taking as a initial position the host control position in which the popup is shown
            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }
}
