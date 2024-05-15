using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.Session;
using DynamoUnits;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    if (dynObj != null)
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

                    //types supported by Geometry.FromJson
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
                        try
                        {
                            return Color.ByARGB(
                                (int)jObject["A"],
                                (int)jObject["R"],
                                (int)jObject["G"],
                                (int)jObject["B"]);
                        }
                        catch {
                            throw new FormatException(string.Format(Properties.Resources.Exception_Deserialize_Bad_Format, typeof(Color).FullName));
                        }

#if _WINDOWS
                    case "dynamo.graphics:png-1.0.0":

                        jObject.TryGetValue(ImageFormat.Png.ToString(), out var value);

                        if (value != null)
                        {
                            try
                            {
                                var stream = Convert.FromBase64String(value.ToString());

                                Bitmap bitmap;
                                using (var ms = new MemoryStream(stream))
                                    bitmap = new Bitmap(Bitmap.FromStream(ms));

                                return bitmap;
                            }
                            catch {
                                //Pass through to the next throw
                            }
                        }

                        throw new FormatException(string.Format(Properties.Resources.Exception_Deserialize_Bad_Format, "dynamo.graphics:png-1.0.0"));
#else
                        return null;
#endif
                    case "dynamo.data:location-1.0.0":
                        try
                        {
                            return DynamoUnits.Location.ByLatitudeAndLongitude(
                            (double)jObject["Latitude"],
                            (double)jObject["Longitude"],
                            (string)jObject["Name"]);
                        }
                        catch
                        {
                            throw new FormatException(string.Format(Properties.Resources.Exception_Deserialize_Bad_Format, typeof(DynamoUnits.Location).FullName));
                        }

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
            var settings = new JsonSerializerSettings()
            {
                Converters =
                [
                    new DictConverter(),
                    new DesignScriptGeometryConverter(),
                    new ColorConveter(),
                    new LocationConverter(),
#if _WINDOWS
                    new PNGImageConverter(),
#endif
                ]
            };

            StringBuilder sb = new(256);
            using StringWriter writer = new(sb, CultureInfo.InvariantCulture);
            using MaxDepthJsonTextWriter jsonWriter = new(writer);
            JsonSerializer.Create(settings).Serialize(jsonWriter, values);
            
            return writer.ToString();
        }

        /// <summary>
        /// Subclass of JsonTextWriter that limits a maximum supported object depth to prevent circular reference crashes when serializing arbitrary .NET objects types.
        /// </summary>
        private class MaxDepthJsonTextWriter : JsonTextWriter
        {
            private readonly int maxDepth = 15;
            private int depth = 0;

            public MaxDepthJsonTextWriter(TextWriter writer) : base(writer) { }

            public override void WriteStartArray()
            {
                base.WriteStartArray();
                depth++;
                CheckDepth();
            }

            public override void WriteEndArray()
            {
                base.WriteEndArray();
                depth--;
                CheckDepth();
            }

            public override void WriteStartObject()
            {
                base.WriteStartObject();
                depth++;
                CheckDepth();
            }

            public override void WriteEndObject()
            {
                base.WriteEndObject();
                depth--;
                CheckDepth();
            }

            private void CheckDepth()
            {
                if (depth > maxDepth)
                {
                    throw new JsonSerializationException(string.Format(Properties.Resources.Exception_Serialize_Depth_Unsupported, depth, maxDepth, Path));
                }
            }
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

                switch (value)
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

                throw new NotSupportedException(Properties.Resources.Exception_Serialize_DesignScript_Unsupported);
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
                return typeof(DesignScriptEntity).IsAssignableFrom(objectType);
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

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        private class PNGImageConverter : JsonConverter
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
                    catch (Exception ex)
                    {
                        dynamoLogger?.Log("Remember failed to deserialize with this exception: " + ex.Message);
                        throw new NotSupportedException(Properties.Resources.Exception_Deserialize_Unsupported_Cache);
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
            catch (Exception ex)
            {
                dynamoLogger?.Log("Remember failed to serialize with this exception: " + ex.Message);
                throw new NotSupportedException(string.Format(Properties.Resources.Exception_Serialize_Unsupported_Type, inputObject.GetType().FullName));
            }

            return new Dictionary<string, object>
            {
                { ">", inputObject },
                { "Cache", newCachedJson }
            };
        }

        internal static DynamoLogger dynamoLogger = ExecutionEvents.ActiveSession?.GetParameterValue(ParameterKeys.Logger) as DynamoLogger;
        #endregion

        #region DefineData Node

        /// <summary>
        /// A class representing a DataType supported by Dynamo
        /// </summary>
        internal class DataNodeDynamoType(Type type, string name = null)
        {
            /// <summary>
            /// The underlying Type
            /// </summary>
            public Type Type { get; private set; } = type;
            /// <summary>
            /// An optional Name to override the Type name (`Number` instead of `long`)
            /// </summary>
            public string Name { get; private set; } = name ?? type.Name;
            /// <summary>
            /// The hierarchical level to be displayed in the UI
            /// </summary>
            public int Level { get; private set; } = 0;
            /// <summary>
            /// If the type is a last child of a hierarchy (for UI purposes)
            /// </summary>
            public bool IsLastChild { get; private set; } = false;
            /// <summary>
            /// The parent of the Type, if any
            /// </summary>
            public DataNodeDynamoType Parent { get; private set; }

            public DataNodeDynamoType(Type type, int level, bool isLastChild = false, string name = null, DataNodeDynamoType parent = null)
            : this(type, name)
            {
                Level = level;
                IsLastChild = isLastChild;
                Parent = parent;
            }
        }

        /// <summary>
        /// A static list for all Dynamo supported data types
        /// </summary>
        /// <returns>The list containing the supported data types</returns>
        internal static readonly ReadOnlyCollection<DataNodeDynamoType> DataNodeDynamoTypeList;

        /// <summary>
        /// Static constructor
        /// </summary>
        static Data()
        {
            var curve = new DataNodeDynamoType(typeof(Curve), 0, false, null, null);
            var polyCurve = new DataNodeDynamoType(typeof(PolyCurve), 1, false, null, curve);
            var polygon = new DataNodeDynamoType(typeof(Polygon), 2, false, null, polyCurve);  // polygon is subtype of polyCurve
            var rectangle = new DataNodeDynamoType(typeof(Autodesk.DesignScript.Geometry.Rectangle), 3, true, null, polyCurve);    // rectangle is subtype of polygon
            var solid = new DataNodeDynamoType(typeof(Solid), 0, false, null, null);
            var cone = new DataNodeDynamoType(typeof(Cone), 1, false, null, solid);    // cone is subtype of solid
            var cylinder = new DataNodeDynamoType(typeof(Cylinder), 2, false, null, cone); // cylinder is subtype of cone 
            var cuboid = new DataNodeDynamoType(typeof(Cuboid), 1, false, null, solid);    // cuboid is subtype of solid
            var sphere = new DataNodeDynamoType(typeof(Sphere), 1, true, null, solid);    // sphere is subtype of solid

            var surface = new DataNodeDynamoType(typeof(Surface), 0, false, null, null);

            var typeList = new List<DataNodeDynamoType>
            {
                new(typeof(bool)),
                new(typeof(BoundingBox)),
                new(typeof(CoordinateSystem)),
                curve,
                new(typeof(Arc), 1, false, null, curve),
                new(typeof(Circle), 1, false, null, curve),
                new(typeof(Ellipse), 1, false, null, curve),
                new(typeof(EllipseArc), 1, false, null, curve),
                new(typeof(Helix), 1, false, null, curve),
                new(typeof(Line), 1, false, null, curve),
                new(typeof(NurbsCurve), 1, false, null, curve),
                polyCurve,
                polygon,
                rectangle,
                new(typeof(System.DateTime)),
                new(typeof(double), "Number"),
                new(typeof(long), "Integer"),
                new(typeof(Location)),
                new(typeof(Mesh)),
                new(typeof(Plane)),
                new(typeof(Autodesk.DesignScript.Geometry.Point)),
                solid,
                cone,
                cylinder,
                cuboid,
                sphere,
                new(typeof(string)),
                surface,
                new(typeof(NurbsSurface), 1, false, null, surface),
                new(typeof(PolySurface), 1, true, null, surface),
                new(typeof(System.TimeSpan)),
                new(typeof(UV)),
                new(typeof(Vector))
            };

            DataNodeDynamoTypeList = new ReadOnlyCollection<DataNodeDynamoType>(typeList);
        }

        /// <summary>
        /// This is the function used by AST
        /// Handles some of the the node logic while performing the validation
        /// </summary>
        /// <param name="inputValue">Upstream input value</param>
        /// <param name="typeString">The Type as string (it would be better to pass an object of type 'Type' for direct type comparison)</param>
        /// <param name="isList">If the input is of type `ArrayList`</param>
        /// <param name="isAutoMode">If the node is in Auto mode</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, object> IsSupportedDataNodeType([ArbitraryDimensionArrayImport] object inputValue,
            string typeString, bool isList, bool isAutoMode, string playerValue)
        {
            if (inputValue == null)
            {
                // Don't raise a warning if the node is unused
                return new Dictionary<string, object>
                {
                    { ">", inputValue },
                    { "Validation", null }
                };
            }

            if (!IsSingleValueOrSingleLevelArrayList(inputValue))
            {
                throw new NotSupportedException(Properties.Resources.DefineDataSupportedInputValueExceptionMessage);
            }

            // If the playerValue is not empty, then we assume it was set by the player.
            // In that case, we need to parse it to get the actual value replace the inputValue.
            if (!string.IsNullOrEmpty(playerValue))
            {
                try
                {
                    inputValue = ParseJSON(playerValue);
                }
                catch (Exception ex)    
                {
                    dynamoLogger?.Log("A Player value failed to deserialize with this exception: " + ex.Message);
                    throw new NotSupportedException(Properties.Resources.Exception_Deserialize_Unsupported_Cache);
                }
            }

            object result;  // Tuple<IsValid: bool, UpdateList: bool, InputType: DataNodeDynamoType>

            // Currently working around passing the type as a string from the node - can be developed further to pass directly the type value
            var type = DataNodeDynamoTypeList.First(x => x.Type.ToString().Equals(typeString));

            if (isAutoMode)
            {
                // If running in AutoMode, then we would propagate the actual Type and List value and validate against them
                // List logic
                var arrayList = inputValue as ArrayList;
                bool assertList = arrayList is not null;
                bool updateList = assertList != isList;

                // Type logic - we try to 'guess' the type of the object
                if (type == null || !IsSupportedDataNodeDynamoType(inputValue, type.Type, assertList))
                {
                    var valueType = assertList ? FindCommonAncestor(arrayList) : inputValue.GetType();
                    if (valueType == null)
                    {
                        // We couldn't find a common ancestor - list containing unsupported or incompatible data types
                        var incompatibleDataTypes = ConcatenateUniqueDataTypes(inputValue);
                        throw new ArgumentException(string.Format(Properties.Resources.DefineDataUnsupportedCombinationOfDataTypesExceptionMessage,
                            incompatibleDataTypes));
                    }
                    var inputType = DataNodeDynamoTypeList.FirstOrDefault(x => x.Type == valueType, null);
                    if (inputType == null)
                    {
                        // We couldn't find a Dynamo data type that fits, so we throw
                        throw new ArgumentException(string.Format(Properties.Resources.DefineDataUnsupportedDataTypeExceptionMessage,
                            valueType.Name));
                    }
                    result = (IsValid: false, UpdateList: updateList, InputType: inputType);
                }
                else
                {
                    result = (IsValid: true, UpdateList: updateList, InputType: type);
                }

                return new Dictionary<string, object>
                {
                    { ">", inputValue },
                    { "Validation", result }
                };
            }
            else
            {
                // If we are in 'Manual mode' then we just validate and throw as needed
                var isSupportedType = IsSupportedDataNodeDynamoType(inputValue, type.Type, isList);
                if (!isSupportedType)
                {
                    throw new ArgumentException(string.Format(Properties.Resources.DefineDataUnexpectedInputExceptionMessage, type.Type.FullName,
                        inputValue.GetType().FullName));
                }
                result = (IsValid: isSupportedType, UpdateList: false, InputType: type);

                return new Dictionary<string, object>
                {
                    { ">", inputValue },
                    { "Validation", result }
                };
            }
        }

        private static string ConcatenateUniqueDataTypes(object inputValue)
        {
            if (inputValue is not ArrayList) return string.Empty;

            var list = inputValue as ArrayList;
            var dataTypeList = GetListFromTypes(list);

            var resultString = string.Join(", ", dataTypeList
                .GroupBy(x => x.Type)
                .Select(g => g.First().Name));

            return resultString;
        }

        /// <summary>
        /// A function to help find the type in case an ArrayList of objects was passed in AutoMode
        /// </summary>
        /// <param name="list">The input value, expected to be of type ArrayList</param>
        /// <returns></returns>
        private static Type FindCommonAncestor(ArrayList list)
        {
            if (list.Count == 1)
                return list[0].GetType(); // Only one node, so it's the "common" ancestor

            var dataTypeList = GetListFromTypes(list);

            return FindClosestCommonAncestor(dataTypeList)?.Type;
        }

        /// <summary>
        /// A helper function returning the lowest-level node from a list of DataNodeDynamoType nodes
        /// </summary>
        /// <param name="nodes">The list of DataNodeDynamoType to evaluate</param>
        /// <returns></returns>
        private static DataNodeDynamoType LikelyAncestor(List<DataNodeDynamoType> nodes)
        {
            var minLevel = nodes.Min(x => x.Level);
            if(minLevel == 0)
            {
                return nodes.First(x => x.Level == 0);  // Already at the root ancestor
            }

            var uniqueNodes = nodes
                .Where(x =>  x.Level == minLevel)
                .GroupBy(x => x.Type)
                .Select(g => g.First())
                .ToList();

            // If we have more than one type of node of the highest level, then recursively find the likely ancestor
            if (uniqueNodes.Count > 1)
            {
                return LikelyAncestor(uniqueNodes.Select(x => x.Parent).ToList());
            }

            // The lowest-level node type
            return uniqueNodes.First();
        }

        /// <summary>
        /// A helper function to try to determine a common ancestor in a list of data types
        /// </summary>
        /// <param name="nodes">The list of DataType nodes to evaluate</param>
        /// <returns></returns>
        private static DataNodeDynamoType FindClosestCommonAncestor(List<DataNodeDynamoType> nodes)
        {
            if (nodes == null || nodes.Count == 0) return null; // No nodes to process

            // Shortcut for homogeneity
            if (nodes.All(node => node.Type == nodes[0].Type))
            {
                return nodes[0];
            }

            // Try to predict the likely ancestor as the single lowest-level node type
            var likelyAncestor = LikelyAncestor(nodes);

            var uniqueLevelNodes = nodes
                .GroupBy(x => x.Level)
                .Select(g => g.First())
                .ToList();

            foreach (var node in uniqueLevelNodes)
            {
                if (node.Type == likelyAncestor.Type) continue;
                likelyAncestor = FindCommonAncestorBetweenTwoNodes(node, likelyAncestor);

                if (likelyAncestor == null) break; 
            }

            return likelyAncestor;
        }

        /// <summary>
        /// Recursive function to try and find a common ancestor between two dynamo types
        /// Climbs up the hierarchical tree of the likelyAncestor until it 
        /// </summary>
        /// <param name="node">Check if this node is derived from the likely ancestor</param>
        /// <param name="likelyAncestor">The likely ancestor that the node should be deriving from</param>
        /// <returns></returns>
        private static DataNodeDynamoType FindCommonAncestorBetweenTwoNodes(DataNodeDynamoType node, DataNodeDynamoType likelyAncestor)
        {
            if (!IsDerivedFrom(node.Type, likelyAncestor.Type))
            {
                if(likelyAncestor.Level == 0) return null;  // we have reached the top of the hierarchical tree, but we haven't found common ancestor

                return FindCommonAncestorBetweenTwoNodes(node, likelyAncestor.Parent);
            }

            return likelyAncestor; 
        }

        /// <summary>
        /// Return a list of DataNodeDynamoTypes from an ArrayList of objects
        /// </summary>
        /// <param name="list">The ArrayList of objects to reformat</param>
        /// <returns></returns>
        private static List<DataNodeDynamoType> GetListFromTypes(ArrayList list)
        {
            List<DataNodeDynamoType> typeList = [];
            foreach (var item in list)
            {
                var matchingType = DataNodeDynamoTypeList.FirstOrDefault(x => x.Type == item.GetType());
                if (matchingType != null)
                {
                    typeList.Add(matchingType);
                }
            }
            return typeList;
        }

        /// <summary>
        /// Check if the input object is a single value or a single-level ArrayList.
        /// </summary>
        /// <param name="obj">The input object to evaluate</param>
        /// <returns></returns>
        private static bool IsSingleValueOrSingleLevelArrayList(object obj)
        {
            if (obj is ArrayList arrayList)
            {
                foreach (var item in arrayList)
                {
                    if (item is IEnumerable && item is not string)
                    {
                        return false;
                    }
                }
                return true;
            }

            // Check if the object is a string, since strings are IEnumerable but usually considered single values
            if (obj is IEnumerable && obj is not string) return false;

            return true;
        }

        /// <summary>
        /// Function to validate input type against supported Dynamo input types
        /// </summary>
        /// <param name="inputValue">The incoming data to validate</param>
        /// <param name="type">The input type provided by the user. It has to match the inputValue type</param>
        /// <param name="isList">The value of this boolean decides if the input is a single object or a list</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        internal static bool IsSupportedDataNodeDynamoType([ArbitraryDimensionArrayImport] object inputValue, Type type, bool isList)
        {
            if (inputValue == null || type == null)
            {
                return false;
            }

            if (!isList)
            {
                if (inputValue is ArrayList) return false;

                return IsItemOfType(inputValue, type);
            }
            else
            {
                if (inputValue is not ArrayList arrayList) return false;

                foreach (var item in arrayList) 
                {
                    if (!IsItemOfType(item, type))
                    {
                        return false; 
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// This method checks if an item is of a required Dynamo DataType
        /// 'IsInstanceOfType' recursively checks for upward inheritance
        /// </summary>
        /// <param name="item">The item to check the data type for</param>
        /// <param name="dataType">The DataType to check against</param>
        /// <returns>A true or false result based on the check validation</returns>
        private static bool IsItemOfType(object item, Type dataType) => dataType.IsInstanceOfType(item);

        /// <summary>
        /// This method checks if a type is derived from a base type
        /// </summary>
        /// <param name="derivedType">The type we want to assert</param>
        /// <param name="baseType">The base type we compare with</param>
        /// <returns></returns>
        private static bool IsDerivedFrom(Type derivedType, Type baseType) => baseType.IsAssignableFrom(derivedType) && derivedType != baseType;

        #endregion
    }
}
