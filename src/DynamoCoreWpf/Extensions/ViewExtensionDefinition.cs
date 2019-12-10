using System.Runtime.Serialization;

namespace Dynamo.Wpf.Extensions
{
    [DataContract]
    internal class ViewExtensionDefinition
    {
        [DataMember]
        public string AssemblyPath { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        public bool RequiresSignedEntryPoint { get; internal set; } = false;
    }
}
