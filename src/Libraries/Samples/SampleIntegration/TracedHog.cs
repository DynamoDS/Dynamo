using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

class TracedHogManager
{
    private static int hogID = 0;

    public static int GetNextUnusedID()
    {
        int next = hogID;
        hogID++;
        return next;
    }

    private static Dictionary<int, TracedHog> hogDictionary = new Dictionary<int, TracedHog>();

    public static TracedHog GetHogByID(int id)
    {
        TracedHog ret;
        hogDictionary.TryGetValue(id, out ret);
        return ret;
    }

    public static void RegisterHogForID(int id, TracedHog hog)
    {
        if (hogDictionary.ContainsKey(id))
        {
            hogDictionary[id] = hog;
        }
        else
        {
            hogDictionary.Add(id, hog);
        }
            
    }

}
    
[Serializable]
public class HogID : ISerializable
{
    public int IntID { get; set; }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("intID", IntID, typeof(int));
    }

    public HogID()
    {
        IntID = int.MinValue;

    }

    /// <summary>
    /// Ctor used by the serialisation engine
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public HogID(SerializationInfo info, StreamingContext context)
    {
        IntID = (int)info.GetValue("intID", typeof(int));

    }
}


[DSNodeServices.RegisterForTrace]
public class TracedHog
{
    //TODO(lukechurch): This really should have been moved into the attribute already
    private const string REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";

    public double X { get; set; }
    public double Y { get; set; }

    public int ID { get; private set; }

    private TracedHog(double x, double y)
        : this(x, y, TracedHogManager.GetNextUnusedID())
    {
    }

    private TracedHog(double x, double y, int id)
    {

        this.X = x;
        this.Y = y;
        this.ID = id;

        TracedHogManager.RegisterHogForID(id, this);
    }

    public static TracedHog ByPoint(double x, double y)
    {
        TracedHog tHog;

        HogID hid = DSNodeServices.TraceUtils.GetTraceData(REVIT_TRACE_ID) as HogID;

        if (hid == null)
        {
            // Trace didn't give us a hog, it's a new one.
            tHog = new TracedHog(x, y);
        }
        else
        {
            tHog = TracedHogManager.GetHogByID(hid.IntID);
        }

        // Set the trace data on the return to be this hog.
        DSNodeServices.TraceUtils.SetTraceData(REVIT_TRACE_ID, new HogID { IntID = tHog.ID });
        return tHog;
    }

    public override string ToString()
    {
        return String.Format("{0}: ({1}, {2})", ID, X, Y);
    }

}
