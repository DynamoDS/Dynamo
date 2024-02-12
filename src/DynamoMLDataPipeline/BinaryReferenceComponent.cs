using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    class BinaryReferenceComponent : Dictionary<string, Dictionary<string, IPropertySet>>
    {
        private string objectId = "autodesk.data:binary.reference.component-1.0.0";
        public string ObjectId { get { return objectId; } }
        public BinaryReferenceComponent(string binaryId)
        {
            var propertyDictionary = new Dictionary<string, IPropertySet>();
            propertyDictionary.Add("String", new StringPropertySet(binaryId));
            propertyDictionary.Add("Uint32", new IntPropertySet());

            this.Add("binary_reference", propertyDictionary);
        }
    }

    class StringPropertySet : IPropertySet
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("revision")]
        public string Revision { get; set; }

        public StringPropertySet(string binaryId, string revision = "v0")
        {
            Id = binaryId;
            Revision = revision;
        }
    }

    class IntPropertySet : IPropertySet
    {
        [JsonProperty("end")]
        public int End { get; set; }
        [JsonProperty("start")]
        public int Start { get; set; }

        public IntPropertySet(int start = 0, int end = 8710)
        {
            End = end;
            Start = start;
        }
    }
    
    interface IPropertySet { }
}
