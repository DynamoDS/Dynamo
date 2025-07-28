using Dynamo.Graph.Nodes;
using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FFITarget
{
    public class DummyZeroTouchClass
    {
        [NodeDescription("Description")]
        public int FunctionWithDescription(int a)
        {
            return 0;
        }

        public int FunctionWithoutDescription(int a)
        {
            return 0;
        }

        public static Dictionary<string, object> PreviewByteDictionary()
        {
            // Example base64 encoded string
            string encodedString = "SGVsbG8gV29ybGQh";

            byte[] decodedBytes = Convert.FromBase64String(encodedString);
            var result = new Dictionary<string, object>();
            result.Add("decodedBytes", decodedBytes);

            return result;
        }

        public static Dictionary<string, object> PreviewUint32Dictionary()
        {
            var uintArray = new uint[] { 1, 2, 3, 4, 5 };
            var result = new Dictionary<string, object>();
            result.Add("uint32List", uintArray);

            return result;
        }

        public static Dictionary<string, object> PreviewUint64Dictionary()
        {
            var uintArray = new ulong[] { 1, 2, 3, 4, 5 };
            var result = new Dictionary<string, object>();
            result.Add("uint64List", uintArray);

            return result;
        }

        public static Dictionary<string, object> PreviewJSONDictionary()
        {
            var json = new JObject();
            json.Add("key1", "value1");
            json.Add("key2", "value2");
            var result = new Dictionary<string, object>();
            result.Add("json", json);

            return result;
        }
    }
}
