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

namespace Dynamo.Tests
{
    [TestFixture]
    class SerializationTests : DynamoModelTestBase
    {
        [Test]
        public void SerializationTest()
        {
            var openPath = Path.Combine(TestDirectory, @"core\input_nodes\NumberNodeAndNumberSlider.dyn");
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
                Converters = new[] { new FunctionDescriptorConverter(CurrentDynamoModel.LibraryServices) },
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
