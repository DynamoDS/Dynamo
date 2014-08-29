using System.Collections.Generic;
using Dynamo.Interfaces;

namespace DynamoWebServer.Responses
{
    public class NodeCreationDataResponse : Response
    {
        public string WorkspaceID { get; set; }
        public IEnumerable<object> Nodes { get; set; }
        public IEnumerable<object> Connections { get; set; }
        public IEnumerable<object> NodesResult { get; set; }
    }
}
