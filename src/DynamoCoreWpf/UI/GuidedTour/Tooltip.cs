using System.Windows;
using System.Windows.Media;
using Dynamo.Wpf.ViewModels.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// The tooltip class represents the popup that will be shown over the Dynamo UI pointing to a specific control
    /// </summary>
    public class Tooltip : Step
    {
        double PointerHeight = 30;
        double PointerWidth = 15;
        double PointerDownWidth = 20;
        double PointerDownHeight = 10;
        double PointerTooltipOverlap = 5;
        double PointerVerticalOffset;
        double PointerHorizontalOffset;
        enum SHADOW_DIRECTION { LEFT = 180, RIGHT = 0, BOTTOM = 270, TOP = 90};

        /// <summary>
        /// The Tooltip constructor
        /// </summary>
        /// <param name="host">Host control (of the TreeView) in which the tooltip will be shown</param>
        /// <param name="width">Tooltip width</param>
        /// <param name="height">Tooltip height</param>
        /// <param name="direction">The pointer direction of the tooltip</param>
        public Tooltip(HostControlInfo host, double width, double height, PointerDirection direction, double verticalTooltipOffset)
            :base(host, width, height)
        {
            SetPointerDirection(direction);

            //The offset represent the distance vertically from the top/bottom for showing the tooltip pointer (a triangle)
            PointerVerticalOffset = (Height / 8) + verticalTooltipOffset;
            PointerHorizontalOffset = (Width / 8);
            DrawPointerDirection(direction);
        }

        /// <summary>
        /// This method will create the X,Y points (3 sets) for drawing a triangle that represents the Tooltip pointer direction
        /// </summary>
        /// <param name="direction"></param>
        private void DrawPointerDirection(PointerDirection direction)
        {
            //The Default pointer direction is PointerDirection.TOP_LEFT

            //First X,Y coordinate
            double pointX1 = PointerWidth;
            double pointY1 = 0;

            //Second X,Y coordinate
            double pointX2 = PointerWidth;
            double pointY2 = PointerHeight;

            //Third X,Y coordinate
            double pointX3 = 0;
            double pointY3 = PointerHeight / 2;

            //Due that we are drawing the 2 direction pointers we need to add 20 to the current width (Width + PointerWidth*2 - PointerTooltipOverlap*2)
            double realWidth = Width + PointerWidth * 2 - PointerTooltipOverlap * 2;
            double realHeight = Height + PointerHeight * 2 - PointerTooltipOverlap * 2;

            //For each Y coordinate we add the Vertical offset otherwise the pointer will be always at the top or the botton
            if (direction == PointerDirection.TOP_LEFT)
            {
                pointX1 = PointerWidth;
                pointY1 = 0 + PointerVerticalOffset;

                pointX2 = PointerWidth;
                pointY2 = PointerHeight + PointerVerticalOffset;

                pointX3 = 0;
                pointY3 = PointerHeight / 2 + PointerVerticalOffset;
                //Left Shadow
                ShadowTooltipDirection = (double)SHADOW_DIRECTION.LEFT;

            }
            else if (direction == PointerDirection.BOTTOM_LEFT)
            {
                pointX1 = PointerWidth;
                pointY1 = Height - PointerVerticalOffset;

                pointX2 = PointerWidth;
                pointY2 = Height - PointerHeight - PointerVerticalOffset;

                pointX3 = 0;
                pointY3 = Height - PointerHeight / 2 - PointerVerticalOffset;
                //Left Shadow
                ShadowTooltipDirection = (double)SHADOW_DIRECTION.LEFT;
            }
            else if (direction == PointerDirection.TOP_RIGHT)
            {
                pointX1 = realWidth - PointerWidth;
                pointY1 = 0 + PointerVerticalOffset;

                pointX2 = realWidth - PointerWidth;
                pointY2 = PointerHeight + PointerVerticalOffset;

                pointX3 = realWidth;
                pointY3 = PointerHeight / 2 + PointerVerticalOffset;
                //Right Shadow
                ShadowTooltipDirection = (double)SHADOW_DIRECTION.RIGHT;

            }
            else if (direction == PointerDirection.BOTTOM_RIGHT)
            {
                pointX1 = realWidth - PointerWidth;
                pointY1 = Height - PointerVerticalOffset;

                pointX2 = realWidth - PointerWidth;
                pointY2 = Height - PointerHeight - PointerVerticalOffset;

                pointX3 = realWidth;
                pointY3 = Height - PointerHeight / 2 - PointerVerticalOffset;
                //Right Shadow
                ShadowTooltipDirection = (double)SHADOW_DIRECTION.RIGHT;
            }
            else if (direction == PointerDirection.BOTTOM_DOWN)
            {
                pointX1 = PointerHorizontalOffset;
                pointY1 = Height + PointerDownHeight;

                pointX2 = PointerDownWidth + PointerHorizontalOffset;
                pointY2 = Height + PointerDownHeight;

                pointX3 = PointerDownWidth / 2 + PointerHorizontalOffset;
                pointY3 = realHeight - PointerHeight;
                //Bottom Shadow
                ShadowTooltipDirection = (double)SHADOW_DIRECTION.BOTTOM;
            }
            else if(direction == PointerDirection.NONE)
            {
                pointX1 = 0;
                pointX2 = 0;
                pointX3 = 0;
            }

            TooltipPointerPoints = new PointCollection(new[] { new Point(pointX1, pointY1),
                                                               new Point(pointX2, pointY2),
                                                               new Point(pointX3, pointY3),});
        }

        /// <summary>
        /// Overriding the parent method for creating a popup
        /// </summary>
        protected override void CreatePopup()
        {
            //Creating the ViewModel for the PopupWindow (View)
            var popupViewModel = new PopupWindowViewModel(this)
            {
                //Controls setup specific for a Tooltip popup
                ShowPopupPointer = true,
                ShowPopupButton = false,
                ShowTooltipNavigationControls = true,
                Width = Width,
                Height = Height
            };

            //The Popup Viewmodel and Host is passed as parameters to the PopupWindow so it will create the popup with the needed values (width, height)
            stepUIPopup = new PopupWindow(popupViewModel,HostPopupInfo);
            
        }
    }
}
