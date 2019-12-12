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

        /// <summary>
        /// Is set to true if the ExtensionDefinition is located in a directory that requires certificate verification of its entry point dll.
        /// </summary>
        internal bool RequiresSignedEntryPoint { get; set; } = false;
    }
}
