using System.Runtime.Serialization;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class UpdateNodeMessage : Message
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// Parameter name
        /// </summary>
        [DataMember]
        public string ParameterName { get; set; }

        /// <summary>
        /// Parameter value
        /// </summary>
        [DataMember]
        public string ParameterValue { get; set; }

        /// <summary>
        /// Guid of a specified workspace. Empty string for Home workspace
        /// </summary>
        [DataMember]
        public string WorkspaceGuid { get; set; }

    }

}
