using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.FileTrust;
using Dynamo.Wpf.ViewModels.Notifications;

namespace Dynamo.Wpf.Views.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationUI.xaml
    /// </summary>
    public partial class NotificationUI : Popup
    {
        private NotificationsUIViewModel notificationsUIViewModel;       

        public NotificationUI(DynamoView dynamoViewWindow)
        {
            InitializeComponent();

            if(notificationsUIViewModel == null)
            {
                notificationsUIViewModel = new NotificationsUIViewModel();
            }

            DataContext = notificationsUIViewModel;

            //The BackgroundRectangle represent the tooltip background rectangle that is drawn over a Canvas
            //Needs to be moved 10 pixels over the X axis to show the direction pointers (Height was already increased above)
            //Needs to be moved 10 pixels over the Y axis to show the shadow at top and bottom.
            BackgroundRectangle.Rect = new Rect(notificationsUIViewModel.PopupBordersOffSet, notificationsUIViewModel.PopupBordersOffSet, notificationsUIViewModel.PopupRectangleWidth, notificationsUIViewModel.PopupRectangleHeight);


            //The CustomRichTextBox has a margin of 10 in left and 10 in right, also there is a indentation for drawing the PointerDirection of 10 of each side so that's why we are subtracting 40.
            mainPopupGrid.Width = notificationsUIViewModel.PopupRectangleWidth;
            mainPopupGrid.Height = notificationsUIViewModel.PopupRectangleHeight;
            mainPopupGrid.Margin = new Thickness(notificationsUIViewModel.PopupBordersOffSet, notificationsUIViewModel.PopupBordersOffSet, 0, 0);
        }

        internal void UpdatePopupLocation()
        {
            if (IsOpen)
            {
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }
    }
}
