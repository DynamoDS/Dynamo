using System;
using System.Windows;

namespace Dynamo.UI.Views
{
    [Obsolete("For internal use only, do not reference outside DynamoCoreWpf")]
    public class HandlingEventTrigger : Dynamo.Microsoft.Xaml.Behaviors.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }
}
