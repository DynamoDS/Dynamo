using System.Windows;
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
        /// This method is executed when the Close button in the RealTimeInfo window is pressed, so we clean the subscriptions to events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CleanRealTimeInfoWindow();
        }

        /// <summary>
        /// This method will update the current location of the RealTimeInfo window due that probably the window was moved or resized
        /// </summary>
        public void UpdateLocation()
        {
            if (IsOpen == true)
            {
                //This section will update the Popup location by calling the private method UpdatePosition using reflection
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }
        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
                BorderLine.Y2 = PopupGrid.ActualHeight /*+ ((TextBlock)sender).Margin.Bottom*/;
        }


    }
}
