using System.Windows;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will contain info related to the popup location and the host control in which the popup will appear over
    /// </summary>
    public class HostControlInfo
    {
        private string name;
        private string hostClass;
        private UIElement hostUIElement;
        private double verticalPopupOffSet;
        private double horizontalPopupOffSet;
        private string htmlPage;
        private double widthBoxDelta;
        private double heightBoxDelta;

        /// <summary>
        /// Host Name, this property will contain the name of the host control located in the TreeView
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// This property will hold the UI Element over which the popup will be shown (it should be any item in the TreeView) 
        /// </summary>
        public UIElement HostUIElement
        {
            get
            {
                return hostUIElement;
            }
            set
            {
                hostUIElement = value;
            }
        }

        /// <summary>
        /// This variable will hold the name of the host (UIElement) in a string representation
        /// </summary>
        [JsonProperty("HostUIElementString")]
        public string HostUIElementString { get; set; }

        /// <summary>
        /// This property will hold the placement location of the popup, for now we are just using Right, Left, Top and Bottom
        /// </summary>
        [JsonProperty("PopupPlacement")]
        public PlacementMode PopupPlacement { get; set; }

        /// <summary>
        /// This property will highlight the clickable area if its set to true
        /// </summary>
        [JsonProperty("HighlightColor")]
        public string HighlightColor { get; set; }

        /// <summary>
        /// Once the popup host control and placecement is set we can use this property for moving the popup location Vertically (by specifying an offset) 
        /// </summary>
        [JsonProperty("VerticalPopupOffset")]
        public double VerticalPopupOffSet
        {
            get
            {
                return verticalPopupOffSet;
            }
            set
            {
                verticalPopupOffSet = value;
            }
        }

        /// <summary>
        /// Once the popup host control and placecement is set we can use this property for moving the popup location Horizontally (by specifying an offset) 
        /// </summary>
        [JsonProperty("HorizontalPopupOffset")]
        public double HorizontalPopupOffSet
        {
            get
            {
                return horizontalPopupOffSet;
            }
            set
            {
                horizontalPopupOffSet = value;
            }
        }

        /// <summary>
        /// Since the box that highlights the elements has its size fixed, this variable applies a value to fix its Width
        /// </summary>
        [JsonProperty("WidthBoxDelta")]
        public double WidthBoxDelta { get => widthBoxDelta; set => widthBoxDelta = value; }
        /// <summary>
        /// Since the box that highlights the elements has its size fixed, this variable applies a value to fix its Height
        /// </summary>
        [JsonProperty("HeightBoxDelta")]
        public double HeightBoxDelta { get => heightBoxDelta; set => heightBoxDelta = value; }

        /// <summary>
        /// The html page anme that is going to be rendered inside the popup
        /// </summary>
        [JsonProperty("HtmlPage")]
        public string HtmlPage { get => htmlPage; set => htmlPage = value; }
    }
}
