using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class ExtensionDefinition
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string TypeName { get; set; }
    }
}
