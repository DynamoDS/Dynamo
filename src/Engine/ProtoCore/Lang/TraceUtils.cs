using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace ProtoCore.Lang
{
    [Obsolete("This class is not used anymore, use the same class from Dynamo.Services namespace", false)]
    public static class TraceUtils
    {


// ReSharper disable InconsistentNaming
//Luke: This is deliberately inconsistent, it is not supposed to be in widespread use, to work around a defiency
//in the TLS implementation.
//TODO(Luke): Replace this with an attribute lookup
        internal const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
// ReSharper restore InconsistentNaming
        

        /// <summary>
        /// Returns a list of the keys bound to trace elements
        /// This should be extracted from the attribute on the methods
        /// </summary>
        /// <returns></returns>
        public static List<String> TEMP_GetTraceKeys()
        {
            //TODO:Luke Extract this from RequiresTraceAttribute

            return new List<string>() { __TEMP_REVIT_TRACE_ID };
        }

        /// <summary>
        /// Returns a map of TraceID -> Objects
        /// </summary>
        /// <returns></returns>
        public static Dictionary<String, ISerializable> GetObjectFromTLS()
        {
            Dictionary<String, ISerializable> objs = new Dictionary<String, ISerializable>();

            foreach (String key in TEMP_GetTraceKeys())
            {
                objs.Add(key, 
                    (ISerializable)Thread.GetData(Thread.GetNamedDataSlot(key)));
            }

            return objs;
        }

        /// <summary>
        /// Set the data associated with trace
        /// </summary>
        /// <param name="objs"></param>
        public static void SetObjectToTLS(Dictionary<String, ISerializable> objs)
        {
            foreach (String k in objs.Keys)
            {
                if (objs[k] == null)
                    Thread.FreeNamedDataSlot(k);

                Thread.SetData(Thread.GetNamedDataSlot(k), objs[k]);

            }
        }

        /// <summary>
        /// Clear a specific key
        /// </summary>
        /// <param name="key"></param>
        public static void ClearTLSKey(string key)
        {
            Dictionary<String, ISerializable> objs = new Dictionary<string, ISerializable>();
            objs.Add(key, null);
            SetObjectToTLS(objs);
            
        }


        /// <summary>
        /// Clear the named slots for all the know keys
        /// </summary>
        public static void ClearAllKnownTLSKeys()
        {
            Dictionary<String, ISerializable> objs = new Dictionary<string, ISerializable>();

            foreach (String key in TEMP_GetTraceKeys())
            {
                objs.Add(key, null);
            }

            SetObjectToTLS(objs);

        }

    }
}
﻿