using System.Windows;
using System.Windows.Media;
using Dynamo.ViewModels;

namespace Dynamo.Notifications
{
    class NotificationsUIViewModel : ViewModelBase
    {
        public PointCollection TooltipPointerPoints { get; set; }

        //Direction of the shadow shown in the Popup
        public double ShadowTooltipDirection { get; set; } = 90;

        //This value will describe the needed space for drawing pointers inside the Popup
        internal int PopupBordersOffSet { get; set; } = 15;

        /// <summary>
        /// Popup Width, by default it will have an exaggerated Width to accommodate localized languages
        /// TODO it should really be responsive either using grid of auto canvas:
        /// https://stackoverflow.com/questions/855334/wpf-how-to-make-canvas-auto-resize
        /// </summary>
        public double PopupRectangleWidth { get; set; } = 310;

        /// <summary>
        /// Popup Height, by default it will have a Height of 568
        /// </summary>
        internal double PopupRectangleHeight { get; set; } = 568;
                
        double PointerWidth = 25;

        public NotificationsUIViewModel()
        {
            var pointerOffSet = 10;

            //First X,Y coordinate
            double pointX1 = PopupRectangleWidth - PointerWidth + pointerOffSet;
            double pointY1 = PopupBordersOffSet;

            ////Second X,Y coordinate
            double pointX2 = PopupRectangleWidth + pointerOffSet;
            double pointY2 = PopupBordersOffSet;

            ////Third X,Y coordinate
            double pointX3 = PopupRectangleWidth - PointerWidth / 2 + pointerOffSet;
            double pointY3 = 0;

            //Creating the pointer pointing to the Run section of Dynamo
            TooltipPointerPoints = new PointCollection(new[] { new Point(pointX1, pointY1),
                                                               new Point(pointX2, pointY2),
                                                               new Point(pointX3, pointY3)});
        }
    }
}
