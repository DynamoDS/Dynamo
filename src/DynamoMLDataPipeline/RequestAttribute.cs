using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    // Attributes for the data request object.
    internal class RequestAttribute
    {
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        public RequestAttribute(string name, string value, string category = "application", string type = "String")
        {
            Category = category;
            Name = name;
            Value = value;
            Type = type;
        }
    }
}
