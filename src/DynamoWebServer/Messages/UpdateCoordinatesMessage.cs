using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class UpdateCoordinatesMessage : Message
    {
        #region Class Data Members

        /// <summary>
        /// List of recordable commands that should be executed on server
        /// </summary>
        [DataMember]
        public IEnumerable<NodePosition> NodePositions { get; set; }

        [DataMember]
        public string WorkspaceGuid { get; set; }

        #endregion
    }

    [DataContract]
    public class NodePosition
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// X coordinate of the specified node
        /// </summary>
        [DataMember]
        public double X { get; set; }

        /// <summary>
        /// Y coordinate of the specified node
        /// </summary>
        [DataMember]
        public double Y { get; set; }
    }
}
