using System.Collections.Generic;

namespace DynamoWebServer.Responses
{
    public class ComputationResponse : Response
    {
        public IEnumerable<object> Nodes { get; set; }
    }
}
