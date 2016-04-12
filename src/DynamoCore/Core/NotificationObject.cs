using System;
using System.ComponentModel;

namespace Dynamo.Core
{
    /// <summary>
    /// This class notifies the View when there is a change.    
    /// </summary>
    [Serializable]
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>        
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
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
