using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    class BaseComponent : Dictionary<string, Dictionary<string, ObjectInfo>>
    {
        private string objectId = "autodesk.design:components.base-1.0.0";
        public BaseComponent(string name)
        {
            var objectInfo = new ObjectInfo(name);
            var item = new Dictionary<string, ObjectInfo>
            {
                { "String", objectInfo }
            };
            this.Add("objectInfo", item);
        }

        public string ObjectId { get { return objectId; } }
    }

    class ObjectInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public ObjectInfo(string name)
        {
            Name = name;
        }
    }
}
