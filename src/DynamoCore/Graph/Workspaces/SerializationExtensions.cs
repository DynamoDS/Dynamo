using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dynamo.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Contains methods for serializing a WorkspaceModel to json.
    /// </summary>
    public static class SerializationExtensions
    {
        private const string NewtonsoftTypeName = "$type";
        private const string ServerTypeName = "ConcreteType";

        private const string IntegerSlider32Bit = "CoreNodeModels.Input.IntegerSlider";
        private const string IntegerSlider64Bit = "CoreNodeModels.Input.IntegerSlider64Bit";

        private const int DefaultDynamoFileSize = 4096;

        private static readonly Regex serverTypeRegex = new Regex(ServerTypeName, RegexOptions.Compiled);
        private static readonly Regex newtonsoftTypeRegex = new Regex(Regex.Escape(NewtonsoftTypeName), RegexOptions.Compiled);

        internal static SerializationBinder Binder { get; } = new SerializationBinder();

        /// <summary>
        /// Save a Workspace to json.
        /// </summary>
        /// <returns>A string representing the serialized WorkspaceModel.</returns>
        internal static string ToJson(this WorkspaceModel workspace, EngineController engine)
        {
            var jobject = ToJsonJObject(workspace, engine);

            var sb = new StringBuilder(DefaultDynamoFileSize);
            using var tw = new StringWriter(sb);
            SerializeJObject(jobject, tw);

            return sb.ToString();
        }

        internal static JObject ToJsonJObject(this WorkspaceModel workspace, EngineController engine)
        {
            var logger = engine != null ? engine.AsLogger() : null;

            var serializer = new JsonSerializer
            {
                SerializationBinder = Binder,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Culture = CultureInfo.InvariantCulture,
                ReferenceResolver = new IdReferenceResolver(),
                NullValueHandling = NullValueHandling.Include,
            };
            serializer.Converters.Add(new ConnectorConverter(logger));
            serializer.Converters.Add(new WorkspaceWriteConverter(engine));
            serializer.Converters.Add(new DummyNodeWriteConverter());
            serializer.Converters.Add(new TypedParameterConverter());
            serializer.Converters.Add(new NodeLibraryDependencyConverter(logger));
            serializer.Converters.Add(new LinterManagerConverter(logger));

            serializer.Error += (sender, args) =>
            {
                args.ErrorContext.Handled = true;
                Console.WriteLine(args.ErrorContext.Error);
            };

            return JObject.FromObject(workspace, serializer);
        }

        internal static void SerializeJObject(JObject workspaceJObject, TextWriter textWriter)
        {
            using (var writer = new TypeReplacerWriter(textWriter))
            {
                workspaceJObject.WriteTo(writer);
            }
        }

        /// <summary>
        /// The <see cref="TypeReplacerWriter"/> replaces outgoing property name tokens in the raw json string that
        /// match <see cref="NewtonsoftTypeName"/> with <see cref="ServerTypeName"/> so that the json can be
        /// serialized correctly.
        /// </summary>
        /// <param name="textWriter">The text writer to write the serialized json to</param>
        private class TypeReplacerWriter(TextWriter textWriter) : JsonTextWriter(textWriter)
        {
            public override void WritePropertyName(string name, bool escape)
            {
                base.WritePropertyName(name == NewtonsoftTypeName ? ServerTypeName : name, escape);
            }

            public override void WritePropertyName(string name)
            {
                base.WritePropertyName(name == NewtonsoftTypeName ? ServerTypeName : name);
            }
        }

        /// <summary>
        /// The <see cref="TypeReplacerReader"/> replaces incoming property name tokens in the raw json string that
        /// match <see cref="ServerTypeName"/> with <see cref="NewtonsoftTypeName"/> so that the json can be
        /// deserialized correctly.
        /// </summary>
        /// <param name="reader">The text reader to read the json string from</param>
        internal class TypeReplacerReader(TextReader reader) : JsonTextReader(reader)
        {
            public override bool Read()
            {
                var hasToken = base.Read();

                if (hasToken && base.TokenType == JsonToken.PropertyName && base.Value != null && base.Value.Equals(ServerTypeName))
                {
                    SetToken(JsonToken.PropertyName, NewtonsoftTypeName);
                }

                return hasToken;
            }
        }

        /// <summary>
        /// The <see cref="SerializationBinder"/> is used to used to replace the given json type with another while
        /// serializing or deserializing. In our case, it is used for the integer sliders (32 vs. 64) bit.
        /// If other types need to be switched out in the future, this is more flexible than doing string
        /// replacements on the raw json.
        /// </summary>
        internal class SerializationBinder : ISerializationBinder
        {
            private static readonly DefaultSerializationBinder _defaultBinder = new();

            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;

                // serialize the serializedType (64 bit) to 32 bit
                if (typeName == IntegerSlider64Bit)
                {
                    typeName = IntegerSlider32Bit;
                }
            }

            public Type BindToType(string assemblyName, string typeName)
            {
                // deserialize the json type string (32 bit) to 64 bit
                return typeName == IntegerSlider32Bit
                    ? _defaultBinder.BindToType(assemblyName, IntegerSlider64Bit)
                    : _defaultBinder.BindToType(assemblyName, typeName);
            }
        }

        /// <summary>
        /// Strips $type references from the generated json, replacing them with 
        /// type names matching those expected by the server.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <param name="fromServer">A flag indicating whether this json is coming from the server, and thus
        /// needs to be converted back to its Json.net friendly format.</param>
        /// <returns></returns>
        internal static string ReplaceTypeDeclarations(string json, bool fromServer = false)
        {
            return fromServer
                ? serverTypeRegex.Replace(json, NewtonsoftTypeName)
                : newtonsoftTypeRegex.Replace(json, ServerTypeName);
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string DeserializeIntegerSliderTo64BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider64Bit");
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string SerializeIntegerSliderAs32BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider64Bit\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider");
        }        
    }
}
