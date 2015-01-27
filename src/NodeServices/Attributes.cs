using System;

namespace DynamoServices
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
