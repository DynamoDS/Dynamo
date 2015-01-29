using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;
using ProtoCore.Mirror;

namespace DynamoWebServer.Responses
{
    class ArrayItemsDataResponse: Response
    {
        /// <summary>
        /// Guid of the specified code block node
        /// </summary>
        [DataMember]
        public string NodeId { get; private set; }

        /// <summary>
        /// String representing of the data about input,
        /// output ports, text of specified code block node
        /// </summary>
        [DataMember]
        public IEnumerable<string> Items { get; private set; }

        /// <summary>
        /// Index from which Dynamo responds array items
        /// </summary>
        [DataMember]
        public int IndexFrom { get; set; }

        public ArrayItemsDataResponse(NodeModel node, int indexFrom, int length)
        {
            NodeId = node.GUID.ToString();
            if (node.CachedValue.IsCollection)
            {
                //Func<MirrorData, string> wrappedValue = (el) => "\"" + GetValueFromMirrorData(el) + "\"";
                var allItems = node.CachedValue.GetElements();
                if (allItems.Count < indexFrom)
                {
                    Items = new string[0];
                    IndexFrom = allItems.Count;
                }
                else
                {
                    Items = allItems.Skip(indexFrom).Select(e => GetValueFromMirrorData(e)).Take(length);
                    IndexFrom = indexFrom;
                }
            }
            else
            {
                indexFrom = -1;
            }
        }

        private string GetValueFromMirrorData(MirrorData cachedValue)
        {
            if (cachedValue.IsCollection)
            {
                Func<MirrorData, string> wrappedValue = (el) => "\"" + GetValueFromMirrorData(el) + "\"";

                var elements = cachedValue.GetElements().ConvertAll(e => wrappedValue(e));

                return "[" + string.Join(", ", elements) + "]";
            }
            else if (cachedValue.Data != null)
            {
                return cachedValue.Data.ToString();
            }

            return "null";
        }
    }
}
