using System.Runtime.Serialization;

namespace DynamoWebServer.Messages
{
    class GeometryMessage : Message
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeID { get; set; }

        public GeometryMessage(string id)
        {
            this.NodeID = id;
        }
    }
}
