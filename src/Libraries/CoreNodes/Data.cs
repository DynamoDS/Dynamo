using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;

namespace DSCore
{
    public static class Data
    {
        /// <summary>
        /// Parse converts an arbitrary JSON string to a value. It is the opposite of JSON.Stringify.
        /// </summary>
        /// <param name="json">A JSON string</param>
        /// <returns name="result">The result type depends on the content of the input string. The result type can be a primitive value (e.g. string, boolean, double), a List, or a Dictionary.</returns>
        public static object ParseJSON(string json)
        {
            return ToNative(JToken.Parse(json));
        }

        /// <summary>
        /// Parse implementation for converting JToken types to native .NET objects.
        /// </summary>
        /// <param name="token">JToken to parse to N</param>
        /// <returns></returns>
        private static object ToNative(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = token as JObject;

                    var dynObj = DynamoJObjectToNative(obj);
                    if(dynObj != null)
                    {
                        return dynObj;
                    }
     
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
        /// Parse implementation for converting JObject types to specific Dynamo objects (ie Geometry, Color, Images, etc) 
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static object DynamoJObjectToNative(JObject jObject)
        {
            if (jObject.ContainsKey("$typeid"))
            {
                var typeid = jObject["$typeid"].ToString();

                switch (typeid)
                {
                    //autodesk.math to abstract ProtoGeometry types
                    case "autodesk.math:vector3d-1.0.0":
                        return Vector.FromJson(jObject.ToString());

                    case "autodesk.math:matrix44d-1.0.0":
                        return CoordinateSystem.FromJson(jObject.ToString());

                    //autodesk.geometry to abstract ProtoGeometry types
                    case "autodesk.geometry:boundingbox3d-1.0.0":
                        return BoundingBox.FromJson(jObject.ToString());

                    case "dynamo.geometry:mesh-1.0.0":
                        return Mesh.FromJson(jObject.ToString());

                    //types supported by Goemetry.FromJson
                    case "autodesk.math:point3d-1.0.0":
                    case "dynamo.geometry:sab-1.0.0":
                    case "dynamo.geometry:tsm-1.0.0":
                    case "dynamo.geometry:rectangle-1.0.0":
                    case "dynamo.geometry:cuboid-1.0.0":
                    case "dynamo.geometry:solid-1.0.0":
                    case string geoId when geoId.Contains("autodesk.geometry"):
                        return Geometry.FromJson(jObject.ToString());

                    //Dynamo types
                    case "dynamo.graphics:color-1.0.0":
                        return Color.ByARGB(
                        (int)jObject["A"],
                        (int)jObject["R"],
                        (int)jObject["G"],
                        (int)jObject["B"]);

                    case "dynamo.graphics:png-1.0.0":
                        jObject.TryGetValue(ImageFormat.Png.ToString(), out var value);

                        if (value != null)
                        {
                            var stream = Convert.FromBase64String(value.ToString());

                            Bitmap bitmap;
                            using (var ms = new MemoryStream(stream))
                                bitmap = new Bitmap(Bitmap.FromStream(ms));

                            return bitmap;
                        }

                        return null;

                    case "dynamo.data:location-1.0.0":
                        return DynamoUnits.Location.ByLatitudeAndLongitude(
                        (double)jObject["Latitude"],
                        (double)jObject["Longitude"],
                        (string)jObject["Name"]);

                    default:
                        return null;
                }
            }

            if (jObject.ContainsKey("typeid"))
            {
                var typeid = jObject["typeid"].ToString();
                if (typeid == "autodesk.soliddef:model-1.0.0")
                {
                    return Geometry.FromSolidDef(jObject.ToString());
                }
            }

            return null;
        }

        /// <summary>
        ///     Stringify converts an arbitrary value or a list of arbitrary values to JSON. Replication can be used to apply the operation over a list, producing a list of JSON strings.
        /// </summary>
        /// <param name="values">A List of values</param>
        /// <returns name="json">A JSON string where primitive types (e.g. double, int, boolean), Lists, and Dictionary's will be turned into the associated JSON type.</returns>
        public static string StringifyJSON([ArbitraryDimensionArrayImport] object values)
        {
            return JsonConvert.SerializeObject(values,
                new JsonConverter[]
                {
                    new DictConverter(),
                    new DesignScriptGeometryConverter(),
                    new ColorConveter(),
                    new LocationConverter(),
                    new ImageConverter()
                });
        }


        #region Converters
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

        private class DesignScriptGeometryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string serializedValue;

                switch(value)
                {
                    case Geometry item:
                        var geoString = item.ToJson();

                        if (!string.IsNullOrEmpty(geoString))
                        {
                            writer.WriteRawValue(geoString);
                            return;
                        }
                        break;
                    case BoundingBox item:
                        writer.WriteRawValue(item.ToJson());
                        return;
                    case CoordinateSystem item:
                        writer.WriteRawValue(item.ToJson());
                        return;
                    case Mesh item:
                        writer.WriteRawValue(item.ToJson());
                        return;
                    case Vector item:
                        writer.WriteRawValue(item.ToJson());
                        return;
                }

                throw new NotSupportedException();
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
                return typeof(DesignScriptEntity).IsAssignableFrom(objectType) && !typeof(Mesh).IsAssignableFrom(objectType) && !typeof(IndexGroup).IsAssignableFrom(objectType);
            }
        }

