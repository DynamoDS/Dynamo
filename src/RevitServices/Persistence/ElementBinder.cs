using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;

namespace RevitServices.Persistence
{
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
            

            //TODO(DE-Serialise)

            String traceDataStr = null;

            T ret;

            if (Elements.ElementValidity.TryGetElement(document, traceDataStr, out ret))
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


            //TODO(Serialise the ID into a string)

            ISerializable traceData = null;


            //Set the element ID cached in the callsite
            TraceUtils.SetTraceData(REVIT_TRACE_ID, traceData);
        }


    }

}
