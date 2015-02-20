using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ProtoCore.Mirror;

namespace DynamoWebServer.Responses
{
    class ArrayItemsDataResponse: Response
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeId { get; private set; }

        /// <summary>
        /// String representing of the data about input,
        /// output ports, text of specified code block node
        /// </summary>
        [DataMember]
        public string Items { get; private set; }

        /// <summary>
        /// Index from which Dynamo responds array items
        /// </summary>
        [DataMember]
        public int IndexFrom { get; set; }

        public ArrayItemsDataResponse(NodeModel node, int indexFrom, int length)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            NodeId = node.GUID.ToString();

            if (node.CachedValue == null)
            {
                IndexFrom = -1;
                Items = "";
                return;
            }

            if (node.CachedValue.IsCollection)
            {
                var allItems = node.CachedValue.GetElements();
                if (allItems.Count < indexFrom)
                {
                    Items = "";
                    IndexFrom = allItems.Count;
                }
                else
                {
                    Items = JsonConvert.SerializeObject(allItems.Skip(indexFrom).Select(GetValueFromMirrorData).Take(length).ToArray(), jsonSettings);
                    IndexFrom = indexFrom;
                }
            }
        }

        private object GetValueFromMirrorData(MirrorData cachedValue)
        {
            if (cachedValue == null) return "null";

            if (cachedValue.IsCollection)
            {
                return cachedValue.GetElements().Select(GetValueFromMirrorData).ToArray();
            }
            
            if (cachedValue.Data != null)
            {
                return cachedValue.Data.ToString();
            }
            
            if (!cachedValue.IsNull && cachedValue.Class != null)
                return cachedValue.Class.ClassName;

            return "null";
        }
    }
}