        private class ColorConveter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var jobject = JObject.FromObject(value);
                jobject.Add("$typeid", "dynamo.graphics:color-1.0.0");

                jobject.WriteTo(writer);
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
                return typeof(DSCore.Color) == objectType;
            }
        }

        private class LocationConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var jobject = JObject.FromObject(value);
                jobject.Add("$typeid", "dynamo.data:location-1.0.0");

                jobject.WriteTo(writer);
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
                return typeof(DynamoUnits.Location) == objectType;
            }
        }

        private class ImageConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var image = value as Bitmap;

                string serializedValue;
                var stream = new MemoryStream();
                image?.Save(stream, ImageFormat.Png);
                serializedValue = Convert.ToBase64String(stream.ToArray());

                writer.WriteStartObject();
                writer.WritePropertyName("$typeid");
                writer.WriteValue("dynamo.graphics:png-1.0.0");
                writer.WritePropertyName(ImageFormat.Png.ToString());
                writer.WriteValue(serializedValue);
                writer.WriteEndObject();
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
                return typeof(Bitmap).IsAssignableFrom(objectType);
            }
        }

        #endregion

        #region Remember node functions

        /// <summary>
        /// Helper function to determine if object can be cached or if it is null, "null" string, or empty list.  
        /// </summary>
        /// <param name="inputObject">Object to check</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool CanObjectBeCached(object inputObject)
        {
            if (inputObject == null
                || (inputObject is string inputString && inputString == "null")
                || (inputObject is ArrayList inputArray && inputArray.Count == 0))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function to handle caching for the Data.Remember node
        /// </summary>
        /// <param name="inputObject">Object to cache</param>
        /// <param name = "cachedJson" >Optional existing cache json</param >
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, object> Remember([ArbitraryDimensionArrayImport] object inputObject, string cachedJson)
        {
            //Handle the case where the node has no inputs or the input value is null
            if (!CanObjectBeCached(inputObject))
            {
                //If a previous cache exists, de-serialize and return
                if (cachedJson != "")
                {
                    object cachedObject = null;
                    try
                    {
                        cachedObject = ParseJSON(cachedJson);
                    }
                    catch
                    {
                        throw new NotSupportedException(Properties.Resources.Exception_Deserialize_Unsupported_Type);
                    }

                    return new Dictionary<string, object>
                    {
                        { ">", cachedObject },
                        { "Cache", cachedJson }
                    };
                }

                //Else pass through the empty inputs and cacheJson
                return new Dictionary<string, object>
                {
                    { ">", inputObject },
                    { "Cache", cachedJson }
                };
            }

            //Try to serialize the inputs and return
            string newCachedJson;
            try
            {
                newCachedJson = StringifyJSON(inputObject);
            }
            catch(Exception ex)
            {
                throw new NotSupportedException(Properties.Resources.Exception_Serialize_Unsupported_Type);
            }
            
            return new Dictionary<string, object>
            {
                { ">", inputObject },
                { "Cache", newCachedJson }
            };
        }

        #endregion
    }
}
