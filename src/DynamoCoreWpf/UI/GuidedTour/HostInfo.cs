using System.Windows;
using System.Windows.Controls.Primitives;

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
        /// Host Class, this property will contain the class of the host control located in the TreeView
        /// </summary>
        public string HostClass
        {
            get
            {
                return hostClass;
            }
            set
            {
                hostClass = value;
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
        /// This property will hold the placement location of the popup, for now we are just using Right, Left, Top and Bottom
        /// </summary>
        public PlacementMode PopupPlacement { get; set; }

        /// <summary>
        /// Once the popup host control and placecement is set we can use this property for moving the popup location Vertically (by specifying an offset) 
        /// </summary>
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
        public double HorizontalPopupOffSet
        {
            get
            {
                return horizontalPopupOffSet;
            }
            set
            {
                horizontalPopupOffSet  = value;
            }
        }
    }
}
