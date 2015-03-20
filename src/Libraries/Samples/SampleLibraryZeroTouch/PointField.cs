using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using DynamoServices;

namespace SampleLibraryZeroTouch
{
    // This sample demonstrates the use of the CanUpdatePeriodically
    // attribute. Placing an instance of the PeriodicUpdateExample.PointField 
    // node in your workspace will enable the Periodic RunType in the run
    // settings control. This formula used to create the point field
    // requires a parameter, 't', that is incremented during each run. 
    // This example also demonstrates how to use trace to store data that
    // you want to carry over between evaluations.
    public class  PeriodicUpdateExample : IGraphicItem
    {
        private List<double> vertexCoords = new List<double>();

        private PeriodicUpdateExample(double t, int id)
        {
            for (double x = -5; x <= 5; x += 0.5)
            {
                for (double y = -5; y <= 5; y += 0.5)
                {
                    var z = Math.Sin(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - t);
                    vertexCoords.Add(x);
                    vertexCoords.Add(y);
                    vertexCoords.Add(z);
                }
            }

            t += 0.1;
            if (t > Math.PI*2) t = 0.0;

            // Remember to store the updated object in the trace object manager,
            // so it's available to use the next time around.
            TraceableObjectManager.RegisterTraceableObjectForId(id, t);
        }

        // The CanUpdatePeriodicallyAttribute can be applied to methods in 
        // your library which you want to enable the Periodic RunType in Dynamo
        // when exposed as nodes.
        /// <summary>
        /// Create a field of waving points that periodically updates.
        /// </summary>
        /// <returns>A PeriodicUpdateExample object.</returns>
        [CanUpdatePeriodically(true)]
        public static PeriodicUpdateExample PointField()
        {
            // See if the data for this object is in trace.
            var traceId = TraceableObjectManager.GetObjectIdFromTrace();

            var t = 0.0;
            int id;
            if (traceId == null)
            {
                // If there's no id stored in trace for this object,
                // then grab the next unused trace id.
                id = TraceableObjectManager.GetNextUnusedID();
            }
            else
            {
                // If there's and id stored in trace, then retrieve the object stored
                // with that id from the trace object manager.
                id = traceId.IntID;
                t = (double)TraceableObjectManager.GetTracedObjectById(traceId.IntID);
            }

            return new PeriodicUpdateExample(t, id);
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            package.PointVertices = vertexCoords;
        }
    }

    /// <summary>
    /// The TraceableObjectManager class maintains a static 
    /// dictionary of objects keyed by their trace id. This
    /// dictionary is where your objects are stored between
    /// runs. At run-time, they are retrieved using the id
    /// stored in trace.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class TraceableObjectManager
    {
        private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

        private static int solverId = 0;

        public static int GetNextUnusedID()
        {
            var next = solverId;
            solverId++;
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
    }

    [IsVisibleInDynamoLibrary(false)]
    [Serializable]
    public class  TraceableId : ISerializable
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
