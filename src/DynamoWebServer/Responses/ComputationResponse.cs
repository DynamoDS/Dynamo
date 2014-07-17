using System;
using Newtonsoft.Json;

namespace DynamoWebServer.Responses
{
    public class ComputationResponse : Response
    {
        public string Nodes { get; set; }
    }
}
