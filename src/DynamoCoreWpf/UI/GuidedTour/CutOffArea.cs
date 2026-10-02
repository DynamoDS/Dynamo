using System.Windows;
using Dynamo.Core;
using System.Text.Json.Serialization;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will contain information about the area that will be cut off from the background Overlay
    /// </summary>
    public class CutOffArea : NotificationObject
    {
        #region Private Properties
        private double widthBoxDelta;
        private double heightBoxDelta;
        private double xPosOffset;
        private double yPosOffset;
        private Rect cutOffRect;
        #endregion

        /// <summary>
        /// Since the box that cuts the elements has its size fixed, this variable applies a value to fix its Width
        /// </summary>
        [JsonPropertyName(nameof(WidthBoxDelta))]
        public double WidthBoxDelta { get => widthBoxDelta; set => widthBoxDelta = value; }

        /// <summary>
        /// Since the box that cuts the elements has its size fixed, this variable applies a value to fix its Height
        /// </summary>
        [JsonPropertyName(nameof(HeightBoxDelta))]
        public double HeightBoxDelta { get => heightBoxDelta; set => heightBoxDelta = value; }

        /// <summary>
        /// This property will move the CutOff area horizontally over the X axis
        /// </summary>
        [JsonPropertyName(nameof(XPosOffset))]
        public double XPosOffset { get => xPosOffset; set => xPosOffset = value; }

        /// <summary>
        /// This property will move the CutOff area vertically over the Y axis
        /// </summary>
        [JsonPropertyName(nameof(YPosOffset))]
        public double YPosOffset { get => yPosOffset; set => yPosOffset = value; }

        /// <summary>
        /// In the case the cutoff area is not the same than HostControlInfo.HostUIElementString the this property needs to be populated
        /// </summary>
        [JsonPropertyName(nameof(WindowElementNameString))]
        public string WindowElementNameString { get; set; }

        /// <summary>
        /// In cases when we need to put the CutOff area over a node in the Workspace this property will be used
        /// </summary>
        [JsonPropertyName(nameof(NodeId))]
        public string NodeId { get; set; }

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