using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DSNodeServices;

namespace FFITarget
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
        /// <param name="id">The element that needs to be deleted></param>
        public void NotifyOfRevitDeletion(T id)
        {
            revitDeleted[id] = true;

        }


    }

    public class WrapperObject : IDisposable
    {
        private int id;

        public int ID { 
            get { return id; } 
            set { 
                id = value;
                var manager = ElementIDLifecycleManager<int>.GetInstance();
                manager.RegisterAsssociation(id, this);
            } 
        }
        public Guid WrapperGuid { get; private set; }

        private static int nextID = 0;

        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

        public WrapperObject(int x)
        {
            WrapperGuid = Guid.NewGuid();

            var traceVal = DSNodeServices.TraceUtils.GetTraceData(__TEMP_REVIT_TRACE_ID);

            if (traceVal != null)
            {

                IDHolder idHolder = (IDHolder)traceVal;
                ID = idHolder.ID;

            }
            else
            {
                nextID++;
                ID = nextID;
                DSNodeServices.TraceUtils.SetTraceData(__TEMP_REVIT_TRACE_ID, new IDHolder() { ID = nextID });
            }



        }

        public void Dispose()
        {
            Debug.WriteLine("Wrapper: " + WrapperGuid.ToString());
            Debug.WriteLine("     Pre-dispose of: " + ID);



            var elementManager = ElementIDLifecycleManager<int>.GetInstance();
            int remainingBindings = elementManager.UnRegisterAssociation(id, this);

            // Do not delete Revit owned elements
            if (remainingBindings == 0)
            {
                //Do the real dispose here

                WrappersTest.CleanedObjects.Add(
                    new Tuple<Guid, int>(WrapperGuid, ID));

                Debug.WriteLine("     Dispose of wrapper target: " + ID);
            }
            else
            {
                //This element has gone
                //but there was something else holding onto the Revit object so don't purge it

                id = -1;
            }


        }
    

    
    }

    public static class WrappersTest
    {
        public static List<Tuple<Guid, int>> CleanedObjects { get; private set; }

        static WrappersTest()
        {
            CleanedObjects = new List<Tuple<Guid, int>>();
        }

        public static void Reset()
        {
            CleanedObjects = new List<Tuple<Guid, int>>();
            
        }

        
    }





}
