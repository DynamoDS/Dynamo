using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Responses
{
    /// <summary>
    /// Response with data to redraw a code block node
    /// </summary>
    public class CodeBlockDataResponse : Response
    {
        /// <summary>
        /// Guid of the workspace that contains specified code
        /// block node. Empty string for Home workspace
        /// </summary>
        [DataMember]
        public string WorkspaceGuid { get; set; }
        
        /// <summary>
        /// Guid of the specified code block node
        /// </summary>
        [DataMember]
        public string NodeId { get; private set; }

        /// <summary>
        /// String representing of the data about input,
        /// output ports, text of specified code block node
        /// </summary>
        [DataMember]
        public string Data { get; private set; }

        public CodeBlockDataResponse(string wsGuid, string nodeGuid, string data)
        {
            WorkspaceGuid = wsGuid;
            NodeId = nodeGuid;
            Data = data;
        }
    }
}
