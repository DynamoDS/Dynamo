using System;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using DSNodeServices;

namespace RevitServices.Persistence
{
    /// <summary>
    /// Holds a  representation of a Revit ID that supports serialisation
    /// </summary>
    [Serializable]
    public class SerializableId : ISerializable
    {
        public String StringID { get; set; }
        public int IntID { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("stringID", StringID, typeof(string));
            info.AddValue("intID", IntID, typeof(int));
        }

        public SerializableId()
        {
            StringID = "";
            IntID = int.MinValue;

        }

        /// <summary>
        /// Ctor used by the serialisation engine
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public SerializableId(SerializationInfo info, StreamingContext context)
        {
            StringID = (string) info.GetValue("stringID", typeof (string));
            IntID = (int)info.GetValue("intID", typeof(int));

        }
    }

    /// <summary>
    /// Class for handling unique ids in a typesafe ammner
    /// </summary>
    public class ElementUUID
    {
        public String UUID { get; set; }

        public ElementUUID()
        {
            UUID = "";
        }

        public ElementUUID(string uuid)
        {
            this.UUID = uuid;
        }

    }



    public class ElementBinder
    {
        private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

        public static bool IsEnabled = false;

        /// <summary>
        /// Get an ElementId from trace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static ElementId GetElementIdFromTrace(Document document)
        {
            //Get the element ID that was cached in the callsite
            ISerializable traceData = TraceUtils.GetTraceData(REVIT_TRACE_ID);

            SerializableId id = traceData as SerializableId;
            if (id == null)
                return null; //There was no usable data in the trace cache

            var traceDataInt = id.IntID;
            return new Autodesk.Revit.DB.ElementId(traceDataInt);
        }

        /// <summary>
        /// Get an Element unique Identifier from trace
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static ElementUUID GetElementUUIDFromTrace(Document document)
        {
            //Get the element ID that was cached in the callsite
            ISerializable traceData = TraceUtils.GetTraceData(REVIT_TRACE_ID);

            SerializableId id = traceData as SerializableId;
            if (id == null)
                return null; //There was no usable data in the trace cache

            var traceDataUuid = id.StringID;
            return new ElementUUID(traceDataUuid);
        }

        /// <summary>
        /// Get the elementId associated with a UUID, possibly expensive
        /// </summary>
        /// <param name="document"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static ElementId GetIdForUUID(Document document, ElementUUID uuid)
        {
            return document.GetElement(uuid.UUID).Id;
        }


        /// <summary>
        /// Set the element associated with the current operation from trace
        /// null if there is no object, or it's of the wrong type etc.
        /// </summary>
        /// <param name="element">The element to store in trace</param>
        public static void SetElementForTrace(Element element)
        {
            SetElementForTrace(element.Id, new ElementUUID(element.UniqueId));
        }

        /// <summary>
        /// Set the element associated with the current operation from trace
        /// null if there is no object, or it's of the wrong type etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void SetElementForTrace(ElementId elementId, ElementUUID elementUUID)
        {
            if (IsEnabled)
            {
                SerializableId id = new SerializableId();
                id.IntID = elementId.IntegerValue;
                id.StringID = elementUUID.UUID;

                // if we're mutating the current Element id, that means we need to 
                // clean up the old object

                // Set the element ID cached in the callsite
                TraceUtils.SetTraceData(REVIT_TRACE_ID, id);
            }
        }

        /// <summary>
        /// Get the element associated with the current operation from trace
        /// null if there is no object, or it's of the wrong type etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetElementFromTrace<T>(Document document)
            where T : Autodesk.Revit.DB.Element
        {
            var elementUUID = GetElementUUIDFromTrace(document);

            if (elementUUID == null)
                return null;

            T ret;

            if (Elements.ElementUtils.TryGetElement(document, elementUUID.UUID, out ret))
                return ret;
            else
                return null;
        }

        /// <summary>
        /// Delete a possibly outdated Revit Element and set new element for trace.  
        /// This method should be called if the element could not be mutated on a 
        /// second run and the old value must be destroyed.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void CleanupAndSetElementForTrace(Document document, Element newElement)
        {
            // if the element id has changed on a subsequent run, that means we
            // couldn't mutate the element - hence we need to delete the old
            // element
            var oldId = GetElementUUIDFromTrace(document);
            if (oldId != null && oldId.UUID != newElement.UniqueId)
            {
                DocumentManager.Instance.DeleteElement(oldId);
            }

            SetElementForTrace(newElement);
        }




    }

}
