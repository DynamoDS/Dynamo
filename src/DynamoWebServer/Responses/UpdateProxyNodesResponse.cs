using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    /// <summary>
    /// Represents a data about proxy nodes in specified workspace
    /// that were updated by uploading their definition
    /// </summary>
    public class UpdateProxyNodesResponse: Response
    {
        /// <summary>
        /// Guid of the workspace that custom nodes belong to.
        /// If it's home workspace the value is empty
        /// </summary>
        public string WorkspaceID { get; set; }
        
        /// <summary>
        /// Guid of the custom node workspace that was loaded
        /// </summary>
        public string CustomNodeID { get; set; }
        
        /// <summary>
        /// Guids of nodes that are not proxy anymore
        /// </summary>
        public IEnumerable<string> NodesIDs { get; set; }
    }
}
