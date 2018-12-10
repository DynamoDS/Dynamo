using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSCore
{
    public static class Data
    {
        /// <summary>
        ///     Parse converts an arbitrary JSON string to a value. It is the opposite of JSON.Stringify.
        /// </summary>
        /// <param name="json">A JSON string</param>
        /// <returns name="result">The result type depends on the content of the input string. The result type can be a primitive value (e.g. string, boolean, double), a List, or a Dictionary.</returns>
        public static object ParseJSON(string json)
        {
            return ToNative(JToken.Parse(json));
        }

        private static object ToNative(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = token as JObject;
                    var dict = new Dictionary<string, object>();
                    foreach (var kv in obj)
                    {
                        dict[kv.Key] = ToNative(kv.Value);
                    }
                    return dict;
                case JTokenType.Array:
                    var arr = token as JArray;
                    return arr.Select(ToNative);
                case JTokenType.Null:
                    return null;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.TimeSpan:
                    return (token as JValue).Value;
                case JTokenType.Guid:
                case JTokenType.Uri:
                    return (token as JValue).Value.ToString();
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Stringify converts an arbitrary value or a list of arbitrary values to JSON. Replication can be used to apply the operation over a list, producing a list of JSON strings.
        /// </summary>
        /// <param name="values">A List of values</param>
        /// <returns name="json">A JSON string where primitive types (e.g. double, int, boolean), Lists, and Dictionary's will be turned into the associated JSON type.</returns>
        public static string StringifyJSON([ArbitraryDimensionArrayImport] object values)
        {
            return JsonConvert.SerializeObject(values, new DictConverter());
        }

        /// <summary>
        /// Ensures DesignScript.Builtin.Dictionary's, which deliberately don't implement IDictionary, are transformed into JSON objects.
        /// </summary>
        private class DictConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                object obj;
                if (value is DesignScript.Builtin.Dictionary)
                {
                    var dict = value as DesignScript.Builtin.Dictionary;
                    var rdict = new Dictionary<string, object>();
                    foreach (var key in dict.Keys)
                    {
                        rdict[key] = dict.ValueAtKey(key);
                    }
                    obj = rdict;
                }
                else
                {
                    obj = value;
                }

                serializer.Serialize(writer, obj);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(DesignScript.Builtin.Dictionary);
            }
        }

    }
}
