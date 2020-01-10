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

        /// <summary>
        /// Is set to true if the ViewExtensionDefinition is located in a directory that requires certificate verification of its entry point dll.
        /// </summary>
        internal bool RequiresSignedEntryPoint { get; set; } = false;
    }
}
