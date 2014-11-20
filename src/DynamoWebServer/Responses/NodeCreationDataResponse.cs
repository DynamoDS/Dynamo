using System.Collections.Generic;
using Dynamo.Interfaces;

namespace DynamoWebServer.Responses
{
    public class NodeCreationDataResponse : Response
    {
        public string WorkspaceId { get; set; }
        public string WorkspaceName { get; private set; }
        public IEnumerable<object> Nodes { get; private set; }
        public IEnumerable<object> Connections { get; private set; }
        public IEnumerable<object> NodesResult { get; private set; }

        public NodeCreationDataResponse(string workspaceName,
            IEnumerable<object> nodes, 
            IEnumerable<object> connections,
            IEnumerable<object> nodesResult)
        {
            this.WorkspaceName = workspaceName;
            this.Nodes = nodes;
            this.Connections = connections;
            this.NodesResult = nodesResult;
        }
    }
}
