using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    class GetNodeArrayItemsMessage : Message
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// Index from which Flood requests array items
        /// </summary>
        [DataMember]
        public int IndexFrom { get; set; }

        /// <summary>
        /// Number of requested array items 
        /// </summary>
        [DataMember]
        public int Length { get; set; }
    }
}
