using Dynamo.Engine;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Presets;
using ProtoCore;
using Type = System.Type;

namespace Dynamo.Serialization
{
    /// <summary>
    /// The WorkspaceConverter is used to serialize and deserialize WorkspaceModels.
    /// Construction of a WorkspaceModel requires things like an EngineController,
    /// a NodeFactory, and a Scheduler. These must be supplied at the time of 
    /// construction and should not be serialized.
    /// </summary>
    public class WorkspaceConverter : JsonConverter
    {
        Scheduler.DynamoScheduler scheduler;
        EngineController engine;
        NodeFactory factory;
        bool isTestMode;
        bool verboseLogging;

        public WorkspaceConverter(EngineController engine, 
            Scheduler.DynamoScheduler scheduler, NodeFactory factory, bool isTestMode, bool verboseLogging)
        {
            this.scheduler = scheduler;
            this.engine = engine;
            this.factory = factory;
            this.isTestMode = isTestMode;
            this.verboseLogging = verboseLogging;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(WorkspaceModel).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var isCustomNode = obj["IsCustomNode"].Value<bool>();
            var lastModifiedStr = obj["LastModified"].Value<string>();
            var lastModified = DateTime.Parse(lastModifiedStr);
            var author = obj["LastModifiedBy"].Value<string>();
            var description = obj["Description"].Value<string>();
            var guidStr = obj["Uuid"].Value<string>();
            var guid = Guid.Parse(guidStr);
            var name = obj["Name"].Value<string>();

            // nodes
            var nodes = obj["Nodes"].ToObject<IEnumerable<NodeModel>>(serializer);
            
            // notes
            var notes = obj["Notes"].ToObject<IEnumerable<NoteModel>>(serializer);

            // connectors
            // Although connectors are not used in the construction of the workspace
            // we need to deserialize this collection, so that they connect to their
            // relevant ports.
            var connectors = obj["Connectors"].ToObject<IEnumerable<ConnectorModel>>(serializer);

            // annotations
            var annotations = obj["Annotations"].ToObject<IEnumerable<AnnotationModel>>(serializer);

            var info = new WorkspaceInfo()
            {
                Name = name,
                Description = description,
                RunType = Models.RunType.Automatic
            };

            WorkspaceModel ws;
            if (isCustomNode)
            {
                info.ID = guid.ToString();
                ws = new CustomNodeWorkspaceModel(factory, nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), new ProtoCore.Namespace.ElementResolver(), info);
            }
            else
            {
                ws = new HomeWorkspaceModel(engine, scheduler, factory, 
                    Enumerable.Empty<KeyValuePair<Guid, List<CallSite.RawTraceData>>>(), nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), new ProtoCore.Namespace.ElementResolver(), 
                    info, verboseLogging, isTestMode);
                ws.Guid = guid;
            }

            return ws;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ws = (WorkspaceModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Uuid");
            writer.WriteValue(ws.Guid.ToString());
            writer.WritePropertyName("IsCustomNode");
            writer.WriteValue(value is CustomNodeWorkspaceModel ? true : false);
            if(value is CustomNodeWorkspaceModel)
            {
                writer.WritePropertyName("Category");
                writer.WriteValue(((CustomNodeWorkspaceModel)value).Category);
            }
            writer.WritePropertyName("LastModified");
            writer.WriteValue(ws.LastSaved);
            writer.WritePropertyName("LastModifiedBy");
            writer.WriteValue(ws.Author);
            writer.WritePropertyName("Description");
            writer.WriteValue(ws.Description);
            writer.WritePropertyName("Name");
            writer.WriteValue(ws.Name);

            //nodes
            writer.WritePropertyName("Nodes");
            serializer.Serialize(writer, ws.Nodes);

            //notes
            writer.WritePropertyName("Notes");
            serializer.Serialize(writer, ws.Notes);

            //connectors
            writer.WritePropertyName("Connectors");
            serializer.Serialize(writer, ws.Connectors);

            //annotations
            writer.WritePropertyName("Annotations");
            serializer.Serialize(writer, ws.Annotations);

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// The AnnotationConverter is used to serialize and deserialize AnnotationModels.
    /// The SelectedModels property on AnnotationModel is a list of references
    /// to ModelBase objects. During serialization we want to refer to these objects
    /// by their ids. During deserialization, we use the ReferenceResolver to
    /// find the correct ModelBase instances to reference.
    /// </summary>
    public class AnnotationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AnnotationModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var title = obj["Title"].Value<string>();
            var guid = Guid.Parse(obj["Uuid"].Value<string>());

            // This is a collection of string Guids, which
            // should be accessible in the ReferenceResolver.
            var models = obj["SelectedModels"].Values<JValue>();

            var existing = models.Select(m => serializer.ReferenceResolver.ResolveReference(serializer.Context, m.Value<string>()));

            var nodes = existing.Where(m => typeof(NodeModel).IsAssignableFrom(m.GetType())).Cast<NodeModel>();
            var notes = existing.Where(m => typeof(NoteModel).IsAssignableFrom(m.GetType())).Cast<NoteModel>();

            var anno = new AnnotationModel(nodes, notes);
            anno.AnnotationText = title;
            anno.GUID = guid;

            return anno;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var anno = (AnnotationModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Title");
            writer.WriteValue(anno.AnnotationText);
            writer.WritePropertyName("SelectedModels");
            writer.WriteStartArray();
            foreach (var m in anno.SelectedModels)
            {
                writer.WriteValue(m.GUID.ToString());
            }
            writer.WriteEndArray();
            writer.WritePropertyName("Uuid");
            writer.WriteValue(anno.GUID.ToString());

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// The FunctionDescriptorConverter is used to serialize and deserialize
    /// the FunctionDescription property on DSFunction. Because a lookup in 
    /// LibraryServices is required during deserialization,
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
            writer.WriteValue(fd.IsBuiltIn ? "" : fd.Assembly);
            writer.WritePropertyName("Name");
            writer.WriteValue(fd.MangledName);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// The CodeBlockNodeConverter is used to serialize and deserialize CodeBlockNodeModels. 
    /// CodeBlockNodeModel requires an instance of LibraryServices for construction.
    /// </summary>
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

    /// <summary>
    /// The ConnectorConverter is used to serialize and deserialize ConnectorModels.
    /// The Start and End of a ConnectorModel are references to PortModels, but
    /// we want the serialized representation of a Connector to reference these 
    /// ports by Id. This converter resolves the reference to the correct NodeModel
    /// instance by id, and constructs the ConnectorModel.
    /// </summary>
    public class ConnectorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ConnectorModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var startId = obj["Start"].Value<string>();
            var endId = obj["End"].Value<string>();

            var startPort = (PortModel)serializer.ReferenceResolver.ResolveReference(serializer.Context, startId);
            var endPort = (PortModel)serializer.ReferenceResolver.ResolveReference(serializer.Context, endId);

            var connectorId = Guid.Parse(obj["Uuid"].Value<string>());
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
            writer.WritePropertyName("Uuid");
            writer.WriteValue(connector.GUID.ToString());
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
