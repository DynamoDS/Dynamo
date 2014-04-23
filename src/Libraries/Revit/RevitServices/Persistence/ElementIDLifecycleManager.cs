using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSNodeServices;

namespace RevitServices.Persistence
{
    /// <summary>
    /// Class to handle the lifetime of elements from their IDs
    /// </summary>
    public class ElementIDLifecycleManager<T>
    {
        //Note this is only mutex for a specific type param
        private static Object singletonMutex = new object();
        private static ElementIDLifecycleManager<T> manager;

        private Dictionary<T, List<Object>> wrappers;
        private Dictionary<T, bool> revitDeleted; 


        private ElementIDLifecycleManager()
        {
            wrappers = new Dictionary<T, List<object>>();
            revitDeleted = new Dictionary<T, bool>();
        }

        /// <summary>
        /// Get the LifecycleManager for the specific type
        /// WARNING: This is only a singleton for a given TypeArg
        /// </summary>
        /// <returns></returns>
        public static ElementIDLifecycleManager<T> GetInstance()
        {
            lock (singletonMutex)
            {
                if (manager == null)
                {
                    manager = new ElementIDLifecycleManager<T>();
                }

                return manager;
            }
        }


        /// <summary>
        /// Register a new dependency between an element ID and a wrapper
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="wrapper"></param>
        public void RegisterAsssociation(T elementID, Object wrapper)
        {

            List<Object> existingWrappers;
            if (wrappers.TryGetValue(elementID, out existingWrappers))
            {
                //ID already existed, check we're not over adding
                Validity.Assert(!existingWrappers.Contains(wrapper), 
                    "Lifecycle manager alert: registering the same Revit Element Wrapper twice"
                    + " {6528305F}");
                //return;
            }
            else
            {
                existingWrappers = new List<object>();
                wrappers.Add(elementID, existingWrappers);
            }

            existingWrappers.Add(wrapper);
            if (!revitDeleted.ContainsKey(elementID))
            {
                revitDeleted.Add(elementID, false);                
            }
        }

        /// <summary>
        /// Remove an association between an element ID and 
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="wrapper"></param>
        /// <returns>The number of remaining associations</returns>
        public int UnRegisterAssociation(T elementID, Object wrapper)
        {
            List<Object> existingWrappers;
            if (wrappers.TryGetValue(elementID, out existingWrappers))
            {
                //ID already existed, check we're not over adding
                if (existingWrappers.Contains(wrapper))
                {
                    existingWrappers.Remove(wrapper);
                    if (existingWrappers.Count == 0)
                    {
                        wrappers.Remove(elementID);
                        revitDeleted.Remove(elementID);
                        return 0;
                    }
                    else
                    {
                        return existingWrappers.Count;
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "Attempting to remove a wrapper that wasn't there registered");
                }

            }
            else
            {
                //The ID didn't exist

                throw new InvalidOperationException(
                    "Attempting to remove a wrapper, but there were no ids registered");
            }
            

        }

        /// <summary>
        /// Get the number of wrappers that are registered
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetRegisteredCount(T id)
        {
            if (!wrappers.ContainsKey(id))
            {
                return 0;
            }
            else
            {
                return wrappers[id].Count;
            }
            
        }

        /// <summary>
        /// Checks whether an element has been deleted in Revit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsRevitDeleted(T id)
        {
            if (!revitDeleted.ContainsKey(id))
            {
                throw new ArgumentException("Element is not registered");
            }

            return revitDeleted[id];
        }


        /// <summary>
        /// This method tells the life cycle 
        /// </summary>
        /// <param name="id"The element that needs to be deleted></param>
        public void NotifyOfRevitDeletion(T id)
        {
            revitDeleted[id] = true;

        }


    }
}
