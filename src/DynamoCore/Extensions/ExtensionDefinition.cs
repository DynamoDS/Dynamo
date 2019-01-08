using System.Runtime.Serialization;

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
