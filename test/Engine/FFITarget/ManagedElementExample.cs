using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RevitServices.Persistence;

namespace FFITarget
{


    public class WrapperObject : IDisposable
    {
        private int id;

        public int ID { 
            get { return id; } 
            set { 
                id = value;
                var manager = RevitServices.Persistence.ElementIDLifecycleManager<int>.GetInstance();
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
