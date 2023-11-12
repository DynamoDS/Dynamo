using System.Windows;
using Dynamo.Core;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public class HighlightArea : NotificationObject
    {
        #region Private properties
        private double widthBoxDelta;
        private double heightBoxDelta;
        private double highlightRectWidth;
        private double highlightRectHeight;
        private double highlightRectCanvasTop;
        private double highlightRectCanvasLeft;
        private UIElement windowElementName;
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        internal HighlightArea() { }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="copyInstance">Instance with the HighlightArea info</param>
        internal HighlightArea(HighlightArea copyInstance) 
        {
            HighlightColor = copyInstance.HighlightColor;
            WidthBoxDelta = copyInstance.WidthBoxDelta;
            HeightBoxDelta = copyInstance.HeightBoxDelta;
            WindowName = copyInstance.WindowName;
            WindowElementNameString = copyInstance.WindowElementNameString;
            UIElementTypeString = copyInstance.UIElementTypeString;
            UIElementGridContainer = copyInstance.UIElementGridContainer;
        }

        /// <summary>
        /// Since the box that highlights the elements has its size fixed, this variable applies a value to fix its Width
        /// </summary>
        [JsonProperty(nameof(WidthBoxDelta))]
        public double WidthBoxDelta { get => widthBoxDelta; set => widthBoxDelta = value; }


        /// <summary>
        /// Since the box that highlights the elements has its size fixed, this variable applies a value to fix its Height
        /// </summary>
        [JsonProperty(nameof(HeightBoxDelta))]
        public double HeightBoxDelta { get => heightBoxDelta; set => heightBoxDelta = value; }

        /// <summary>
        /// This property will highlight the clickable area if its set to true
        /// </summary>
        [JsonProperty(nameof(HighlightColor))]
        public string HighlightColor { get; set; }

        /// <summary>
        /// This represent the UIElement of the VisualTree that will be highlighted (according to the json file)
        /// </summary>
        public UIElement WindowElementName
        {
            get
            {
                return windowElementName;
            }
            set
            {
                windowElementName = value;
                RaisePropertyChanged(nameof(WindowElementName));
            }
        }

        /// <summary>
        /// This property will contain the Window name in case we need to highligh a control in a different Window than DynamoView
        /// </summary>
        [JsonProperty(nameof(WindowName))]
        public string WindowName { get; set; }

        /// <summary>
        /// This represent the UIElement as string of the VisualTree that will be highlighted (readed from the json file)
        /// </summary>
        [JsonProperty(nameof(WindowElementNameString))]
        public string WindowElementNameString { get; set; }


        /// <summary>
        /// This property represent the UI Element Type that contains the UIElement (WindowElementName) due that in some cases is not the DynamoView (like the sub MenuItems)
        /// </summary>
        [JsonProperty(nameof(UIElementTypeString))]
        public string UIElementTypeString { get; set; }

        /// <summary>
        /// In case the UIElement to be highlighted is in a Grid, then this property will have the Grid name
        /// </summary>
        [JsonProperty(nameof(UIElementGridContainer))]
        public string UIElementGridContainer { get; set; }


        /// <summary>
        /// This property represents the Rectangle.Width that will be used for the highlight rectangle feature
        /// </summary>
        public double HighlightRectWidth { 
            get 
            { 
                return highlightRectWidth; 
            }
            set 
            { 
                highlightRectWidth = value;
                RaisePropertyChanged(nameof(HighlightRectWidth));
            }
        }

        /// <summary>
        /// This property represents the Rectangle.Height that will be used for the highlight rectangle feature
        /// </summary>
        public double HighlightRectHeight
        {
            get 
            { 
                return highlightRectHeight;
            }
            set 
            { 
                highlightRectHeight = value;
                RaisePropertyChanged(nameof(HighlightRectHeight));
            }
        }

        /// <summary>
        /// This property represents the Rectangle.CanvasTop that will be used for the highlight rectangle location
        /// </summary>
        public double HighlightRectCanvasTop
        {
            get
            {
                return highlightRectCanvasTop;
            }
            set
            {
                highlightRectCanvasTop = value;
                RaisePropertyChanged(nameof(HighlightRectCanvasTop));
            }
        }

        /// <summary>
        /// This property represents the Rectangle.CanvasLeft that will be used for the highlight rectangle location
        /// </summary>
        public double HighlightRectCanvasLeft
        {
            get
            {
                return highlightRectCanvasLeft;
            }
            set
            {
                highlightRectCanvasLeft = value;
                RaisePropertyChanged(nameof(HighlightRectCanvasLeft));
            }
        }

        /// <summary>
        /// This method will be used in case we need to set all the Rectangle location properties in one time
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal void SetHighlighRectSize(double top, double left, double width, double height)
        {
            HighlightRectWidth = width;
            HighlightRectHeight = height;
            HighlightRectCanvasTop = top;
            HighlightRectCanvasLeft = left;
        }

        /// <summary>
        /// This method will be used for hiding the highlight rectangle
        /// </summary>
        internal void ClearHighlightRectangleSize()
        {
            HighlightRectWidth = 0;
            HighlightRectHeight = 0;
            HighlightRectCanvasTop = 0;
            HighlightRectCanvasLeft = 0;
        }
    }
}