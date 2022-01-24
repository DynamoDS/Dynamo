using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Core
{
    /// <summary>
    /// Class can be used to stop property change notifications from being raised if the client object
    /// inherits from <see cref="NotificationObject"/> and uses RaisePropertyChanged to raise prop notifications.
    /// Use like:
    /// using (node.SetPropsToSupress("frozen")){
    /// node.frozen = true;
    /// }
    /// </summary>
    internal class PropertyChangeManager : IDisposable
    {
        //TODO lock field during these accessors or use concurrent structure?
        protected IEnumerable<string> PropertyNames { get; set; }
        /// <summary>
        /// Disable property change notifications for a list of a props.
        /// The previous set of suppressed notifications will be cleared.
        /// This API is meant to be used only for a short time and Dispose should be
        /// called on the object this call returns.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public PropertyChangeManager SetPropsToSuppress(params string[] props)
        {
            PropertyNames = props;
            return this;
        }
        /// <summary>
        /// Disposing this class will allow prop notifications to be raised.
        /// </summary>
        public void Dispose()
        {
            PropertyNames = null;
        }
        /// <summary>
        /// Checks if notifications are suppressed for a specific property.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public bool ShouldRaiseNotification(string prop)
        {
            if(PropertyNames is null)
            {
                return true;
            }
            return !PropertyNames.Contains(prop);
        }
    }
}
