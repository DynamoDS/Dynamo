using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Wpf.Utilities
{
    public class UINotificationObject : NotificationObject
    {
        internal Core.PropertyChangeManager PropertyChangeManager { get; } = new Core.PropertyChangeManager();
        protected override void RaisePropertyChanged(string propertyName)
        {
            if (!PropertyChangeManager.ShouldRaiseNotification(propertyName))
            {
                return;
            }

            if (PropertyChangeManager.ShouldDeferNotification(propertyName))
            {
                return;
            }

            base.RaisePropertyChanged(propertyName);
        }

        internal static IDisposable DeferPropertyChanges(UINotificationObject obj)
        {
            if (obj == null) return null;

            return Scheduler.Disposable.Create(
                () => {
                    obj.PropertyChangeManager.DeferedNotifications = new List<string>();
                },
                () => {
                    if (obj.PropertyChangeManager.DeferedNotifications != null)
                    {
                        var properties = obj.PropertyChangeManager.DeferedNotifications.ToList();
                        obj.PropertyChangeManager.DeferedNotifications = null;
                        foreach (var item in properties)
                        {
                            obj.RaisePropertyChanged(item);
                        }
                    }
                });
        }
    }
}
