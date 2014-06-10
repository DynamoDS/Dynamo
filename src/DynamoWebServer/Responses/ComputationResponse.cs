using System;
using Newtonsoft.Json;

namespace DynamoWebServer.Responses
{
    public class ComputationResponse : Response
    {
        public Guid[] Nodes { get; set; }

        public override string GetResponse()
        {
            return JsonConvert.SerializeObject(Nodes);
        }
    }
}
