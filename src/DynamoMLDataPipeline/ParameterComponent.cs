using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    // Parameters like host and user info for the request schema
    class ParameterComponent: Dictionary<string, Dictionary<string, Parameter>>
    {
        private string objectId = "autodesk.design:components.parameter-1.0.0";
        public string ObjectId { get { return objectId; } }
        public ParameterComponent()
        {
        }

        public void AddParameterFromSchema(dynamic value, Schema schema)
        {
            var parameterEntry = new Dictionary<string, Parameter>();
            var paramererValue = new Parameter(value, schema);
            parameterEntry.Add(schema.TypeId, paramererValue);
            
            this.Add(schema.Constants.FirstOrDefault().Value, parameterEntry);
        }
    }

    class Parameter
    {
        [JsonProperty("parameterValue")]
        public Dictionary<String, Value> ValueEntry { get; set; }
        public Parameter(dynamic value, Schema schema)
        {
            ValueEntry = new Dictionary<String, Value>
            {
                { schema.Type, new Value(value) }
            };
        }
    }

    class Value
    {
        [JsonProperty("value")]
        public dynamic ValueEntry { get; set; }
        public Value(dynamic value)
        {
            ValueEntry = value;
        }
    }
}
