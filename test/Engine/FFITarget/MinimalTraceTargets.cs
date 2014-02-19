using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FFITarget
{
    public class MinimalTracedClass
    {
        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        private bool wasTraced = false;

        public MinimalTracedClass()
        {
            var retVal = DSNodeServices.TraceUtils.GetTraceData(__TEMP_REVIT_TRACE_ID);

            if (retVal != null)
            {
                wasTraced = true;
            }

            DSNodeServices.TraceUtils.SetTraceData(__TEMP_REVIT_TRACE_ID, new DummyDataHolder());
        }

        public bool WasCreatedWithTrace()
        {
            return wasTraced;
        }


    }

    public class IncrementerTracedClass
    {
        public static void ResetForNextTest()
        {
            nextID = -1;
        }

        public static int nextID = -1;

        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        private bool wasTraced = false;

        public int ID { get; set; }
        public IncrementerTracedClass(int x)
        {
            var retVal = DSNodeServices.TraceUtils.GetTraceData(__TEMP_REVIT_TRACE_ID);

            if (retVal != null)
            {
                wasTraced = true;

                IDHolder idHolder = (IDHolder) retVal;
                ID = idHolder.ID;

            }
            else
            {
                nextID++;
                ID = nextID;
                DSNodeServices.TraceUtils.SetTraceData(__TEMP_REVIT_TRACE_ID, new IDHolder() { ID = nextID });
            }


            
        }
        public bool WasCreatedWithTrace()
        {
            return wasTraced;
        }

    }


    internal class IDHolder : ISerializable
    {
        public int ID = int.MinValue;

        public IDHolder()
        {
               
        }

        public IDHolder(SerializationInfo info, StreamingContext context)
        {
            ID = (int) info.GetValue("intID", typeof (int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("intID", ID);
        }
    }


    internal class DummyDataHolder : ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

}
