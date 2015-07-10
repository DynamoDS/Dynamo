using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Dynamo.Wpf.Extensions
{
    [DataContract]
    internal class ViewExtensionDefinition
    {
        [DataMember]
        public string AssemblyPath { get; set; }

        [DataMember]
        public string TypeName { get; set; }
    }
}
