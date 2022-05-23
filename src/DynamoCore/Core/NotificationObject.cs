using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Dynamo.Core
{
    /// <summary>
    /// This class notifies the View when there is a change.    
    /// </summary>
    [Serializable]
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        internal PropertyChangeManager PropertyChangeManager { get; } = new PropertyChangeManager();
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>        
        public event PropertyChangedEventHandler PropertyChanged;

        internal static IDisposable DeferPropertyChanges(NotificationObject obj)
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

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            //TODO profile this?
            if (!PropertyChangeManager.ShouldRaiseNotification(propertyName))
            {
                return;
            }

            if (PropertyChangeManager.ShouldDeferNotification(propertyName))
            {
                return;
            }

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException("propertyNames");

            foreach (var name in propertyNames)
            {
                RaisePropertyChanged(name);
            }
        }
    }
}
