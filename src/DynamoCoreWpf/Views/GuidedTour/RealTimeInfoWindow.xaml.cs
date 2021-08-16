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

        /// <summary>
        /// This method remove the existing subscription to events and close the current RealTimeInfo window
        /// </summary>
        /// <param name="args"></param>
        private void GuideFlowEvents_GuidedTourStart(GuidedTourStateEventArgs args)
        {
            CleanRealTimeInfoWindow();
        }

        /// <summary>
        /// When the Tour has finished we need to close the RealTimeInfo window and remove subscriptions to events
        /// </summary>
        /// <param name="args"></param>
        private void GuideFlowEvents_GuidedTourFinish(GuidedTourStateEventArgs args)
        {
            CleanRealTimeInfoWindow();
        }

        /// <summary>
        /// This methos is executed when the Close button in the RealTimeInfo window is pressed, so we clean the subscriptions to events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CleanRealTimeInfoWindow();
        }
    }
}
