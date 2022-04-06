using System.Collections.Generic;
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
        private UIElement hostUIElement;
        private double verticalPopupOffSet;
        private double horizontalPopupOffSet;
        private HtmlPage htmlPage;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal HostControlInfo() { }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="copyInstance"></param>
        /// <param name="MainWindow"></param>
        internal HostControlInfo(HostControlInfo copyInstance, UIElement MainWindow)
        {
            PopupPlacement = copyInstance.PopupPlacement;
            HostUIElementString = copyInstance.HostUIElementString;
            HostUIElement = MainWindow;
            VerticalPopupOffSet = copyInstance.VerticalPopupOffSet;
            HorizontalPopupOffSet = copyInstance.HorizontalPopupOffSet;
            HtmlPage = copyInstance.HtmlPage;
            WindowName = copyInstance.WindowName;
            DynamicHostWindow = copyInstance.DynamicHostWindow;
        }

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
        /// Property that represents the highlight rectangle that will be shown over the Overlay
        /// </summary>
        [JsonProperty("HighlightRectArea")]
        internal HighlightArea HighlightRectArea { get; set; }

        /// <summary>
        /// Property that represents the cut off section that will be removed from the Overlay
        /// </summary>
        [JsonProperty("CutOffRectArea")]
        internal CutOffArea CutOffRectArea { get; set; }

        /// <summary>
        /// The html page that is going to be rendered inside the popup
        /// </summary>
        [JsonProperty("HtmlPage")]
        public HtmlPage HtmlPage { get => htmlPage; set => htmlPage = value; }

        /// <summary>
        /// This property will contain the WPF Window Name for the cases when the Popup need to be located in a different Window than DynamoView
        /// </summary>
        [JsonProperty(nameof(WindowName))]
        internal string WindowName { get; set; }

        /// <summary>
        /// This property will decide if the Popup.PlacementTarget needs to be calculated again or not (probably after UI Automation a new Window was opened)
        /// </summary>
        [JsonProperty(nameof(DynamicHostWindow))]
        internal bool DynamicHostWindow { get; set; }
    }

    public class HtmlPage
    {
        private string fileName;
        private Dictionary<string,string> resources;

        /// <summary>
        /// A dictionary containing the key word to be replaced in page and the filename as values
        /// </summary>
        [JsonProperty("Resources")]
        public Dictionary<string, string> Resources { get => resources; set => resources = value; }

        /// <summary>
        /// Filename of the HTML page
        /// </summary>
        [JsonProperty("FileName")]
        public string FileName { get => fileName; set => fileName = value; }
    }
}
