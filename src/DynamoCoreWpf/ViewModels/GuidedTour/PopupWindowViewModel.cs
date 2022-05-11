using System.Windows.Media;
using Dynamo.Wpf.UI.GuidedTour;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.ViewModels.GuidedTour
{
    public class PopupWindowViewModel
    {
        #region Private Properties
        private Step step;
        private bool showPopupPointer;
        private bool showPopupButton;
        private bool showTooltipNavigationCont;
        private string tourLabelProgress;
        #endregion

        #region Public Properties

        /// <summary>
        /// Popup Width, by default it will have a Width of 480
        /// </summary>
        public double Width { get; set; } = 480;

        /// <summary>
        /// Popup Height, by default it will have a Height of 190
        /// </summary>
        public double Height { get; set; } = 190;

        /// <summary>
        /// This PointCollection property will have the 3 points needed for drawing the Tooltip Pointer 
        /// </summary>
        public PointCollection TooltipPointerPoints
        {
            get
            {
                return Step.TooltipPointerPoints;
            }
            set
            {
                Step.TooltipPointerPoints = value;
            }
        }

        /// <summary>
        /// This will contains the shadow direction in degrees that will be shown in the pointer
        /// </summary>
        public double ShadowTooltipDirection
        { 
            get
            {
                return Step.ShadowTooltipDirection;
            }
            set
            {
                Step.ShadowTooltipDirection = value;
            } 
        }

        /// <summary>
        /// Due that some popups doesn't need the pointer then this property hides or show the pointer
        /// </summary>
        public bool ShowPopupPointer 
        { 
            get
            {
                return showPopupPointer;
            }
            set
            {
                showPopupPointer = value;
            }
        }

        /// <summary>
        /// Due that some popups doesn't need the button (Start Tour button) then this property hides or show the button
        /// </summary>
        public bool ShowPopupButton
        {
            get
            {
                return showPopupButton;
            }
            set
            {
                showPopupButton = value;
            }
        }

        /// <summary>
        /// Due that some popups doesn't need the NavigationControls (e.g. 1 of 6) then this property hides or show the controls
        /// </summary>
        public bool ShowTooltipNavigationControls
        {
            get
            {
                return showTooltipNavigationCont;
            }
            set
            {
                showTooltipNavigationCont = value;
            }
        }

        /// <summary>
        /// This property hold a reference to the Step that was created (can be Welcome, Survey, Tooltip, ExitTour). 
        /// </summary>
        public Step Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
            }
        }

        /// <summary>
        /// This string will be shown in the Navigation Controls for showing some specific info (for example the "1 of 6" label)
        /// </summary>
        public string TourLabelProgress
        {
            get
            {
                tourLabelProgress = string.Format("{0} {1} {2}", Step.Sequence,Res.TourLabelProgressText, Step.TotalTooltips);
                return tourLabelProgress;
            }
        }
        #endregion

        /// <summary>
        /// Foir creating a PopupWindowViewModel we need to pass the step to be used 
        /// </summary>
        /// <param name="popupType">Step is a abstract class then the parameter can be any of the child clases like Welcome, Tooltip, Survey....</param>
        public PopupWindowViewModel(Step popupType)
        {
            Step = popupType;
        }
    }
}
