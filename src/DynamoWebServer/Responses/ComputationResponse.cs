using DynamoWebServer.Messages;
using System.Collections.Generic;

namespace DynamoWebServer.Responses
{
    public class ComputationResponse : Response
    {
        public IEnumerable<ExecutedNode> Nodes { get; private set; }

        public ComputationResponse(IEnumerable<ExecutedNode> nodes)
        {
            this.Nodes = nodes;
        }
    }
}
