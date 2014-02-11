using System;
using System.Windows;
using EventTrigger = System.Windows.Interactivity.EventTrigger;

namespace Dynamo.UI.Views
{
    public class HandlingEventTrigger : EventTrigger
    {
        protected override void OnEvent(EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }
}
