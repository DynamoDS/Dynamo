using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    // Schema for the parameters used in the request.
    class StringParameterSchema : Schema
    {
        public StringParameterSchema(string value, string schemaNamespaceId, string schemaId)
        {
            var constant = new Constant(value);
            Constants = new List<Constant>
            {
                constant
            };

            Parent = new List<string>
            {
                "autodesk.parameter:parameter.string-3.0.0"
            };

            TypeId = $"exchange.parameter.{schemaNamespaceId}:{schemaId}-1.0.0";
            Type = "String";
        }
    }


    class Schema
    {
        [JsonProperty("constants")]
        public List<Constant> Constants { get; set; }

        [JsonProperty("inherits")]
        public List<string> Parent { get; set; }

        [JsonProperty("typeid")]
        public string TypeId { get; set; }

        [JsonIgnore]
        public string Type { get; set; }

    }

    class Constant
    {
        public Constant(string value)
        {
            Id = "name";
            Value = value;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
