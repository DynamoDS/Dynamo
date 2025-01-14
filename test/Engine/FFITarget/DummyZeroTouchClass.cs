using Dynamo.Graph.Nodes;
using System.Text;
using System;
using System.Collections.Generic;

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

        public static Dictionary<string, object> DecodeToByteDictionary()
        {
            // Example base64 encoded string
            string encodedString = "SGVsbG8gV29ybGQh";

            // Decode the base64 encoded string
            byte[] decodedBytes = Convert.FromBase64String(encodedString);
            var result = new Dictionary<string, object>();
            result.Add("decodedBytes", decodedBytes);

            return result;
        }

        public static Dictionary<string, object> DecodeToUint32Dictionary()
        {
            // Example base64 encoded string
            var uintArray = new uint[] { 1, 2, 3, 4, 5 };
            var result = new Dictionary<string, object>();
            result.Add("uint32List", uintArray);

            return result;
        }

        public static Dictionary<string, object> DecodeToUint64Dictionary()
        {
            // Example base64 encoded string
            var uintArray = new ulong[] { 1, 2, 3, 4, 5 };
            var result = new Dictionary<string, object>();
            result.Add("uint64List", uintArray);

            return result;
        }
    }
}
