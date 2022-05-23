using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Wrapper over Microsoft.Practices.Prism.ViewModel.NotificationObject
    /// Provides more controls over notifications at UI level
    /// </summary>
    public class UINotificationObject : NotificationObject
    {
        internal Core.PropertyChangeManager PropertyChangeManager;
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

        /// <summary>
        /// Disable property change notifications for a list of a props.
        /// The previous set of suppressed notifications will be cleared.
        /// This API is meant to be used only for a short time and Dispose should be
        /// called on the object this call returns.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        internal static IDisposable SuppressPropertyChanges(UINotificationObject obj, params string[] props)
        {
            if (obj == null) return null;

            return Scheduler.Disposable.Create(
                () => {
                    obj.PropertyChangeManager.PropertiesToSuppress = new List<string>(props);
                },
                () => {
                    obj.PropertyChangeManager.PropertiesToSuppress = null;
                });
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
