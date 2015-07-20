using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Dynamo.Extensions
{
    [DataContract]
    public class ExtensionDefinition
    {
        [DataMember]
        public string AssemblyPath { get; set; }

        [DataMember]
        public string TypeName { get; set; }
    }
}
