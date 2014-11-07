using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class SetModelPositionMessage : Message
    {
        #region Class Data Members

        /// <summary>
        /// List of nodes' guids with their positions
        /// </summary>
        [DataMember]
        public IEnumerable<ModelPosition> ModelPositions { get; set; }

        /// <summary>
        /// Guid of a specified workspace. Empty string for Home workspace
        /// </summary>
        [DataMember]
        public string WorkspaceGuid { get; set; }

        /// <summary>
        /// New name for a specified workspace
        /// </summary>
        [DataMember]
        public string WorkspaceName { get; set; }

        #endregion
    }

    [DataContract]
    public class ModelPosition
    {
        /// <summary>
        /// Guid of the specified model
        /// </summary>
        [DataMember]
        public string ModelId { get; set; }

        /// <summary>
        /// X coordinate of the specified model
        /// </summary>
        [DataMember]
        public double X { get; set; }

        /// <summary>
        /// Y coordinate of the specified model
        /// </summary>
        [DataMember]
        public double Y { get; set; }
    }
}
