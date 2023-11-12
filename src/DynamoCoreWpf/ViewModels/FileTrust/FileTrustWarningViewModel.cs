using System.Windows;
using System.Windows.Media;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.FileTrust
{
    /// <summary>
    /// The View Model used for FileTrustWarning.xaml view
    /// </summary>
    public class FileTrustWarningViewModel : ViewModelBase
    {
        private bool showWarningPopup;
        private bool allowOneTimeTrust;

        /// <summary>
        /// This offset is set to make the popup pointer point at the middle of the RunMode combo box
        /// </summary>
        private double PointerHorizontalOffset = 60;


        /// <summary>
        /// This will contains the 3 points needed for drawing the Tooltip pointer direction
        /// </summary>
        public PointCollection TooltipPointerPoints { get; set; }
        
        //Direction of the shadow shown in the Popup
        public double ShadowTooltipDirection { get; set; } = 270;

        //This value will describe the needed space for drawing pointers inside the Popup
        public int PopupBordersOffSet { get; set; } = 15;

        /// <summary>
        /// Popup Width, by default it will have an exaggerated Width to accommodate localized languages
        /// TODO it should really be responsive either using grid of auto canvas:
        /// https://stackoverflow.com/questions/855334/wpf-how-to-make-canvas-auto-resize
        /// </summary>
        public double PopupRectangleWidth { get; set; } = 500;

        /// <summary>
        /// Popup Height, by default it will have a Height of 220
        /// </summary>
        public double PopupRectangleHeight { get; set; } = 220;

       /// <summary>
       /// This property indicates if the Popup should be displayed or not
       /// </summary>
        public bool ShowWarningPopup 
        { 
            get
            {
                return showWarningPopup;
            }
            set
            {
                showWarningPopup = value;
                RaisePropertyChanged(nameof(ShowWarningPopup));
            }
        }

        /// <summary>
        /// This property indicates if the Popup has been dismissed once, temporarily ignoring the trust warning
        /// </summary>
        public bool AllowOneTimeTrust
        {
            get
            {
                return allowOneTimeTrust;
            }
            set
            {
                allowOneTimeTrust = value;
            }
        }

        /// <summary>
        /// This property will contain the full path of the file to be opened and is used to check if already exists in the Trusted Paths 
        /// </summary>
        public string DynFileDirectoryName { get; set; }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public FileTrustWarningViewModel()
        {
            //First X,Y coordinate
            double pointX1 = PopupBordersOffSet + PointerHorizontalOffset;
            double pointY1 = PopupRectangleHeight + PopupBordersOffSet;

            //Second X,Y coordinate
            double pointX2 = PopupBordersOffSet + (PopupBordersOffSet * 2) + PointerHorizontalOffset;
            double pointY2 = PopupRectangleHeight + PopupBordersOffSet;

            //Third X,Y coordinate
            double pointX3 = (PopupBordersOffSet * 2) + PointerHorizontalOffset;
            double pointY3 = PopupRectangleHeight + (PopupBordersOffSet * 2);

            //Creating the pointer pointing to the Run section of Dynamo
            TooltipPointerPoints = new PointCollection(new[] { new Point(pointX1, pointY1),
                                                               new Point(pointX2, pointY2),
                                                               new Point(pointX3, pointY3)});

            showWarningPopup = false;
            allowOneTimeTrust = false;
        }

    }
}
