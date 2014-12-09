using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    /// <summary>
    /// This response is being sent from Dynamo Server to the connecting client when
    /// a custom node definition has been uploaded to the server. Responding to this 
    /// response, the client will then update proxy nodes that are found in workspace,
    /// turning them into actual custom node instances.
    /// </summary>
    public class UpdateProxyNodesResponse: Response
    {
        /// <summary>
        /// Guid of the workspace that custom nodes belong to.
        /// If it's home workspace the value is empty
        /// </summary>
        public string WorkspaceId { get; private set; }
        
        /// <summary>
        /// Guid of the custom node workspace that was loaded
        /// </summary>
        public string CustomNodeId { get; private set; }
        
        /// <summary>
        /// Guids of nodes that are not proxy anymore
        /// </summary>
        public IEnumerable<string> NodesIds { get; private set; }

        public UpdateProxyNodesResponse(string workspaceId, string customNodeId,
            IEnumerable<string> nodesIds)
        {
            this.WorkspaceId = workspaceId;
            this.CustomNodeId = customNodeId;
            this.NodesIds = nodesIds;
        }
    }
}
