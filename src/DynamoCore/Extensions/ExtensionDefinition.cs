using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Dynamo.Extensions
{
    [DataContract]
    internal class ExtensionDefinition
    {
        [DataMember]
        public string AssemblyName { get; set; }

        [DataMember]
        public string TypeName { get; set; }
    }
}
