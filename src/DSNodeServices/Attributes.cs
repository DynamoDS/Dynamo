using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSNodeServices
{

    //TODO(Luke): Move this to Protointerface

    /// <summary>
    /// Placeholder for Trace registration attribute
    /// </summary>
    public class RegisterForTraceAttribute : Attribute
    {}


    public class Constants
    {
        public const string RevitTraceID = "Revit-Slot-{8C34D023-EF13-43A7-A210-C7EC45731FED}";
    }
}
