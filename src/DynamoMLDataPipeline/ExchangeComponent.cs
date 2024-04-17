using System.Collections.Generic;
using Newtonsoft.Json;


namespace DynamoMLDataPipeline
{
    internal class ExchangeComponent
    {
        private readonly string type = "autodesk.data:exchange.space-1.0.0";

        public ExchangeComponent(List<RequestAttribute> attributes = null)
        {
            Attributes = attributes;
            Components = new Dictionary<string, Contract>
            {
                { "insert", new Contract() }
            };
        }
        [JsonProperty("type")]
        public string Type { get { return type; } }

        [JsonProperty("components")]
        public Dictionary<string, Contract> Components { get; set; }

        [JsonProperty("attributes")]
        public List<RequestAttribute> Attributes { get; set; }
    }

    internal class Contract : Dictionary<string, Dictionary<string, Dictionary<string, string>>>
    {
        private readonly string type = "autodesk.data:exchange.contract.dynamo-1.0.0";

        public Contract()
        {
            var contractContent = new Dictionary<string, Dictionary<string, string>>
            {
                { "contract", new Dictionary<string, string>() }
            };

            Add(type, contractContent);
        }
    }
}
