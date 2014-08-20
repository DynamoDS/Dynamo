using System.Runtime.Serialization;

namespace DynamoWebServer.Messages
{
    class GetNodeGeometryMessage : Message
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeID { get; set; }

        public GetNodeGeometryMessage(string id)
        {
            this.NodeID = id;
        }
    }
}
