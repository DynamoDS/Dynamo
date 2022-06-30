using Dynamo.Controls;
using Dynamo.NotificationCenter.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dynamo.NotificationCenter
{
    public class NotificationCenterController
    {
        NotificationUI notificationUIPopup;
        Button notificationsButton;
        public NotificationCenterController(DynamoView dynamoView, Button notificationsButton)
        {
            notificationUIPopup = new NotificationUI();
            notificationUIPopup.IsOpen = false;
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.HorizontalOffset = -285;
            notificationUIPopup.VerticalOffset = 10;

            this.notificationsButton = notificationsButton;

            dynamoView.SizeChanged += DynamoView_SizeChanged;
            dynamoView.LocationChanged += DynamoView_LocationChanged;
            notificationsButton.Click += NotificationsButton_Click;
        }

        private void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            notificationUIPopup.IsOpen = !notificationUIPopup.IsOpen;
        }
    }
}
