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


    public sealed class ShortNameAttribute : Attribute
    {
        
        // This is a positional argument
        public ShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }

        public string ShortName { get; private set; }

    }

}
