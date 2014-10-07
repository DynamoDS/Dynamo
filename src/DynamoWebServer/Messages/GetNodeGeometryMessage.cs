using System.Runtime.Serialization;

namespace DynamoWebServer.Messages
{
    class GetNodeGeometryMessage : Message
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        public GetNodeGeometryMessage(string id)
        {
            this.NodeId = id;
        }
    }
}
