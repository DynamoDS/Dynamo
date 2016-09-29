using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using Dynamo.Graph.Workspaces;
using System;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Engine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json.Converters;
using Dynamo.Graph.Connectors;

namespace Dynamo.Tests
{
    [TestFixture]
    class SerializationTests : DynamoModelTestBase
    {
        [Test]
        public void SerializationTest()
        {
            var openPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            OpenModel(openPath);

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>{
                    new FunctionDescriptorConverter(CurrentDynamoModel.LibraryServices),
                    new CodeBlockNodeConverter(CurrentDynamoModel.LibraryServices),
                    new ConnectorConverter(), new PortConverter()
                },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var json = JsonConvert.SerializeObject(CurrentDynamoModel.CurrentWorkspace, settings);
            Console.WriteLine(json);

            Assert.IsNotNullOrEmpty(json);

            var ws = JsonConvert.DeserializeObject<HomeWorkspaceModel>(json, settings);
            Assert.NotNull(ws);
            var doubleNode = ws.Nodes.First(n => n is DoubleInput);
            Assert.AreEqual(1, doubleNode.OutPorts.Count);
            Assert.AreEqual(0, doubleNode.InPorts.Count);

            var sliderNode = ws.Nodes.First(n => n is DoubleSlider);
            Assert.AreEqual(1, sliderNode.OutPorts.Count);
            Assert.AreEqual(0, sliderNode.InPorts.Count);

            var funcNode = ws.Nodes.First(n => n is DSFunction);
            Assert.AreEqual(1, funcNode.OutPorts.Count);
            Assert.AreEqual(2, funcNode.InPorts.Count);

            Assert.AreEqual(2,ws.Connectors.Count());
            
            // Set the ws as the current home workspace
            // and try to run it.
            
        }
    }

    /// <summary>
    /// The FunctionDescriptorConverter is responsible for deserializing
    /// and serializing the FunctionDescription property on DSFunction. 
    /// Because a lookup in LibraryServices is required during deserialization,
    /// we use this converter to find the correct FunctionDescriptor, and
    /// call a node constructor which constructs a ZeroTouchNodeController
    /// using the FunctionDescriptor.
    /// </summary>
    public class FunctionDescriptorConverter : JsonConverter
    {
        /// <summary>
        /// A reference to an instance of the LibraryServices class.
        /// This is required to properly setup the function given
        /// the assembly and function name.
        /// </summary>
        private LibraryServices libraryServices;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(FunctionDescriptor));
        }

        public FunctionDescriptorConverter(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var asm = jObject["Assembly"].Value<string>();
            var mangledName = jObject["Name"].Value<string>();

            return string.IsNullOrEmpty(asm) ?
                libraryServices.GetFunctionDescriptor(mangledName) :
                libraryServices.GetFunctionDescriptor(asm, mangledName);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fd = (FunctionDescriptor)value;
            writer.WriteStartObject();
            writer.WritePropertyName("Assembly");
            writer.WriteValue(fd.IsBuiltIn? "": fd.Assembly);
            writer.WritePropertyName("Name");
            writer.WriteValue(fd.MangledName);
            writer.WriteEndObject();
        }
    }

    public class CodeBlockNodeConverter : CustomCreationConverter<CodeBlockNodeModel>
    {
        private LibraryServices libraryServices;

        public CodeBlockNodeConverter(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public override CodeBlockNodeModel Create(Type objectType)
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }

    public class ConnectorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType ==  typeof(ConnectorModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var startId = obj["Start"].Value<string>();
            var endId = obj["End"].Value<string>();

            var startPort = (PortModel)serializer.ReferenceResolver.ResolveReference(serializer.Context, startId);
            var endPort = (PortModel)serializer.ReferenceResolver.ResolveReference(serializer.Context, endId);

            var connectorId = Guid.Parse(obj["Guid"].Value<string>());
            return new ConnectorModel(startPort, endPort, connectorId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var connector = (ConnectorModel)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Start");
            writer.WriteValue(connector.Start.GUID.ToString());
            writer.WritePropertyName("End");
            writer.WriteValue(connector.End.GUID.ToString());
            writer.WritePropertyName("Guid");
            writer.WriteValue(connector.GUID.ToString());
            writer.WriteEndObject();
        }
    }

    public class PortConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PortModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var displayName = obj["DisplayName"].Value<string>();
            var toolTip = obj["ToolTip"].Value<string>();
            var portTypeStr = obj["PortType"].Value<string>();
            var portType = (PortType)Enum.Parse(typeof(PortType), portTypeStr);
            var owner = obj["Owner"].Value<string>();
            var usingDefaultValue = obj["UsingDefaultValue"].Value<bool>();
            var level = obj["Level"].Value<int>();
            var useLevels = obj["UseLevels"].Value<bool>();
            var shouldKeepListStructure = obj["ShouldKeepListStructure"].Value<bool>();
            var guidStr = obj["Guid"].Value<string>();
            var guid = Guid.Parse(guidStr);
            var port = new PortModel(portType, null, displayName, toolTip);
            port.UsingDefaultValue = usingDefaultValue;
            port.Level = level;
            port.UseLevels = useLevels;
            port.ShouldKeepListStructure = shouldKeepListStructure;
            port.GUID = guid;
            serializer.ReferenceResolver.AddReference(serializer.Context, port.GUID.ToString(), port);
            return port;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var port = (PortModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("DisplayName");
            writer.WriteValue(port.PortName);
            writer.WritePropertyName("ToolTip");
            writer.WriteValue(port.ToolTipContent);
            writer.WritePropertyName("PortType");
            writer.WriteValue(Enum.GetName(typeof(PortType), port.PortType));
            writer.WritePropertyName("Owner");
            writer.WriteValue(port.Owner.GUID.ToString());
            writer.WritePropertyName("UsingDefaultValue");
            writer.WriteValue(port.UsingDefaultValue);
            writer.WritePropertyName("Level");
            writer.WriteValue(port.Level);
            writer.WritePropertyName("UseLevels");
            writer.WriteValue(port.UseLevels);
            writer.WritePropertyName("ShouldKeepListStructure");
            writer.WriteValue(port.ShouldKeepListStructure);
            writer.WritePropertyName("Guid");
            writer.WriteValue(port.GUID.ToString());

            writer.WriteEndObject();
        }
    }
    /// <summary>
    /// The IdReferenceResolver class allows us to use the Guid of
    /// an object as the reference id during serialization.
    /// </summary>
    public class IdReferenceResolver : IReferenceResolver
    {
        private readonly IDictionary<Guid, object> models = new Dictionary<Guid, object>();

        public void AddReference(object context, string reference, object value)
        {
            Guid id = new Guid(reference);
            models[id] = value;
        }

        private static Guid GetGuidPropertyValue(object value)
        {
            // Use reflection to find the Guid or the GUID
            // property on the object.

            var pi = value.GetType().GetProperty("Guid");
            if (pi == null)
            {
                pi = value.GetType().GetProperty("GUID");
            }

            var id = pi == null ? Guid.NewGuid() : (Guid)pi.GetValue(value);
            return id;
        }

        public string GetReference(object context, object value)
        {
            models[GetGuidPropertyValue(value)] = value;

            return GetGuidPropertyValue(value).ToString();
        }

        public bool IsReferenced(object context, object value)
        {
            var id = GetGuidPropertyValue(value);
            return models.ContainsKey(id);
        }

        public object ResolveReference(object context, string reference)
        {
            var id = new Guid(reference);

            object model;
            models.TryGetValue(id, out model);

            return model;
        }
    }
}
