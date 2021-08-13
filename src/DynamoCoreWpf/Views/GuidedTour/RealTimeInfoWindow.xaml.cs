using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for RealTimeInfoWindow.xaml
    /// </summary>
    public partial class RealTimeInfoWindow : Popup
    {

        /// <summary>
        /// This property contains the text that will be shown in the popup and it can be updated on runtime.
        /// </summary>
        public string TextContent { get; set; }

        public RealTimeInfoWindow()
        {
            InitializeComponent();

            DataContext = this;

            GuideFlowEvents.GuidedTourFinish += GuideFlowEvents_GuidedTourFinish;
            GuideFlowEvents.GuidedTourStart += GuideFlowEvents_GuidedTourStart;
        }

        private void CleanRealTimeInfoWindow()
        {
            IsOpen = false;
            GuideFlowEvents.GuidedTourFinish -= GuideFlowEvents_GuidedTourFinish;
            GuideFlowEvents.GuidedTourStart -= GuideFlowEvents_GuidedTourStart;
        }

        private void GuideFlowEvents_GuidedTourStart(GuidedTourStateEventArgs args)
        {
            CleanRealTimeInfoWindow();
        }

        private void GuideFlowEvents_GuidedTourFinish(GuidedTourStateEventArgs args)
        {
            CleanRealTimeInfoWindow();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CleanRealTimeInfoWindow();
        }
    }
}
