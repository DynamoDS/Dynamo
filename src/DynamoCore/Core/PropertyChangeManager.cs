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
    internal struct PropertyChangeManager
    {
        //TODO lock field during these accessors or use concurrent structure?
        internal IEnumerable<string> PropertiesToSuppress { get; set; }
        internal List<string> DeferedNotifications { get; set; }

        /// <summary>
        /// Checks if notifications are suppressed for a specific property.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public bool ShouldRaiseNotification(string prop)
        {
            if(PropertiesToSuppress is null)
            {
                return true;
            }
            return !PropertiesToSuppress.Contains(prop);
        }

        /// <summary>
        /// Checks if notifications are defered.
        /// Adds property to deferred list if deferring is enabled.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        internal bool ShouldDeferNotification(string prop)
        {
            if (DeferedNotifications != null)
            {
                if (!DeferedNotifications.Contains(prop))
                    DeferedNotifications.Add(prop);
                return true;
            }
            return false;
        }
    }
}
