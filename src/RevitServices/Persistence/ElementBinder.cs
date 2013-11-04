using System;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using DSNodeServices;

namespace RevitServices.Persistence
{
    [Serializable]
    internal class SerializableId : ISerializable
    {
        public String stringID { get; set; }
        public int intID { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class ElementBinder
    {
        private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";


        /// <summary>
        /// Get the element associated with the current operation from trace
        /// null if there is no object, or it's of the wrong type etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetElementFromTrace<T>(Document document)
            where T : Autodesk.Revit.DB.Element
        {

            //Get the element ID that was cached in the callsite
            ISerializable traceData = TraceUtils.GetTraceData(REVIT_TRACE_ID);

            SerializableId id = traceData as SerializableId;
            if (id == null)
                return null; //There was no usable data in the trace cache


            //@TODO(Luke): make this work with hot swapping ids and guids rather than
            //always using GUIDs

            String traceDataStr = id.stringID;

            T ret;

            if (Elements.ElementUtils.TryGetElement(document, traceDataStr, out ret))
                return ret;
            else
                return null;

        }

        /// <summary>
        /// Set the element associated with the current operation from trace
        /// null if there is no object, or it's of the wrong type etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void SetElementForTrace(ElementId elementId)
        {

            SerializableId id = new SerializableId();
            id.intID = elementId.IntegerValue;
       
            //TODO(Push the GUID into the object)

            //Set the element ID cached in the callsite
            TraceUtils.SetTraceData(REVIT_TRACE_ID, id);
        }


    }

}
