using System.Windows;
using Dynamo.Core;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will contain information about the area that will be cur off from the background Overlay
    /// </summary>
    public class CutOffArea : NotificationObject
    {
        #region Private Properties
        private double widthBoxDelta;
        private double heightBoxDelta;
        private Rect cutOffRect;
        private UIElement windowElementName;
        #endregion

        /// <summary>
        /// Since the box that cuts the elements has its size fixed, this variable applies a value to fix its Width
        /// </summary>
        [JsonProperty("WidthBoxDelta")]
        public double WidthBoxDelta { get => widthBoxDelta; set => widthBoxDelta = value; }

        /// <summary>
        /// Since the box that cuts the elements has its size fixed, this variable applies a value to fix its Height
        /// </summary>
        [JsonProperty("HeightBoxDelta")]
        public double HeightBoxDelta { get => heightBoxDelta; set => heightBoxDelta = value; }


        /// <summary>
        /// Rect used to cut a rectangle on the guide background 
        /// </summary>
        public Rect CutOffRect
        {
            get
            {
                return cutOffRect;
            }
            set
            {
                cutOffRect = value;
                RaisePropertyChanged(nameof(CutOffRect));
            }
        }
    }
}