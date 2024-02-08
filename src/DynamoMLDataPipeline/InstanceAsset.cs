using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace DynamoMLDataPipeline
{

    class InstanceAsset
    {
        public InstanceAsset(ParameterComponent parameterComponent, BaseComponent baseComponent, BinaryReferenceComponent binaryRefComponent, string operation)
        {
            Components = new Dictionary<string, Dictionary<string, dynamic>>();
              
            var items = new Dictionary<string, dynamic>();
            items.Add(baseComponent.ObjectId, baseComponent);
            items.Add(binaryRefComponent.ObjectId, binaryRefComponent);
            items.Add(parameterComponent.ObjectId, parameterComponent);

            Components.Add(operation, items);

            Type = "autodesk.design:assets.instance-1.0.0";
            Id = Guid.NewGuid().ToString("N").ToUpper();
        }
        [JsonProperty("components")]
        public Dictionary<string, Dictionary<string, dynamic>> Components { get; set; }

        [JsonProperty("customId")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public interface IComponent { }
}
