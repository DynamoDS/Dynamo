using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Autodesk.DesignScript.Runtime;

using DynamoServices;

namespace SampleLibraryZeroTouch
{
    /// <summary>
    /// This is the item that we will store in our data store.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public sealed class TraceExampleItem
    {
        public string Description { get; set; }

        public TraceExampleItem(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    /*
     * After a graph update, Dynamo typically disposes of all
     * objects created during the graph update. But what if there are 
     * objects which are expensive to re-create, or which have other
     * associations in a host application? You wouldn't want those those objects
     * re-created on every graph update. For example, you might 
     * have an external database whose records contain data which needs
     * to be re-applied to an object when it is created in Dynamo.
     * In this example, we use a wrapper class, TraceExampleWrapper, to create 
     * TraceExampleItem objects which are stored in a static dictionary 
     * (they could be stored in a database as well). On subsequent graph updates, 
     * the objects will be retrieved from the data store using a trace id stored 
     * in the trace cache.
     */

    /// <summary>
    /// A class which contains methods to construct TraceExampleItem objects.
    /// </summary>
    public static class TraceExampleWrapper
    {
        /// <summary>
        /// Create a TraceExampleItem and store it in a static dictionary.
        /// </summary>
        public static TraceExampleItem ByString(string description)
        {
            // See if there is data for this object is in trace.
            var traceId = TraceableObjectManager.GetObjectIdFromTrace();

            TraceExampleItem item = null;

            int id;
            if (traceId == null)
            {
                // If there's no id stored in trace for this object,
                // then grab the next unused trace id.
                id = TraceableObjectManager.GetNextUnusedID();

                // Create an item
                item = new TraceExampleItem(description);

                // Remember to store the updated object in the trace object manager,
                // so it's available to use the next time around.
                TraceableObjectManager.RegisterTraceableObjectForId(id, item);
            }
            else
            {
                // If there's and id stored in trace, then retrieve the object stored
                // with that id from the trace object manager.
                item = (TraceExampleItem)TraceableObjectManager.GetTracedObjectById(traceId.IntID)
                    ?? new TraceExampleItem(description);

                // Update the item
                item.Description = description;
            }

            return item;
        }
    }

    /*
     * The TraceableObjectManager class maintains a static 
     * dictionary of objects keyed by their trace id. This
     * dictionary is where your objects are stored between
     * runs. At run-time, they are retrieved using the id
     * stored in trace.
     */

    /// <summary>
    /// A class which maintains a static dictionary for storing
    /// objects, keyed by their trace id.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class TraceableObjectManager
    {
        private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

        private static int id = 0;

        public static int GetNextUnusedID()
        {
            var next = id;
            id++;
            return next;
        }

        private static Dictionary<int, object> traceableObjectManager = new Dictionary<int, object>();

        public static TraceableId GetObjectIdFromTrace()
        {
            return TraceUtils.GetTraceData(REVIT_TRACE_ID) as TraceableId;
        }

        public static object GetTracedObjectById(int id)
        {
            object ret;
            traceableObjectManager.TryGetValue(id, out ret);
            return ret;
        }

        public static void RegisterTraceableObjectForId(int id, object objectToTrace)
        {
            if (traceableObjectManager.ContainsKey(id))
            {
                traceableObjectManager[id] = objectToTrace;
            }
            else
            {
                traceableObjectManager.Add(id, objectToTrace);
                TraceUtils.SetTraceData(REVIT_TRACE_ID, new TraceableId(id));
            }
        }

        public static void Clear()
        {
            traceableObjectManager.Clear();
            id = 0;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    [Serializable]
    public class TraceableId : ISerializable
    {
        public int IntID { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("intID", IntID, typeof(int));
        }

        public TraceableId(int id)
        {
            IntID = id;
        }

        /// <summary>
        /// Ctor used by the serialisation engine
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public TraceableId(SerializationInfo info, StreamingContext context)
        {
            IntID = (int)info.GetValue("intID", typeof(int));
        }
    }
}
